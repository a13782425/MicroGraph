using System;
using System.Collections.Generic;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图运行状态变化
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="state"></param>
    public delegate void MicroGraphRuntimeStateChanged(RuntimeAtom runtimeAtom, MicroGraphRuntimeState oldState, MicroGraphRuntimeState newState);
    /// <summary>
    /// 微图运行时示例
    /// </summary>
    public class MicroGraphRuntime : IDisposable
    {
        /// <summary>
        /// 原始微图
        /// </summary>
        private readonly BaseMicroGraph _originGraph;

        private BaseMicroGraph _runtimeGraph;
        /// <summary>
        /// 运行时的微图实例
        /// </summary>
        public BaseMicroGraph RuntimeGraph { get { return _runtimeGraph; } }

        private string _runtimeName;
        /// <summary>
        /// 运行时名字
        /// </summary>
        public string RuntimeName
        {
            get => _runtimeName;
            set
            {
                string oldName = _runtimeName;
                _runtimeName = value;
                _runtimeGraph.name = value;
#if MICRO_GRAPH_DEBUG
                if (MicroGraphDebugger.IsListener && oldName != _runtimeName)
                {
                    DebuggerGraphRenameData renameData = new DebuggerGraphRenameData();
                    renameData.oldName = oldName;
                    renameData.newName = _runtimeName;
                    renameData.microGraphId = RuntimeGraph.OnlyId;
                    MicroGraphDebugger.Send(renameData);
                }
#endif
            }
        }

        private int _singleRunNodeCount = 100;

        /// <summary>
        /// 单次指定最大节点数量
        /// <para>避免节点过多时卡死</para>
        /// </summary>
        public int SingleRunNodeCount { get => _singleRunNodeCount; set => _singleRunNodeCount = value; }

        /// <summary>
        /// 状态变化事件
        /// </summary>
        public event MicroGraphRuntimeStateChanged onStateChanged;

        /// <summary>
        /// 所有运行时
        /// </summary>
        private Dictionary<int, RuntimeAtom> _runtimeAtoms = new Dictionary<int, RuntimeAtom>();
        private List<int> _runningNodes = new List<int>();

        /// <summary>
        /// 当前运行节点数量
        /// </summary>
        private int _curRunNodeCount = 0;
        /// <summary>
        /// 是否显示运行日志
        /// </summary>
        public bool ShowRunningLog { get; set; }

        /// <summary>
        /// 是否已经释放
        /// </summary>
        private bool _isDispose = false;
        /// <summary>
        /// 是否已经释放
        /// </summary>
        public bool IsDispose => _isDispose;

        public MicroGraphRuntime(BaseMicroGraph microGraph, string runtimeName = null)
        {
            ShowRunningLog = false;
            if (microGraph == null)
            {
                MicroGraphLogger.LogError("原始微图为空");
                return;
            }
            this._originGraph = microGraph;
            this._runtimeGraph = (BaseMicroGraph)microGraph.DeepClone();
            this._runtimeGraph.name = string.IsNullOrWhiteSpace(runtimeName) ? microGraph.name + "_" + _runtimeGraph.GetInstanceID() : runtimeName;
            this._runtimeGraph.Initialize();
            this._runtimeName = this._runtimeGraph.name;
#if MICRO_GRAPH_DEBUG
            if (MicroGraphDebugger.IsListener)
            {
                DebuggerGraphData graphData = DebuggerGraphData.Create();
                graphData.runtimeName = RuntimeName;
                graphData.microGraphId = RuntimeGraph.OnlyId;
                graphData.start = true;
                MicroGraphDebugger.Send(graphData);
                //发送变量
                foreach (var item in _runtimeGraph.Variables)
                    item.SetValue(item.GetValue());
                DebuggerGraphData.Release(graphData);
            }
#endif
        }

        /// <summary>
        /// 开始播放微图
        /// <para>唯一Id <see cref="BaseMicroNode.OnlyId"/></para>
        /// </summary>
        /// <param name="startNodeIds">开始的节点唯一Id</param>
        public void Play(params int[] startNodeIds)
        {
            if (_isDispose)
            {
                MicroGraphLogger.LogWarning($"当前运行时已被释放");
                return;
            }
            if (_runtimeGraph == null)
            {
                MicroGraphLogger.LogError("当前没有可播放的微图");
                return;
            }

            if (startNodeIds == null || startNodeIds.Length == 0)
            {
                MicroGraphLogger.LogWarning("开始节点为空");
                return;
            }
            _curRunNodeCount = SingleRunNodeCount;
            foreach (var item in startNodeIds)
            {
                if (_runningNodes.Contains(item))
                {
                    MicroGraphLogger.LogWarning($"当前开始节点:{item} 正在运行，请先停止运行中的开始节点，再运行开始节点");
                    continue;
                }
                _runningNodes.Add(item);
                if (_runtimeAtoms.ContainsKey(item))
                    continue;
                RuntimeAtom atom = new RuntimeAtom(_runtimeGraph, item);
                atom.SingleRunNodeCount = this.SingleRunNodeCount;
                atom.onStateChanged += m_atomOnStateChanged;
                atom.onStateChanged += onStateChanged;
                _runtimeAtoms[item] = atom;
            }
            foreach (var item in startNodeIds)
            {
                if (_runningNodes.Contains(item))
                    _runtimeAtoms[item].Play();
            }
        }

        /// <summary>
        /// 更新指定正在运行的运行时
        /// </summary>
        /// <param name="deltaTime">一帧的时间受Time.Scale影响</param>
        /// <param name="unscaledDeltaTime">一帧的时间不受影响</param>
        public void Update(int startNodeId, float deltaTime, float unscaledDeltaTime)
        {
            _curRunNodeCount = SingleRunNodeCount;
            if (_runtimeAtoms.TryGetValue(startNodeId, out RuntimeAtom atom))
            {
                try
                {
                    atom.Update(deltaTime, unscaledDeltaTime);
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} Update执行失败, Msg: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 更新所有正在运行的运行时
        /// </summary>
        /// <param name="deltaTime">一帧的时间受Time.Scale影响</param>
        /// <param name="unscaledDeltaTime">一帧的时间不受影响</param>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            _curRunNodeCount = SingleRunNodeCount;
            for (int i = _runningNodes.Count - 1; i >= 0; i--)
            {
                if (_runningNodes.Count <= i)
                    continue;

                if (_runtimeAtoms.TryGetValue(_runningNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Update(deltaTime, unscaledDeltaTime);
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} Update执行失败, Msg: {ex.Message}");
                    }
                }
            }
        }
        /// <summary>
        /// 暂停正在执行的开始节点
        /// </summary>
        /// <param name="startNodeId">开始节点</param>
        public void Pause(int startNodeId)
        {
            if (_runtimeAtoms.TryGetValue(startNodeId, out RuntimeAtom atom))
            {
                try
                {
                    atom.Pause();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 暂停失败, Msg: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 暂停全部
        /// </summary>
        public void Pause()
        {
            for (int i = _runningNodes.Count - 1; i >= 0; i--)
            {
                if (_runningNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_runningNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Pause();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 暂停失败, Msg: {ex.Message}");
                    }

                }
            }
        }
        /// <summary>
        /// 恢复一个暂停中的开始节点
        /// </summary>
        /// <param name="startNodeId">开始节点</param>
        public void Resume(int startNodeId)
        {
            if (_runtimeAtoms.TryGetValue(startNodeId, out RuntimeAtom atom))
            {
                try
                {
                    atom.Resume();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 恢复失败, Msg: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 恢复全部
        /// </summary>
        public void Resume()
        {
            for (int i = _runningNodes.Count - 1; i >= 0; i--)
            {
                if (_runningNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_runningNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Resume();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 恢复失败, Msg: {ex.Message}");
                    }

                }
            }
        }

        /// <summary>
        /// 停止某个节点
        /// </summary>
        /// <param name="startNodeId"></param>
        public void Exit(int startNodeId)
        {
            if (_runtimeAtoms.TryGetValue(startNodeId, out RuntimeAtom atom))
            {
                try
                {
                    atom.Exit();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 退出失败, Msg: {ex.Message}");
                }
                finally
                {
                    _runningNodes.Remove(startNodeId);
                }
            }
        }
        /// <summary>
        /// 停止全部
        /// </summary>
        public void Exit()
        {
            for (int i = _runningNodes.Count - 1; i >= 0; i--)
            {
                if (_runningNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_runningNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Exit();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.startNode.GetType().FullName} 退出失败, Msg: {ex.Message}");
                    }
                }
            }
            _runningNodes.Clear();
        }

        /// <summary>
        /// 获取某个开始节点的运行时状态
        /// </summary>
        /// <param name="startNodeId">开始节点</param>
        /// <returns></returns>
        public MicroGraphRuntimeState GetRuntimeState(int startNodeId)
        {
            if (_runtimeAtoms.TryGetValue(startNodeId, out RuntimeAtom atom))
                return atom.RuntimeState;
            return MicroGraphRuntimeState.Illegality;
        }

        /// <summary>
        /// 释放当前运行时
        /// </summary>
        public void Dispose()
        {
            _isDispose = true;
            Exit();
            foreach (var node in _runtimeGraph.Nodes)
            {
                if (node.State != NodeState.Exit)
                    node.OnExit();
            }
#if MICRO_GRAPH_DEBUG
            if (MicroGraphDebugger.IsListener)
            {
                DebuggerGraphDeleteData graphData = new DebuggerGraphDeleteData();
                graphData.runtimeName = RuntimeName;
                graphData.microGraphId = RuntimeGraph.OnlyId;
                MicroGraphDebugger.Send(graphData);
            }
#endif
        }
        /// <summary>
        /// 运行单元状态变化
        /// </summary>
        /// <param name="runtimeAtom"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void m_atomOnStateChanged(RuntimeAtom runtimeAtom, MicroGraphRuntimeState oldState, MicroGraphRuntimeState newState)
        {
            if (newState == MicroGraphRuntimeState.Complete || newState == MicroGraphRuntimeState.Exit)
                _runningNodes.Remove(runtimeAtom.startNodeId);
        }
        ~MicroGraphRuntime()
        {
#if MICRO_GRAPH_DEBUG
            if (MicroGraphDebugger.IsListener)
            {
                DebuggerGraphDeleteData graphData = new DebuggerGraphDeleteData();
                graphData.runtimeName = RuntimeName;
                graphData.microGraphId = RuntimeGraph.OnlyId;
                MicroGraphDebugger.Send(graphData);
            }
#endif
        }
    }
    /// <summary>
    /// 运行时最小单元
    /// <para>每个运行时最小单元的节点相互独立互不干扰</para>
    /// </summary>
    public class RuntimeAtom
    {
        public readonly int startNodeId;

        internal readonly BaseMicroNode startNode;
        internal readonly BaseMicroGraph microGraph;
        internal event MicroGraphRuntimeStateChanged onStateChanged;
        private MicroGraphRuntimeState _runtimeState = MicroGraphRuntimeState.Idle;
        private MicroGraphRuntimeEndMode _endMode = MicroGraphRuntimeEndMode.None;
        /// <summary>
        /// 运行时的结束模式
        /// <para>1.自动结束</para>
        /// <para>2.触发Exit结束</para>
        /// <para>3.触发EndNodeId结束</para>
        /// </summary>
        public MicroGraphRuntimeEndMode EndMode { get => _endMode; }
        /// <summary>
        /// 执行过的节点
        /// 方便后面操作
        /// </summary>
        private Dictionary<int, BaseMicroNode> _nodes = new Dictionary<int, BaseMicroNode>();
        /// <summary>
        /// 等待中的节点
        /// </summary>
        private Queue<RuntimeNodeData> _waitNodes = new Queue<RuntimeNodeData>();
        private Queue<RuntimeNodeData> _updateNodes = new Queue<RuntimeNodeData>();
        private Queue<RuntimeNodeData> _nextUpdateNodes = new Queue<RuntimeNodeData>();
        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData { get; set; }
        /// <summary>
        /// 单次指定最大节点数量(默认100)
        /// <para>避免节点过多时卡死</para>
        /// </summary>
        public int SingleRunNodeCount { get; set; } = 100;
        /// <summary>
        /// 当前运行节点数量
        /// </summary>
        private int _curRunNodeCount = 0;

        private int _endNodeId = -1;
        /// <summary>
        /// 手动结束节点，默认是-1(自动结束)
        /// <para>如果是-1则该运行单元会全部执行完毕</para>
        /// <para>如果手动指定了结束按钮，那么当手动结束按钮执行结束之后，该运行单元会变成Complete，后续节点也不会运行</para>
        /// </summary>
        public int EndNodeId { get => _endNodeId; set => _endNodeId = value; }

        /// <summary>
        /// 是否运行到结束节点
        /// </summary>
        private bool _isEndNodeComplete = false;

        private int _runtimeUniqueId = 0;
        /// <summary>
        /// 运行时的唯一Id
        /// </summary>
        public int RuntimeUniqueId => _runtimeUniqueId;

        /// <summary>
        /// 运行时版本
        /// <para>每个节点在执行时版本号会加一</para>
        /// </summary>
        private int _runtimeVersion = int.MinValue;
        /// <summary>
        /// 运行时版本
        /// </summary>
        private int RuntimeVersion
        {
            get
            {
                _runtimeVersion++;
                if (_runtimeVersion == int.MaxValue)
                    _runtimeVersion = int.MinValue;
                return _runtimeVersion;
            }
        }
        public MicroGraphRuntimeState RuntimeState
        {
            get => _runtimeState;
            private set
            {
                var old = _runtimeState;
                _runtimeState = value;
                if (value != old)
                {
                    try
                    {
                        onStateChanged?.Invoke(this, old, value);
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{microGraph.name}中的开始节点:{startNodeId} 的状态改变执行失败，Msg: {ex.Message}");
                    }
                }
            }
        }
        public RuntimeAtom(BaseMicroGraph microGraph, int startNodeId)
        {
            if (microGraph == null)
                throw new MicroGraphNullException("微图为空");
            this.microGraph = microGraph;
            this.startNodeId = startNodeId;
            this.startNode = microGraph.GetNode(startNodeId);
            this._runtimeUniqueId = S_RuntimeId;
            if (startNode == null)
                throw new MicroGraphNullException($"微图: {microGraph.name} 中没有找到开始节点：{startNodeId}");
        }
        public void Play()
        {
            if (_runtimeState == MicroGraphRuntimeState.Complete || _runtimeState == MicroGraphRuntimeState.Exit)
                RuntimeState = MicroGraphRuntimeState.Idle;

            if (_runtimeState != MicroGraphRuntimeState.Idle)
            {
                MicroGraphLogger.LogWarning($"微图: {microGraph.name} 请不要重复运行一个运行时");
                return;
            }
            _endMode = MicroGraphRuntimeEndMode.None;
            _curRunNodeCount = SingleRunNodeCount;
            _isEndNodeComplete = false;
            RuntimeState = MicroGraphRuntimeState.Running;
            var nodeData = m_getNode(startNodeId);
            m_cacheNode(nodeData);
            _waitNodes.Enqueue(nodeData);
            m_execute();
            m_swapUpdateNode();
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime">一帧的时间受Time.Scale影响</param>
        /// <param name="unscaledDeltaTime">一帧的时间不受影响</param>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (_runtimeState != MicroGraphRuntimeState.Running)
                return;
            _curRunNodeCount = SingleRunNodeCount;
            m_update(deltaTime, unscaledDeltaTime);
            m_execute();
            m_swapUpdateNode();
            m_checkFinish();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (_runtimeState != MicroGraphRuntimeState.Running)
            {
                MicroGraphLogger.Log($"微图:{microGraph.name} 中的开始节点: {startNode} 不处于执行状态，无法暂停");
                return;
            }
            RuntimeState = MicroGraphRuntimeState.Pause;
        }
        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            if (_runtimeState != MicroGraphRuntimeState.Pause)
            {
                MicroGraphLogger.Log($"微图:{microGraph.name} 中的开始节点: {startNode} 不处于暂停状态，无法恢复");
                return;
            }
            RuntimeState = MicroGraphRuntimeState.Running;
        }
        public void Exit()
        {
            if (_runtimeState == MicroGraphRuntimeState.Exit)
            {
                MicroGraphLogger.Log($"微图:{microGraph.name} 中的开始节点: {startNode} 重复退出");
                return;
            }
            //如果之前是无结束状态，则需要设置结束状态为手动结束
            if (_endMode == MicroGraphRuntimeEndMode.None)
                _endMode = MicroGraphRuntimeEndMode.Manual;
            while (_waitNodes.Count > 0)
            {
                var node = _waitNodes.Dequeue();
                try
                {
                    node.node.OnStop();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node},停止失败：{ex.Message}");
                }
            }
            while (_updateNodes.Count > 0)
            {
                var node = _updateNodes.Dequeue();
                try
                {
                    node.node.OnStop();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node},停止失败：{ex.Message}");
                }
            }
            _nextUpdateNodes.Clear();
            microGraph.RemoveRuntime(_runtimeUniqueId);
            RuntimeState = MicroGraphRuntimeState.Exit;
        }

        /// <summary>
        /// 当蓝图执行完成
        /// </summary>
        private void m_complete()
        {
            while (_waitNodes.Count > 0)
            {
                var node = _waitNodes.Dequeue();
                try
                {
                    node.node.OnStop();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node},停止失败：{ex.Message}");
                }
            }
            while (_updateNodes.Count > 0)
            {
                var node = _updateNodes.Dequeue();
                try
                {
                    node.node.OnStop();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node},停止失败：{ex.Message}");
                }
            }
            _nextUpdateNodes.Clear();
            foreach (var item in _nodes)
            {
                try
                {
                    item.Value.OnReset();
                }
                catch (Exception ex)
                {
                    MicroGraphLogger.Log($"微图:{microGraph.name} 中的节点: {item.Value} 重置失败: {ex.Message}");
                }
            }
            RuntimeState = MicroGraphRuntimeState.Complete;
        }
        /// <summary>
        /// 检测是否完成
        /// </summary>
        private void m_checkFinish()
        {
            if (_isEndNodeComplete)
                _endMode = MicroGraphRuntimeEndMode.EndNode;
            else if (_waitNodes.Count == 0 && _updateNodes.Count == 0)
                _endMode = MicroGraphRuntimeEndMode.Auto;
            if (_endMode == MicroGraphRuntimeEndMode.None)
                return;
            m_complete();
            MicroGraphLogger.Log($"微图:{microGraph.name} 中的开始节点: {startNode} 执行完成");

        }
        /// <summary>
        /// 执行单个节点
        /// </summary>
        private void m_execute()
        {
            while (_waitNodes.Count > 0)
            {
                _curRunNodeCount--;
                if (_curRunNodeCount == 0)
                {
                    MicroGraphLogger.Log($"微图: {microGraph.name} 中单次运行超过:{SingleRunNodeCount}个节点");
                    break;
                }
                var node = _waitNodes.Dequeue();
                m_cacheNode(node);
                try
                {
                    node.version = RuntimeVersion;
                    node.node.RuntimVersion = node.version;
                    node.node.OnEnable();
                    if (node.node.State == NodeState.Skip) //如果OnEnable后，节点状态为跳过，则不执行后续操作
                    {
                        m_checkEndComplete(node.nodeId);
                        ReleaseNodeData(node);
                        continue;
                    }
                    try
                    {
                        node.node.PullVariable();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node} 拉取变量失败: {ex.Message}");
                        throw ex;
                    }
                    node.node.OnExecute();
                    if (node.node.State == NodeState.Success || node.node.State == NodeState.Skip)
                    {
                        try
                        {
                            node.node.PushVariable();
                        }
                        catch (Exception ex)
                        {
                            MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node} 推送变量失败: {ex.Message}");
                            throw ex;
                        }
                        if (node.node.State == NodeState.Skip) //如果当前节点也是跳过，则不获取其子节点
                        {
                            m_checkEndComplete(node.nodeId);
                            ReleaseNodeData(node);
                            continue;
                        }
                        foreach (var item in node.node.GetChild())
                        {
                            var tempNode = m_getNode(item);
                            _waitNodes.Enqueue(tempNode);
                        }
                        m_checkEndComplete(node.nodeId);
                        ReleaseNodeData(node);
                    }
                    else if (node.node.State == NodeState.Running)
                    {
                        _nextUpdateNodes.Enqueue(node);
                    }
                    else if (node.node.State == NodeState.Failed)
                    {
                        _nextUpdateNodes.Enqueue(node);
                        MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node} 手动标记为执行失败");
                    }
                }
                catch (Exception ex)
                {
                    _nextUpdateNodes.Enqueue(node);
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node},执行失败：{ex.Message}");
                    this.RuntimeState = MicroGraphRuntimeState.RunningFailure;
                    break;
                }
            }
        }

        /// <summary>
        /// 执行单个节点
        /// </summary>
        private void m_update(float deltaTime, float unscaledDeltaTime)
        {
            while (_updateNodes.Count > 0)
            {
                _curRunNodeCount--;
                if (_curRunNodeCount == 0)
                {
                    MicroGraphLogger.Log($"微图: {microGraph.name} 中单次运行超过:{SingleRunNodeCount}个节点");
                    break;
                }
                var node = _updateNodes.Dequeue();
                try
                {
                    node.node.RuntimVersion = node.version;
                    node.node.OnUpdate(deltaTime, unscaledDeltaTime);
                    if (node.node.State == NodeState.Success || node.node.State == NodeState.Skip)
                    {
                        try
                        {
                            node.node.PushVariable();
                        }
                        catch (Exception ex)
                        {
                            MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node} 推送变量失败: {ex.Message}");
                            throw ex;
                        }
                        if (node.node.State == NodeState.Skip) //如果当前节点也是跳过，则不获取其子节点
                        {
                            m_checkEndComplete(node.nodeId);
                            ReleaseNodeData(node);
                            continue;
                        }
                        foreach (var item in node.node.GetChild())
                        {
                            var tempNode = m_getNode(item);
                            _waitNodes.Enqueue(tempNode);
                        }
                        m_checkEndComplete(node.nodeId);
                        ReleaseNodeData(node);
                    }
                    else if (node.node.State == NodeState.Running)
                    {
                        _nextUpdateNodes.Enqueue(node);
                    }
                    else if (node.node.State == NodeState.Failed)
                    {
                        _nextUpdateNodes.Enqueue(node);
                        MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node} 手动标记为更新失败,");
                    }
                }
                catch (Exception ex)
                {
                    _nextUpdateNodes.Enqueue(node);
                    MicroGraphLogger.LogError($"微图: {microGraph.name} 中节点：{node.node},更新失败：{ex.Message}");
                    this.RuntimeState = MicroGraphRuntimeState.RunningFailure;
                    break;
                }
            }
        }
        private void m_swapUpdateNode()
        {
            while (_nextUpdateNodes.Count > 0)
            {
                _updateNodes.Enqueue(_nextUpdateNodes.Dequeue());
            }
        }
        private void m_cacheNode(RuntimeNodeData nodeData)
        {
            if (!_nodes.ContainsKey(nodeData.node.OnlyId))
                _nodes.Add(nodeData.node.OnlyId, nodeData.node);
        }

        private void m_checkEndComplete(int nodeId)
        {
            if (_isEndNodeComplete)
                return;
            _isEndNodeComplete = nodeId == _endNodeId;
        }
        private RuntimeNodeData m_getNode(int nodeId)
        {
            RuntimeNodeData data = GetNodeData();
            data.nodeId = nodeId;
            BaseMicroNode node = microGraph.GetRuntimeNode(_runtimeUniqueId, nodeId);
            if (node == null)
                throw new MicroGraphNullException($"微图: {microGraph.name} 中没有找到节点：{nodeId}");
            node.RuntimeUniqueId = _runtimeUniqueId;
            data.node = node;
            return data;
        }

        private static Queue<RuntimeNodeData> s_nodeDataQueue = new Queue<RuntimeNodeData>();
        /// <summary>
        /// 运行时Id
        /// <para>每个节点在执行时版本号会加一</para>
        /// </summary>
        private static int s_runtimeId = int.MinValue;
        /// <summary>
        /// 运行时版本
        /// </summary>
        private static int S_RuntimeId
        {
            get
            {
                s_runtimeId++;
                if (s_runtimeId == int.MaxValue)
                    s_runtimeId = int.MinValue;
                return s_runtimeId;
            }
        }
        private static RuntimeNodeData GetNodeData()
        {
            if (s_nodeDataQueue.Count == 0)
                return new RuntimeNodeData();
            return s_nodeDataQueue.Dequeue();
        }

        private static void ReleaseNodeData(RuntimeNodeData data)
        {
            if (data == null)
                return;
            s_nodeDataQueue.Enqueue(data);
        }

        private class RuntimeNodeData
        {
            public int nodeId;
            public int version;
            public BaseMicroNode node;
        }
    }

}
