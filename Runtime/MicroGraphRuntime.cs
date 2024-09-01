using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图运行状态变化
    /// </summary>
    /// <param name="runtime"></param>
    /// <param name="state"></param>
    public delegate void MicroGraphRuntimeStateChanged(int startNodeId, MicroGraphRuntimeState oldState, MicroGraphRuntimeState newState);
    /// <summary>
    /// 微图运行时示例
    /// </summary>
    public class MicroGraphRuntime
    {
        private BaseMicroGraph _runtimeGraph;
        /// <summary>
        /// 运行时的微图实例
        /// </summary>
        public BaseMicroGraph RuntimeGraph { get { return _runtimeGraph; } }
        /// <summary>
        /// 原始微图
        /// </summary>
        private readonly BaseMicroGraph _originGraph;

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
        private List<int> _startNodes = new List<int>();

        /// <summary>
        /// 已经被使用的node
        /// 因为刚开始已经深拷贝一次，尽量用一个对象
        /// </summary>
        private HashSet<int> _useNodes = new HashSet<int>();

        /// <summary>
        /// 当前运行节点数量
        /// </summary>
        private int _curRunNodeCount = 0;
        public MicroGraphRuntime(BaseMicroGraph microGraph)
        {
            if (microGraph == null)
            {
                MicroGraphLogger.LogError("原始微图为空");
                return;
            }
            this._originGraph = microGraph;

            this._runtimeGraph = (BaseMicroGraph)microGraph.DeepClone();
            this._runtimeGraph.Initialize();
        }

        /// <summary>
        /// 开始播放微图
        /// <para>唯一Id <see cref="BaseMicroNode.OnlyId"/></para>
        /// </summary>
        /// <param name="startNodeIds">开始的节点唯一Id</param>
        public void Play(params int[] startNodeIds)
        {
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
                if (_runtimeAtoms.TryGetValue(item, out RuntimeAtom atom))
                {
                    if (atom.RuntimeState != MicroGraphRuntimeState.Idle && atom.RuntimeState != MicroGraphRuntimeState.Exit)
                    {
                        MicroGraphLogger.LogWarning($"当前开始节点:{item} 正在运行，请先停止运行中的开始节点，再运行开始节点");
                        continue;
                    }
                    else
                    {
                        if (atom.isRemove)
                        {
                            atom.isRemove = false;
                            _startNodes.Add(item);
                        }
                        atom.Play();
                        continue;
                    }
                }
                BaseMicroNode startNode = m_getNode(item);
                if (startNode == null)
                {
                    MicroGraphLogger.LogWarning($"开始节点：{item} 不存在");
                    continue;
                }
                atom = new RuntimeAtom(this, startNode);
                _startNodes.Add(item);
                _runtimeAtoms[item] = atom;
                atom.Play();
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
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} Update执行失败, Msg: {ex.Message}");
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
            for (int i = _startNodes.Count - 1; i >= 0; i--)
            {
                if (_startNodes.Count <= i)
                    continue;

                if (_runtimeAtoms.TryGetValue(_startNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Update(deltaTime, unscaledDeltaTime);
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} Update执行失败, Msg: {ex.Message}");
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
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 暂停失败, Msg: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 暂停全部
        /// </summary>
        public void Pause()
        {
            for (int i = _startNodes.Count - 1; i >= 0; i--)
            {
                if (_startNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_startNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Pause();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 暂停失败, Msg: {ex.Message}");
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
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 恢复失败, Msg: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 恢复全部
        /// </summary>
        public void Resume()
        {
            for (int i = _startNodes.Count - 1; i >= 0; i--)
            {
                if (_startNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_startNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Resume();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 恢复失败, Msg: {ex.Message}");
                    }

                }
            }
        }

        /// <summary>
        /// 退出指定的开始节点
        ///  <para>会将其删除</para>
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
                    MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 退出失败, Msg: {ex.Message}");
                }
                finally
                {
                    atom.isRemove = true;
                    _startNodes.Remove(startNodeId);
                }
            }
        }
        /// <summary>
        /// 退出全部
        /// <para>会删除全部</para>
        /// </summary>
        public void Exit()
        {
            for (int i = _startNodes.Count - 1; i >= 0; i--)
            {
                if (_startNodes.Count <= i)
                    continue;
                if (_runtimeAtoms.TryGetValue(_startNodes[i], out RuntimeAtom atom))
                {
                    try
                    {
                        atom.Exit();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"微图:{_runtimeGraph.name} 中的开始节点: {atom.StartNode.GetType().FullName} 退出失败, Msg: {ex.Message}");
                    }
                    finally
                    {
                        atom.isRemove = true;
                    }
                }
            }
            _startNodes.Clear();
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
        private BaseMicroNode m_getNode(int startNodeId)
        {
            BaseMicroNode originNode = _runtimeGraph.GetNode(startNodeId);
            if (originNode == null)
                return null;
            if (_useNodes.Contains(startNodeId))
            {
                BaseMicroNode node = (BaseMicroNode)originNode.DeepClone();
                node.Initialize(_runtimeGraph);
                return node;
            }
            else
            {
                _useNodes.Add(startNodeId);
                return originNode;
            }
        }
        private class RuntimeAtom
        {
            private readonly MicroGraphRuntime _runtime;

            internal readonly int StartNodeId;

            internal readonly BaseMicroNode StartNode;

            internal bool isRemove = false;

            private Dictionary<int, BaseMicroNode> _nodes = new Dictionary<int, BaseMicroNode>();
            /// <summary>
            /// 等待中的节点
            /// </summary>
            private Queue<BaseMicroNode> _waitNodes = new Queue<BaseMicroNode>();
            private Queue<BaseMicroNode> _updateNodes = new Queue<BaseMicroNode>();
            private Queue<BaseMicroNode> _nextUpdateNodes = new Queue<BaseMicroNode>();
            internal MicroGraphRuntimeState RuntimeState
            {
                get => _runtimeState;
                set
                {
                    var old = _runtimeState;
                    _runtimeState = value;
                    if (value != old)
                    {
                        try
                        {
                            _runtime.onStateChanged?.Invoke(StartNodeId, old, value);
                        }
                        catch (Exception ex)
                        {
                            MicroGraphLogger.LogError($"开始节点:{StartNodeId} 的状态改变执行失败，Msg: {ex.Message}");
                        }
                    }
                }
            }

            private MicroGraphRuntimeState _runtimeState = MicroGraphRuntimeState.Idle;
            internal RuntimeAtom(MicroGraphRuntime runtime, BaseMicroNode startNode)
            {
                _runtime = runtime;
                StartNodeId = startNode.OnlyId;
                StartNode = startNode;
            }

            internal void Play()
            {
                if (_runtimeState != MicroGraphRuntimeState.Idle)
                {
                    //第二次进入，需要将旧的节点重置
                    foreach (var item in _nodes)
                    {
                        item.Value.OnReset();
                    }
                }
                _waitNodes.Enqueue(StartNode);
                RuntimeState = MicroGraphRuntimeState.Running;
                m_execute();
                m_swapUpdateNode();
            }
            /// <summary>
            /// 更新
            /// </summary>
            /// <param name="deltaTime">一帧的时间受Time.Scale影响</param>
            /// <param name="unscaledDeltaTime">一帧的时间不受影响</param>
            internal void Update(float deltaTime, float unscaledDeltaTime)
            {
                if (_runtimeState != MicroGraphRuntimeState.Running)
                    return;
                m_update(deltaTime, unscaledDeltaTime);
                m_execute();
                m_swapUpdateNode();
                m_checkFinish();
            }
            internal void Pause()
            {
                if (_runtimeState != MicroGraphRuntimeState.Running)
                {
#if UNITY_EDITOR
                    MicroGraphLogger.Log($"微图:{_runtime._runtimeGraph.name} 中的开始节点: {StartNode.GetType().FullName} 不处于执行状态，无法暂停");
#endif
                    return;
                }
                RuntimeState = MicroGraphRuntimeState.Pause;
            }
            internal void Resume()
            {
                if (_runtimeState != MicroGraphRuntimeState.Pause)
                {
#if UNITY_EDITOR
                    MicroGraphLogger.Log($"微图:{_runtime._runtimeGraph.name} 中的开始节点: {StartNode.GetType().FullName} 不处于暂停状态，无法恢复");
#endif
                    return;
                }
                RuntimeState = MicroGraphRuntimeState.Running;
            }
            internal void Exit()
            {
                if (_runtimeState == MicroGraphRuntimeState.Exit || _runtimeState == MicroGraphRuntimeState.ExitFailure)
                {
#if UNITY_EDITOR
                    MicroGraphLogger.Log($"微图:{_runtime._runtimeGraph.name} 中的开始节点: {StartNode.GetType().FullName} 重复退出");
#endif
                    return;
                }
                RuntimeState = MicroGraphRuntimeState.Exit;
                while (_waitNodes.Count > 0)
                {
                    var node = _waitNodes.Dequeue();
                    try
                    {
                        node.OnStop();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{node.GetType().FullName},停止失败：{ex.Message}");
                    }
                }
                while (_updateNodes.Count > 0)
                {
                    var node = _updateNodes.Dequeue();
                    try
                    {
                        node.OnStop();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{node.GetType().FullName},停止失败：{ex.Message}");
                    }
                }
                _nextUpdateNodes.Clear();
                foreach (var item in _nodes.Values)
                {
                    try
                    {
                        item.OnExit();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{item.GetType().FullName},退出失败：{ex.Message}");
                        RuntimeState = MicroGraphRuntimeState.ExitFailure;
                    }
                }
            }
            /// <summary>
            /// 执行单个节点
            /// </summary>
            private void m_execute()
            {
                while (_waitNodes.Count > 0)
                {
                    _runtime._curRunNodeCount--;
                    if (_runtime._curRunNodeCount == 0)
                    {
                        MicroGraphLogger.LogWarning($"单次运行超过:{_runtime.SingleRunNodeCount}个节点");
                        break;
                    }
                    var node = _waitNodes.Dequeue();
                    try
                    {
                        node.OnEnable();
                        try
                        {
                            node.PullVariable();
                        }
                        catch (Exception ex)
                        {
                            MicroGraphLogger.LogError($"节点：{node.GetType().FullName} 拉取变量失败: {ex.Message}");
                            throw ex;
                        }
                        node.OnExecute();
                        if (node.State == NodeState.Success)
                        {
                            node.PushVariable();
                            foreach (var item in node.GetChild())
                            {
                                var tempNode = m_getNode(item);
                                if (tempNode.State == NodeState.Skip)
                                    continue;
                                _waitNodes.Enqueue(tempNode);
                            }
                        }
                        else if (node.State == NodeState.Running)
                        {
                            _nextUpdateNodes.Enqueue(node);
                        }
                        else if (node.State == NodeState.Failed)
                        {
                            throw new Exception($"节点：{node.GetType().FullName} 执行失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{node.GetType().FullName},执行失败：{ex.Message}");
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
                    _runtime._curRunNodeCount--;
                    if (_runtime._curRunNodeCount == 0)
                    {
                        MicroGraphLogger.LogWarning($"单次运行超过:{_runtime.SingleRunNodeCount}个节点");
                        break;
                    }
                    var node = _updateNodes.Dequeue();
                    try
                    {
                        node.OnUpdate(deltaTime, unscaledDeltaTime);
                        if (node.State == NodeState.Success)
                        {
                            node.PushVariable();
                            foreach (var item in node.GetChild())
                            {
                                var tempNode = m_getNode(item);
                                if (tempNode.State == NodeState.Skip)
                                    continue;
                                _waitNodes.Enqueue(tempNode);
                            }
                        }
                        else if (node.State == NodeState.Running)
                        {
                            _nextUpdateNodes.Enqueue(node);
                        }
                        else if (node.State == NodeState.Failed)
                        {
                            throw new Exception($"节点：{node.GetType().FullName} 执行失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{node.GetType().FullName},执行失败：{ex.Message}");
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
            /// <summary>
            /// 检测是否完成
            /// </summary>
            private void m_checkFinish()
            {
                if (_runtimeState == MicroGraphRuntimeState.RunningFailure)
                {
                    Exit();
                    MicroGraphLogger.LogError($"微图:{_runtime._runtimeGraph.name} 中的开始节点: {StartNode.GetType().FullName} 执行失败");
                    return;
                }
                if (_waitNodes.Count != 0 || _updateNodes.Count != 0)
                    return;
                Exit();
#if UNITY_EDITOR
                MicroGraphLogger.Log($"微图:{_runtime._runtimeGraph.name} 中的开始节点: {StartNode.GetType().FullName} 执行完成");
#endif
            }
            private BaseMicroNode m_getNode(int nodeId)
            {
                if (!_nodes.TryGetValue(nodeId, out BaseMicroNode node))
                {
                    node = _runtime.m_getNode(nodeId);
                    _nodes.Add(nodeId, node);
                }
                return node;
            }


        }

    }
}
