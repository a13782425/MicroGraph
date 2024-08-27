using MicroGraph.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace MicroGraph.MonoExample.Runtime
{
    public class MonoRuntime : MonoBehaviour
    {
        private MicroGraphState _graphState = MicroGraphState.Idle;
        public MicroGraphState GraphState
        {
            get
            {
                return _graphState;
            }
            private set
            {
                MicroGraphState graphState = _graphState;
                _graphState = value;
                if (value != graphState)
                {
                    this.onStateChanged?.Invoke(graphState, value);
                }
            }
        }
        private int _singleRunNodeCount = 100;
        public int SingleRunNodeCount { get => _singleRunNodeCount; set => _singleRunNodeCount = value; }

        public event MicroGraphStateChanged onStateChanged;
        private Queue<BaseMicroNode> _waitAwakeNode = new Queue<BaseMicroNode>();

        private Queue<BaseMicroNode> _updateAwakeNode = new Queue<BaseMicroNode>();

        private Queue<BaseMicroNode> _waitUpdateNodes = new Queue<BaseMicroNode>();

        private Queue<BaseMicroNode> _updateUpdateNodes = new Queue<BaseMicroNode>();

        private Queue<BaseMicroNode> _waitDestoryNodes = new Queue<BaseMicroNode>();
        private Queue<BaseMicroNode> _updateNodes = new Queue<BaseMicroNode>();

        private BaseMicroNode _updateNode;

        public MonoGraph graph;
        private MonoGraph _runningGraph;
        private int _curRunNodeCount = 0;
        private bool _isUpdate = false;

        private void Awake()
        {
            if (graph != null)
            {
                _runningGraph = (MonoGraph)graph.DeepClone();
                _runningGraph.Initialize();
                BaseMicroNode node = _runningGraph.GetNode(_runningGraph.AwakeNodeId);
                if (node != null)
                    _waitAwakeNode.Enqueue(node);
                _updateNode = _runningGraph.GetNode(_runningGraph.UpdateNodeId);
                node = _runningGraph.GetNode(_runningGraph.DestroyNodeId);
                if (node != null)
                    _waitDestoryNodes.Enqueue(node);
                _runningGraph.GetVariable("tran").SetValue(this.transform);
                m_execute(_waitAwakeNode, _updateAwakeNode);
            }

        }

        private void Update()
        {
            if (_runningGraph == null)
                return;
            if (_isUpdate)
            {
                //执行Update
                m_update(_waitUpdateNodes, _updateUpdateNodes, Time.deltaTime, Time.unscaledDeltaTime);
                m_execute(_waitUpdateNodes, _updateUpdateNodes);
                if (_waitUpdateNodes.Count == 0 && _updateUpdateNodes.Count == 0)
                {
                    m_startUpdate();
                }
            }
            else
            {
                if (_updateAwakeNode.Count == 0 && _waitAwakeNode.Count == 0)
                {
                    _isUpdate = true;
                    m_startUpdate();
                }
                else
                {
                    m_update(_waitAwakeNode, _updateAwakeNode, Time.deltaTime, Time.unscaledDeltaTime);
                    m_execute(_waitAwakeNode, _updateAwakeNode);
                }

            }
        }

        private void OnDestroy()
        {
            if (_runningGraph == null)
                return;
            m_stopUpdate();
            m_execute(_waitDestoryNodes, null);
            ExitGraph();
        }
        public void ExitGraph()
        {
            if (_graphState == MicroGraphState.Exit || _graphState == MicroGraphState.ExitFailure)
            {
                Debug.LogError("重复退出");
                return;
            }
            GraphState = MicroGraphState.Exit;
            foreach (var item in _runningGraph.Nodes)
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
        private void m_startUpdate()
        {
            if (_updateNode == null)
                return;
            _updateAwakeNode.Enqueue(_updateNode);
            m_execute(_updateAwakeNode, _updateUpdateNodes);
        }
        private void m_update(Queue<BaseMicroNode> waitNodes, Queue<BaseMicroNode> updateNodes, float deltaTime, float unscaledDeltaTime)
        {
            while (updateNodes.Count > 0)
            {
                BaseMicroNode baseMicroNode = updateNodes.Dequeue();
                try
                {
                    baseMicroNode.OnUpdate(deltaTime, unscaledDeltaTime);
                    if (baseMicroNode.State == NodeState.Success)
                    {
                        baseMicroNode.PushVariable();
                        foreach (int item in baseMicroNode.GetChild())
                        {
                            BaseMicroNode node = _runningGraph.GetNode(item);
                            if (node.State != NodeState.Skip)
                            {
                                waitNodes.Enqueue(node);
                            }
                        }
                    }
                    else if (baseMicroNode.State == NodeState.Running)
                    {
                        _waitUpdateNodes.Enqueue(baseMicroNode);
                    }
                    else if (baseMicroNode.State == NodeState.Failed)
                    {
                        throw new Exception("节点执行失败");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("节点：" + baseMicroNode.GetType().FullName + ",执行失败：" + ex.Message);
                    GraphState = MicroGraphState.RunningFailure;
                    break;
                }
            }

            while (_waitUpdateNodes.Count > 0)
            {
                updateNodes.Enqueue(_waitUpdateNodes.Dequeue());
            }
        }
        private void m_stopUpdate()
        {
            _isUpdate = false;
            while (_waitUpdateNodes.Count > 0)
            {
                var node = _waitUpdateNodes.Dequeue();
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
            while (_updateUpdateNodes.Count > 0)
            {
                var node = _updateUpdateNodes.Dequeue();
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
        }




        private void m_execute(Queue<BaseMicroNode> waitNodes, Queue<BaseMicroNode> updateNodes)
        {
            _curRunNodeCount = SingleRunNodeCount;
            while (waitNodes.Count > 0)
            {
                _curRunNodeCount--;
                if (_curRunNodeCount == 0)
                {
                    Debug.LogWarning($"单次运行超过:{SingleRunNodeCount}个节点");
                    break;
                }

                BaseMicroNode baseMicroNode = waitNodes.Dequeue();
                try
                {
                    baseMicroNode.OnEnable();
                    baseMicroNode.PullVariable();
                    baseMicroNode.OnExecute();
                    if (baseMicroNode.State == NodeState.Success)
                    {
                        baseMicroNode.PushVariable();
                        foreach (int item in baseMicroNode.GetChild())
                        {
                            BaseMicroNode node = _runningGraph.GetNode(item);
                            if (node.State != NodeState.Skip)
                            {
                                waitNodes.Enqueue(node);
                            }
                        }
                    }
                    else if (baseMicroNode.State == NodeState.Running)
                    {
                        updateNodes?.Enqueue(baseMicroNode);
                    }
                    else if (baseMicroNode.State == NodeState.Failed)
                    {
                        throw new Exception("节点执行失败");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("节点：" + baseMicroNode.GetType().FullName + ",执行失败：" + ex.Message + "\n" + ex.StackTrace);
                    GraphState = MicroGraphState.RunningFailure;
                    break;
                }
            }
        }
    }
}