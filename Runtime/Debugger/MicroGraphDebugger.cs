#if MICRO_GRAPH_DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图调试器
    /// </summary>
    public static class MicroGraphDebugger
    {
        static MicroGraphDebugger()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload -= s_beforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += s_beforeAssemblyReload;
#endif
        }

        /// <summary>
        /// 心跳数据
        /// </summary>
        private static byte[] HEARTBEAT_DATA = new byte[] { 1 };
        private static Debugger _instance;

        private static Socket _tcpSocket;
        /// <summary>
        /// 客户端连接peer
        /// </summary>
        private static List<ClientPeer> _clientPeers = new List<ClientPeer>(16);

        /// <summary>
        /// 是否在监听
        /// </summary>
        private static bool _isListener = false;
        public static bool IsListener => _isListener;

        /// <summary>
        /// 当前的监听的端口号
        /// </summary>
        private static int _port;

        /// <summary>
        /// 发送消息间隔
        /// </summary>
        private static TimeSpan _sendTimeInterval = new TimeSpan(0, 0, 0, 2);

        private const int FALSE = 0;
        private const int TRUE = 1;
        private static int _valueLock = 0;
        private static Queue<Action> updateActions = new Queue<Action>();
        private static Queue<Action> threadActions = new Queue<Action>();
        private static TcpListener _tcpListener;
        private static CancellationTokenSource _tcpTokenSource;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const float SEND_INTERVAL = 0.2f;
        private static float _lastSendTime = 0;

        /// <summary>
        /// 开始调试
        /// </summary>
        public static void StartDebugger(int port = 65500)
        {
            if (_isListener || !Application.isPlaying)
                return;
            if (port <= 0 || port >= 65535)
            {
                MicroGraphLogger.LogError("调试端口超过限制，请传入0-65535的数字");
                return;
            }
            _isListener = true;
            if (_instance == null)
            {
                GameObject obj = new GameObject("MicroGraphDebugger");
                GameObject.DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<Debugger>();
            }
#if UNITY_EDITOR
            EditorApplication.update -= s_update;
            EditorApplication.update += s_update;
#endif
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Server.ReceiveTimeout = 10000;
            _tcpListener.Server.SendTimeout = 10000;
            _tcpTokenSource = new CancellationTokenSource();
            MicroGraphLogger.LogWarning($"调试已开启等待连接，Ip: {GetIp()}, Port: {_port}");
            m_beginAccept(_tcpTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// 停止调试
        /// </summary>
        public static void StopDebugger()
        {
            if (!_isListener)
                return;
            _isListener = false;
            _tcpTokenSource.Cancel(); // 取消任何正在进行的操作
            _tcpTokenSource.Dispose();
#pragma warning disable CS0168
            foreach (var item in _clientPeers)
            {
                try
                {
                    item.tcpClient?.Close();
                }
                catch (Exception _)
                {
                }
            }
            if (_tcpListener != null)
            {
                // 关闭服务端Socket
                _tcpListener.Stop();
            }
            MicroGraphLogger.LogWarning($"调试已关闭");
        }

        /// <summary>
        /// 发送协议
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void Send<T>(T data) where T : INetData
        {
            m_checkData(data);
        }
        /// <summary>
        /// 添加微图节点数据
        /// </summary>
        /// <param name="data"></param>
        public static void AddGraphNodeData(DebuggerNodeData data)
        {
            string onlyKey = data.microGraphId + "_" + data.runtimeName;
            #region 保存修改信息
            if (!DebuggerCacheData.modifyGraphDataDict.TryGetValue(onlyKey, out var temp))
            {
                temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.modifyGraphDataDict.Add(onlyKey, temp);
                DebuggerCacheData.waitSendQueue.Add(temp);
            }
            temp.AddNodeData(data);
            #endregion

            #region 保存所有信息
            if (!DebuggerCacheData.graphDataDict.TryGetValue(onlyKey, out temp))
            {
                temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.graphDataDict.Add(onlyKey, temp);
            }
            temp.AddNodeData(data);
            #endregion
            DebuggerNodeData.Release(data);
        }
        /// <summary>
        /// 添加微图变量数据
        /// </summary>
        /// <param name="data"></param>
        public static void AddGraphVarData(DebuggerVarData data)
        {
            string onlyKey = data.microGraphId + "_" + data.runtimeName;

            #region 保存修改信息
            if (!DebuggerCacheData.modifyGraphDataDict.TryGetValue(onlyKey, out var temp))
            {
                temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.modifyGraphDataDict.Add(onlyKey, temp);
                DebuggerCacheData.waitSendQueue.Add(temp);
            }
            temp.AddVarData(data);
            #endregion

            #region 保存所有信息
            if (!DebuggerCacheData.graphDataDict.TryGetValue(onlyKey, out temp))
            {
                temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.graphDataDict.Add(onlyKey, temp);
            }
            temp.AddVarData(data);
            #endregion
            DebuggerVarData.Release(data);
        }
        /// <summary>
        /// 发送协议
        /// </summary>
        /// <param name="type"></param>
        /// <param name="datas"></param>
        private static async Task SendData(byte[] bytes)
        {
            if (!_isListener)
                return;
            try
            {
                foreach (var item in _clientPeers)
                {
                    if (!item.tcpClient.Connected)
                        continue;
                    try
                    {
                        await item.netStream?.WriteAsync(bytes, 0, bytes.Length, _tcpTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"网络消息发送失败:{ex.Message}");
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 发送协议
        /// </summary>
        /// <param name="type"></param>
        /// <param name="datas"></param>
        private static async Task SpecifySend(ClientPeer peer, MessageType type, byte[] datas)
        {
            if (!_isListener)
                return;
            try
            {
                if (!peer.tcpClient.Connected)
                    return;
                NetMessagePackage package = NetMessagePackage.CreateSendMessagePackage(type, datas);
                byte[] sendData = package.GetMessageData();
                await peer.netStream?.WriteAsync(sendData, 0, sendData.Length, _tcpTokenSource.Token);
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError($"网络消息发送失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 开始接受客户端
        /// </summary>
        private static async Task m_beginAccept(CancellationToken cancellationToken)
        {
            try
            {
                _tcpListener.Start();
            }
            catch (Exception e)
            {
                MicroGraphLogger.LogError($"网络端口被占用，请重启电脑！");
                _isListener = false;
                return;
            }

            try
            {
                while (_isListener && !cancellationToken.IsCancellationRequested)
                {
                    TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                    if (!_isListener)
                    {
                        client.Close();
                        _isListener = false;
                        break;
                    }
                    ClientPeer peer = new ClientPeer { tcpClient = client, netStream = client.GetStream() };
                    MicroGraphLogger.LogWarning("客户端进入:" + client.Client.RemoteEndPoint.ToString());
                    _clientPeers.Add(peer);
                    //首次接入
                    foreach (var item in DebuggerCacheData.graphDataDict)
                    {
                        item.Value.start = true;
                        NetMessagePackage package = NetMessagePackage.CreateSendMessagePackage(item.Value);
                        byte[] sendData = package.GetMessageData();
                        await peer.netStream.WriteAsync(sendData, 0, sendData.Length, cancellationToken);
                    }
                    // 处理客户端连接
                    _ = m_receiveClientAsync(peer, cancellationToken); // Fire-and-forget
                }
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError($"Server error: {ex.Message}");
            }
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task m_receiveClientAsync(ClientPeer peer, CancellationToken cancellationToken)
        {

            try
            {
                int bytesRead;
                NetworkStream stream = peer.netStream;
                while (!cancellationToken.IsCancellationRequested)
                {

                    // 读取数据
                    bytesRead = await stream.ReadAsync(peer.buffer, 0, peer.buffer.Length, cancellationToken);

                    if (bytesRead == 0)
                    {
                        // 这里的bytesRead为0，表示客户端已断开连接
                        break;
                    }
                    peer.allBytes.AddRange(peer.buffer[..bytesRead]);
                    if (NetMessagePackage.CheckDataComplete(peer.allBytes))
                    {
                        NetMessagePackage package = NetMessagePackage.CreateReceiveMessagePackage(ref peer.allBytes);
                        s_producerAction(() => m_handleMessage(package, peer));
                    }
                }
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError($"Tcp连接是错误: {ex.Message}");
            }
            finally
            {
                // 确保在异常之后，客户端连接被关闭
                MicroGraphLogger.LogWarning("客户端退出:" + peer.tcpClient.Client.RemoteEndPoint.ToString());
                _clientPeers.Remove(peer);
                peer.tcpClient.Close();
            }
        }

        /// <summary>
        /// 处理网络来的消息
        /// </summary>
        /// <param name="package"></param>
        /// <param name="client"></param>
        private static void m_handleMessage(NetMessagePackage package, ClientPeer client)
        {
            try
            {
                switch (package.Type)
                {
                    case MessageType.Heartbeat:
                        SpecifySend(client, MessageType.Heartbeat, HEARTBEAT_DATA).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 获取本机Ip
        /// </summary>
        /// <returns></returns>
        private static string GetIp()
        {
            // 获取主机名称
            string hostName = Dns.GetHostName();

            // 获取与主机关联的IP地址列表
            IPHostEntry ipHostEntry = Dns.GetHostEntry(hostName);

            // 找出所有 IPv4 地址（忽略IPv6）
            IPAddress ipv4 = ipHostEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
            return ipv4 == null ? "空" : ipv4.ToString();
        }
        private static void m_checkData<T>(T data) where T : INetData
        {
            switch (data)
            {
                case DebuggerGraphData graphData:
                    m_checkGraphData(graphData);
                    break;
                case DebuggerGraphRenameData renameData:
                    m_checkGraphRenameData(renameData);
                    break;
                case DebuggerGraphDeleteData deleteData:
                    m_checkGraphDeleteData(deleteData);
                    break;
                default:
                    MicroGraphLogger.LogError($"暂不支持该类型消息:{data.GetType()}");
                    break;
            }
        }
        private static void m_checkGraphData(DebuggerGraphData data)
        {
            string onlyKey = data.microGraphId + "_" + data.runtimeName;
            #region 保存修改信息
            if (!DebuggerCacheData.modifyGraphDataDict.ContainsKey(onlyKey))
            {
                DebuggerGraphData temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                temp.start = data.start;
                DebuggerCacheData.modifyGraphDataDict.Add(onlyKey, temp);
                DebuggerCacheData.waitSendQueue.Add(temp);
            }
            #endregion

            #region 保存所有信息
            if (!DebuggerCacheData.graphDataDict.ContainsKey(onlyKey))
            {
                DebuggerGraphData temp = DebuggerGraphData.Create();
                temp.runtimeName = data.runtimeName;
                temp.microGraphId = data.microGraphId;
                temp.start = data.start;
                DebuggerCacheData.graphDataDict.Add(onlyKey, temp);
            }
            #endregion
        }
        public static void m_checkGraphRenameData(DebuggerGraphRenameData data)
        {
            string onlyKey = data.microGraphId + "_" + data.oldName;
            #region 保存修改信息
            if (DebuggerCacheData.modifyGraphDataDict.TryGetValue(onlyKey, out var temp))
            {
                temp.runtimeName = data.newName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.modifyGraphDataDict.Remove(onlyKey);
                DebuggerCacheData.modifyGraphDataDict.Add(data.microGraphId + "_" + data.newName, temp);
            }
            #endregion

            #region 保存所有信息
            if (DebuggerCacheData.graphDataDict.TryGetValue(onlyKey, out temp))
            {
                temp.runtimeName = data.newName;
                temp.microGraphId = data.microGraphId;
                DebuggerCacheData.graphDataDict.Remove(onlyKey);
                DebuggerCacheData.graphDataDict.Add(data.microGraphId + "_" + data.newName, temp);
            }
            #endregion
            DebuggerCacheData.waitSendQueue.Add(data);
        }

        public static void m_checkGraphDeleteData(DebuggerGraphDeleteData data)
        {
            string onlyKey = data.microGraphId + "_" + data.runtimeName;
            #region 保存修改信息
            if (DebuggerCacheData.modifyGraphDataDict.TryGetValue(onlyKey, out DebuggerGraphData temp))
            {
                DebuggerCacheData.modifyGraphDataDict.Remove(onlyKey);
                DebuggerCacheData.waitSendQueue.Remove(temp);
                DebuggerGraphData.Release(temp);
            }
            #endregion

            #region 保存所有信息
            if (DebuggerCacheData.graphDataDict.TryGetValue(onlyKey, out temp))
            {
                DebuggerCacheData.graphDataDict.Remove(onlyKey);
                DebuggerGraphData.Release(temp);
            }
            #endregion
            DebuggerCacheData.waitSendQueue.Add(data);
        }

        /// <summary>
        /// 更新
        /// </summary>
        private static void s_update()
        {
        Begin:
            if (Interlocked.CompareExchange(ref _valueLock, 1, 0) == FALSE)
            {
                Queue<Action> tempActions = updateActions;
                updateActions = threadActions;
                threadActions = tempActions;
                threadActions.Clear();
                Interlocked.Exchange(ref _valueLock, 0);
            }
            else
                goto Begin;
            while (updateActions.Count > 0)
            {
                try
                {
                    updateActions.Dequeue()?.Invoke();
                }
                catch (Exception)
                {
                }
            }
            _lastSendTime += Time.deltaTime;
            if (_lastSendTime >= SEND_INTERVAL)
            {
                _lastSendTime = 0;
                int count = DebuggerCacheData.waitSendQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    var data = DebuggerCacheData.waitSendQueue[i];
                    SendData(data.GetNetBytes()).ConfigureAwait(false);
                    if (data is DebuggerGraphData graphData)
                        DebuggerGraphData.Release(graphData);
                }
                DebuggerCacheData.waitSendQueue.Clear();
                DebuggerCacheData.modifyGraphDataDict.Clear();
            }
        }
        /// <summary>
        /// 网络协议进入生产者
        /// </summary>
        /// <param name="action"></param>
        private static void s_producerAction(Action action)
        {
        Begin:
            if (Interlocked.CompareExchange(ref _valueLock, 1, 0) == FALSE)
            {
                threadActions.Enqueue(action);
                Interlocked.Exchange(ref _valueLock, 0);
            }
            else
            {
                Thread.Sleep(1);
                goto Begin;
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        private static void s_exit()
        {
#if UNITY_EDITOR
            EditorApplication.update -= s_update;
#endif
            StopDebugger();
        }

        private class ClientPeer
        {
            public const int BufferSize = 1024;
            public TcpClient tcpClient;
            public NetworkStream netStream;
            public byte[] buffer = new byte[BufferSize];
            public List<byte> allBytes = new List<byte>();
        }
        private static class DebuggerCacheData
        {
            /// <summary>
            /// 图数据字典
            /// </summary>
            internal static Dictionary<string, DebuggerGraphData> graphDataDict = new Dictionary<string, DebuggerGraphData>();
            /// <summary>
            /// 图数据列表
            /// </summary>
            //internal static List<DebuggerGraphData> graphDatas = new List<DebuggerGraphData>();
            /// <summary>
            /// 修改等待发送图数据字典
            /// </summary>
            internal static Dictionary<string, DebuggerGraphData> modifyGraphDataDict = new Dictionary<string, DebuggerGraphData>();
            /// <summary>
            /// 等待发送队列
            /// </summary>
            internal static List<INetData> waitSendQueue = new List<INetData>();
        }
#if UNITY_EDITOR
        /// <summary>
        /// 程序集重新加载前
        /// </summary>
        private static void s_beforeAssemblyReload()
        {
            StopDebugger();
        }
#endif

        private class Debugger : MonoBehaviour
        {
            private void Update()
            {
#if !UNITY_EDITOR
                s_update();
#endif
            }

#if !UNITY_EDITOR && !UNITY_STANDALONE
            private void OnApplicationFocus(bool focus)
            {
                if (!focus)
                {
                    if (!IsListener)
                        return;
                    MicroGraphLogger.Log("应用失去焦点，关闭调试");
                    s_exit();
                }
            }
#endif
            private void OnDestroy()
            {
                s_exit();
            }
        }
    }
}

#endif