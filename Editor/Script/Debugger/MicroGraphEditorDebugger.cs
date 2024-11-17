#if MICRO_GRAPH_DEBUG
using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DebuggerMessageType = MicroGraph.Runtime.MessageType;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 调试Tcp
    /// </summary>
    internal static class MicroGraphEditorDebugger
    {
        public enum TcpConnectState
        {
            Disconnect,
            Connecting,
            Connected,
        }
        /// <summary>
        /// 心跳数据
        /// </summary>
        private static byte[] HEARTBEAT_DATA = new byte[] { 1 };

        private static EditorDebugger _instance;

        private static EditorDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorDebugger();
                }

                return _instance;
            }
        }
        static MicroGraphEditorDebugger()
        {
            MicroGraphUtils.onUpdate -= s_consumerUpdate;
            MicroGraphUtils.onUpdate += s_consumerUpdate;
        }
        private const int FALSE = 0;
        private const int TRUE = 1;
        private static int _valueLock = 0;
        private static Queue<Action> updateActions = new Queue<Action>();
        private static Queue<Action> threadActions = new Queue<Action>();
        private static MicroDebuggerStopwatch _stopwatch = MicroDebuggerStopwatch.StartNew();
        private static TimeSpan _heartTimeInterval = new TimeSpan(0, 0, 0, 2);

        private static Dictionary<string, DebuggerGraphContainerData> _debugGraphContainerDatas = new Dictionary<string, DebuggerGraphContainerData>();
        /// <summary>
        /// 调试微图容器
        /// key:微图唯一Id
        /// </summary>
        internal static Dictionary<string, DebuggerGraphContainerData> DebugGraphContainerDatas => _debugGraphContainerDatas;

        /// <summary>
        /// 更新
        /// </summary>
        private static void s_consumerUpdate()
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
            {
                Thread.Sleep(1);
                goto Begin;
            }
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
            if (_stopwatch.GetElapsedTime() > _heartTimeInterval)
            {
                Send(DebuggerMessageType.Heartbeat, HEARTBEAT_DATA);
                _stopwatch = MicroDebuggerStopwatch.StartNew();
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



        private static TcpConnectState _netState;

        /// <summary>
        /// 连接状态
        /// </summary>
        public static TcpConnectState NetState
        {
            get => _netState;
            private set
            {
                TcpConnectState oldState = _netState;
                _netState = value;
                if (oldState != value)
                {
                    try
                    {
                        s_producerAction(() => { MicroGraphEventListener.OnEventAll(MicroGraphEventIds.DEBUGGER_STATE_CHANGED); });
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private const int BufferSize = 1024;
        private static byte[] buffer = new byte[BufferSize];
        private static List<byte> allBytes = new List<byte>();
        public static void Connect(string ip = "127.0.0.1", int port = 65500)
        {
            if (NetState != TcpConnectState.Disconnect)
                return;
            try
            {
                Instance.Client = new TcpClient();
                Instance.Client.SendTimeout = 10000;
                Instance.Client.ReceiveTimeout = 10000;
                NetState = TcpConnectState.Connecting;
                m_connect(ip, port).ConfigureAwait(false);
            }
            catch (Exception)
            {
                NetState = TcpConnectState.Disconnect;
                MicroGraphLogger.LogError("连接失败，请检查Ip或者Port是否合法");
                Instance.Client?.Close();
            }

        }

        public static void Disconnect()
        {
            NetState = TcpConnectState.Disconnect;
            try
            {
                _debugGraphContainerDatas.Clear();
                Instance.Client?.Close();
                Instance.Client = null;
            }
            catch (Exception)
            {
            }
        }

        public static void Send<T>(T data) where T : INetData
        {
            if (NetState != TcpConnectState.Connected)
                return;
            NetMessagePackage package = NetMessagePackage.CreateSendMessagePackage(data);
            byte[] datas = package.GetMessageData();
            Instance.NetStream.Write(datas, 0, datas.Length);
        }
        /// <summary>
        /// 发送协议
        /// </summary>
        /// <param name="type"></param>
        /// <param name="datas"></param>
        private static void Send(DebuggerMessageType type, byte[] datas)
        {
            if (NetState != TcpConnectState.Connected)
                return;
            try
            {
                NetMessagePackage package = NetMessagePackage.CreateSendMessagePackage(type, datas);
                byte[] sendData = package.GetMessageData();
                Instance.NetStream.Write(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError("心跳发送失败:" + ex.Message);
                Disconnect();
            }
        }

        private static async Task m_connect(string ip, int port)
        {
            try
            {
                
                await Instance.Client.ConnectAsync(ip, port);
                Instance.NetStream = Instance.Client.GetStream();
                NetState = TcpConnectState.Connected;
                // 启动接收消息的任务
                _ = m_receiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError("连接失败:" + ex.Message);
                NetState = TcpConnectState.Disconnect;
                Instance.Client?.Close();
            }
        }

        private static async Task m_receiveMessagesAsync()
        {
            try
            {
                int bytesRead;
                while (_netState == TcpConnectState.Connected)
                {
                    // 等待服务器返回数据
                    bytesRead = await Instance.Client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        MicroGraphLogger.LogError("xxxxxx");
                        // Client has closed the connection.
                        break;
                    }
                    //接受到消息需要处理
                    allBytes.AddRange(buffer[..bytesRead]);
                    if (NetMessagePackage.CheckDataComplete(allBytes))
                    {
                        NetMessagePackage package = NetMessagePackage.CreateReceiveMessagePackage(ref allBytes);
                        try
                        {
                            m_handleMessage(package);
                        }
                        catch (Exception ex)
                        {
                            MicroGraphLogger.LogError(ex.Message);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MicroGraphLogger.LogError($"Error receiving message: {ex.Message}");
            }
            finally
            {
                NetState = TcpConnectState.Disconnect;
                Instance.Client?.Close();
                MicroGraphLogger.LogError($"客户端断开连接");
            }
        }

        /// <summary>
        /// 调试器内部处理
        /// </summary>
        /// <param name="package"></param>
        private static void m_handleMessage(NetMessagePackage package)
        {
            if (package.Type == DebuggerMessageType.GraphData)
            {
                DebuggerGraphData graphData = package.GetMessageContent<DebuggerGraphData>();
                if (!_debugGraphContainerDatas.TryGetValue(graphData.microGraphId, out DebuggerGraphContainerData container))
                {
                    container = new DebuggerGraphContainerData() { microGraphId = graphData.microGraphId };
                    _debugGraphContainerDatas.Add(graphData.microGraphId, container);
                }
                container.DebugGraphEditorData.TryGetValue(graphData.runtimeName, out DebuggerGraphEditorData graphEditorData);
                if (graphEditorData == null)
                {
                    graphEditorData = new DebuggerGraphEditorData();
                    graphEditorData.runtimeName = graphData.runtimeName;
                    graphEditorData.microGraphId = graphData.microGraphId;
                    container.DebugGraphEditorData.Add(graphEditorData.runtimeName, graphEditorData);
                }
                foreach (var nodeData in graphData.nodeDatas)
                {
                    if (graphEditorData.nodeDatas.ContainsKey(nodeData.nodeId))
                        graphEditorData.nodeDatas[nodeData.nodeId] = (NodeState)nodeData.nodeState;
                    else
                        graphEditorData.nodeDatas.Add(nodeData.nodeId, (NodeState)nodeData.nodeState);
                }
                foreach (var varData in graphData.varDatas)
                {
                    if (graphEditorData.varDatas.ContainsKey(varData.varName))
                        graphEditorData.varDatas[varData.varName] = varData.data;
                    else
                        graphEditorData.varDatas.Add(varData.varName, varData.data);
                }
                s_producerAction(() => { MicroGraphEventListener.OnEventAll(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPH_DATA_CHANGED, graphData); });
            }
            else if (package.Type == DebuggerMessageType.GraphRename)
            {
                DebuggerGraphRenameData graphData = package.GetMessageContent<DebuggerGraphRenameData>();
                if (!_debugGraphContainerDatas.TryGetValue(graphData.microGraphId, out DebuggerGraphContainerData container))
                    return;
                if (!container.DebugGraphEditorData.TryGetValue(graphData.oldName, out DebuggerGraphEditorData graphEditorData))
                    return;
                graphEditorData.runtimeName = graphData.newName;
                container.DebugGraphEditorData.Remove(graphData.oldName);
                if (!container.DebugGraphEditorData.ContainsKey(graphData.newName))
                    container.DebugGraphEditorData.Add(graphData.newName, graphEditorData);
                else
                    MicroGraphLogger.LogError($"存在重复的运行时:{graphData.newName}");
                s_producerAction(() => { MicroGraphEventListener.OnEventAll(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHRENAME_DATA_CHANGED, graphData); });
            }
            else if (package.Type == DebuggerMessageType.GraphDelete)
            {
                DebuggerGraphDeleteData graphData = package.GetMessageContent<DebuggerGraphDeleteData>();
                if (!_debugGraphContainerDatas.TryGetValue(graphData.microGraphId, out DebuggerGraphContainerData container))
                    return;
                if (!container.DebugGraphEditorData.TryGetValue(graphData.runtimeName, out DebuggerGraphEditorData graphEditorData))
                    return;
                container.DebugGraphEditorData.Remove(graphData.runtimeName);
                s_producerAction(() => { MicroGraphEventListener.OnEventAll(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHDELETE_DATA_CHANGED, graphData); });
            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            // Complete sending the data to the remote device.
            clientSocket.EndSend(ar);
        }

        private class EditorDebugger
        {
            /// <summary>
            /// Tcp的socket
            /// </summary>
            internal TcpClient Client;
            internal NetworkStream NetStream;
            ~EditorDebugger()
            {
                Disconnect();
            }
        }
    }
}

#endif