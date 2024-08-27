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
    public delegate void MicroGraphStateChanged(MicroGraphState oldState, MicroGraphState newState);
    /// <summary>
    /// 微图运行时示例
    /// </summary>
    public class MicroGraphRuntime
    {
        private BaseMicroGraph _microGraph;
        /// <summary>
        /// 当前微图实例
        /// </summary>
        public BaseMicroGraph MicroGraph { get { return _microGraph; } }

        private MicroGraphState _graphState = MicroGraphState.Idle;
        /// <summary>
        /// 微图状态
        /// </summary>
        public MicroGraphState GraphState
        {
            get { return _graphState; }
            protected set
            {
                var old = _graphState;
                _graphState = value;
                if (value != old)
                    onStateChanged?.Invoke(old, value);
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
        public event MicroGraphStateChanged onStateChanged;

        /// <summary>
        /// 等待中的节点
        /// </summary>
        private Queue<BaseMicroNode> _waitNodes = new Queue<BaseMicroNode>();
        private Queue<BaseMicroNode> _updateNodes = new Queue<BaseMicroNode>();
        private Queue<BaseMicroNode> _waitUpdateNodes = new Queue<BaseMicroNode>();

        /// <summary>
        /// 当前运行节点数量
        /// 每次运行前都等于<see cref="SingleRunNodeCount">
        /// </summary>
        private int _curRunNodeCount = 0;
        public MicroGraphRuntime(BaseMicroGraph microGraph)
        {
            this._microGraph = microGraph;
        }
        public virtual void PlayGraph()
        {
            if (_microGraph == null)
            {
                Debug.LogError("当前没有可播放的微图");
                return;
            }
            if (_graphState != MicroGraphState.Idle && _graphState != MicroGraphState.Exit)
            {
                Debug.LogError("当前微图正在运行，请先停止运行中的微图，再运行新图");
                return;
            }
            m_play();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime">一帧的时间受Time.Scale影响</param>
        /// <param name="unscaledDeltaTime">一帧的时间不受影响</param>
        public virtual void UpdateGraph(float deltaTime, float unscaledDeltaTime)
        {
            if (_graphState != MicroGraphState.Running)
                return;
            while (_updateNodes.Count > 0)
            {
                var node = _updateNodes.Dequeue();
                try
                {
                    node.OnUpdate(deltaTime, unscaledDeltaTime);
                    if (node.State == NodeState.Success)
                    {
                        try
                        {
                            node.PushVariable();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("推送变量失败:" + ex.Message);
                            throw ex;
                        }
                        foreach (var item in node.GetChild())
                        {
                            var tempNode = this._microGraph.GetNode(item);
                            if (tempNode.State == NodeState.Skip)
                                continue;
                            _waitNodes.Enqueue(this._microGraph.GetNode(item));
                        }
                    }
                    else if (node.State == NodeState.Running)
                    {
                        _waitUpdateNodes.Enqueue(node);
                    }
                    else if (node.State == NodeState.Failed)
                    {
                        throw new Exception("节点执行失败");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点：{node.GetType().FullName},执行失败：{ex.Message}");
                    this.GraphState = MicroGraphState.RunningFailure;
                    break;
                }
            }
            while (_waitUpdateNodes.Count > 0)
            {
                _updateNodes.Enqueue(_waitUpdateNodes.Dequeue());
            }
            m_execute();
            m_checkFinish();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void ResetGraph()
        {
            if (_graphState != MicroGraphState.Exit && _graphState != MicroGraphState.ExitFailure)
            {
                Debug.LogError("当前图正在执行，无法重置");
                return;
            }

            GraphState = MicroGraphState.Idle;
            _updateNodes.Clear();
            _waitNodes.Clear();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public virtual void PauseGraph()
        {
            if (_graphState != MicroGraphState.Running)
            {
                Debug.LogError("当前图不处于执行状态，无法暂停");
                return;
            }
            GraphState = MicroGraphState.Pause;
        }
        /// <summary>
        /// 恢复暂停
        /// </summary>
        public virtual void ResumeGraph()
        {
            if (_graphState != MicroGraphState.Pause)
            {
                Debug.LogError("当前图暂停状态，无法恢复");
                return;
            }
            GraphState = MicroGraphState.Running;
        }
        /// <summary>
        /// 退出
        /// </summary>
        public virtual void ExitGraph()
        {
            if (_graphState == MicroGraphState.Exit || _graphState == MicroGraphState.ExitFailure)
            {
                Debug.LogError("重复退出");
                return;
            }
            GraphState = MicroGraphState.Exit;
            while (_waitNodes.Count > 0)
            {
                var node = _waitNodes.Dequeue();
                try
                {
                    node.OnStop();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点：{node.GetType().FullName},停止失败：{ex.Message}");
                    //break;
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
                    Debug.LogError($"节点：{node.GetType().FullName},停止失败：{ex.Message}");
                    //break;
                }
            }
            foreach (var item in _microGraph.Nodes)
            {
                try
                {
                    item.OnExit();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点：{item.GetType().FullName},退出失败：{ex.Message}");
                    this.GraphState = MicroGraphState.ExitFailure;
                    //break;
                }
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        private void m_play()
        {
            _microGraph.Initialize();
            List<BaseMicroNode> startNodes = this._microGraph.GetStartNode();
            if (startNodes.Count == 0)
            {
                Debug.LogError("开始节点为空,请在图中设置开始节点");
                return;
            }
            foreach (var item in startNodes)
            {
                _waitNodes.Enqueue(item);
            }
            this.GraphState = MicroGraphState.Running;
            m_execute();
        }
        /// <summary>
        /// 执行单个节点
        /// </summary>
        private void m_execute()
        {
            _curRunNodeCount = SingleRunNodeCount;
            while (_waitNodes.Count > 0)
            {
                _curRunNodeCount--;
                if (_curRunNodeCount == 0)
                {
                    Debug.LogWarning($"单次运行超过:{SingleRunNodeCount}个节点");
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
                        Debug.LogError("拉取变量失败:" + ex.Message);
                        throw ex;
                    }
                    node.OnExecute();
                    if (node.State == NodeState.Success)
                    {
                        node.PushVariable();
                        foreach (var item in node.GetChild())
                        {
                            var tempNode = this._microGraph.GetNode(item);
                            if (tempNode.State == NodeState.Skip)
                                continue;
                            _waitNodes.Enqueue(this._microGraph.GetNode(item));
                        }
                    }
                    else if (node.State == NodeState.Running)
                    {
                        _updateNodes.Enqueue(node);
                    }
                    else if (node.State == NodeState.Failed)
                    {
                        throw new Exception("节点执行失败");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点：{node.GetType().FullName},执行失败：{ex.Message}");
                    this.GraphState = MicroGraphState.RunningFailure;
                    break;
                }
            }
        }

        /// <summary>
        /// 检测是否完成
        /// </summary>
        private void m_checkFinish()
        {
            if (_graphState == MicroGraphState.RunningFailure)
            {
                ExitGraph();
                Debug.LogError($"{_microGraph.name} 执行失败");
                return;
            }
            if (_waitNodes.Count != 0 || _updateNodes.Count != 0)
                return;
            GraphState = MicroGraphState.Exit;
            foreach (var item in _microGraph.Nodes)
            {
                try
                {
                    item.OnExit();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点：{item.GetType().FullName},退出失败：{ex.Message}");
                    this.GraphState = MicroGraphState.ExitFailure;
                    //break;
                }
            }
            Debug.Log($"{_microGraph.name} 执行完毕");
        }
    }
}
