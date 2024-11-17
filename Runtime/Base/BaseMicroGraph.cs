using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图的基类
    /// </summary>
    [Serializable]
    public class BaseMicroGraph : ScriptableObject, IMicroGraphClone
    {
        [SerializeField]
        private string _onlyId = "";
        /// <summary>
        /// 唯一Id,不能修改
        /// </summary>
        public string OnlyId => _onlyId;
        #region 节点定义

        [SerializeReference]
        private List<BaseMicroNode> _nodes = new List<BaseMicroNode>();
        /// <summary>
        /// 当前图的所有节点
        /// </summary>
        public List<BaseMicroNode> Nodes => _nodes;
        /// <summary>
        /// 当前逻辑图的所有节点的另一种缓存
        /// </summary>
        private Dictionary<int, BaseMicroNode> _nodeDic = new Dictionary<int, BaseMicroNode>();

        #endregion

        #region 变量定义

        [SerializeReference]
        private List<BaseMicroVariable> _variables = new List<BaseMicroVariable>();
        /// <summary>
        /// 当前逻辑图的所有变量
        /// </summary>
        public List<BaseMicroVariable> Variables => _variables;
        /// <summary>
        /// 当前逻辑图的所有变量的另一种缓存
        /// </summary>
        private Dictionary<string, BaseMicroVariable> _varDic = new Dictionary<string, BaseMicroVariable>();

        #endregion

        #region 包定义
        [SerializeReference]
        private List<MicroPackageInfo> _packages = new List<MicroPackageInfo>();
        /// <summary>
        /// 当前逻辑图的所有包信息
        /// </summary>
        public List<MicroPackageInfo> Packages => _packages;
        /// <summary>
        /// 当前逻辑图的所有包信息的另一种缓存
        /// </summary>

        private Dictionary<int, MicroPackageInfo> _packageDic = new Dictionary<int, MicroPackageInfo>();
        /// <summary>
        /// 记录原始被使用了的节点Id
        /// </summary>
        private HashSet<int> _useNodes = new HashSet<int>();
        #endregion

        #region 运行时数据

        /// <summary>
        /// 运行时节点缓存
        /// </summary>
        private Dictionary<int, RuntimeNodeCache> _runtimeNodes = new Dictionary<int, RuntimeNodeCache>();

        #endregion

        /// <summary>
        /// 自定义用户数据
        /// </summary>
        public object UserData { get; set; }

#if UNITY_EDITOR
        [SerializeField]
        internal MicroGraphEditorInfo editorInfo = new MicroGraphEditorInfo();
#endif

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            _varDic.Clear();
            _nodeDic.Clear();
            _packageDic.Clear();
            foreach (var item in _variables)
            {
                item.MicroGraph = this;
                _varDic.Add(item.Name, item);
            }
            foreach (var item in Nodes)
            {
                _nodeDic.Add(item.OnlyId, item);
            }
            Nodes.ForEach(n => n.Initialize(this));
            foreach (var item in _packages)
            {
                _packageDic.Add(item.PackageId, item);
            }
        }

        /// <summary>
        /// 根据节点Id获取原始节点
        /// </summary>
        /// <param name="nodeId">节点Id</param>
        /// <returns></returns>
        public BaseMicroNode GetNode(int nodeId)
        {
            return _nodeDic.ContainsKey(nodeId) ? _nodeDic[nodeId] : null;
        }
        /// <summary>
        /// 根据节点Id获取运行时节点
        /// </summary>
        /// <param name="runtimeUniqueId">运行时唯一Id </param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public BaseMicroNode GetRuntimeNode(int runtimeUniqueId, int nodeId)
        {
            if (_runtimeNodes.ContainsKey(runtimeUniqueId))
                return _runtimeNodes[runtimeUniqueId].GetRuntimeNode(nodeId);
            else
            {
                RuntimeNodeCache cache = new RuntimeNodeCache(runtimeUniqueId, this);
                _runtimeNodes.Add(runtimeUniqueId, cache);
                return cache.GetRuntimeNode(nodeId);
            }
        }
        /// <summary>
        /// 根据变量名字获取当前图中的变量
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public BaseMicroVariable GetVariable(string varName)
        {
            return _varDic.ContainsKey(varName) ? _varDic[varName] : null;
        }
        /// <summary>
        /// 根据变量名字获取当前图中的变量
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public BaseMicroVariable<T> GetVariable<T>(string varName)
        {
            return GetVariable(varName) as BaseMicroVariable<T>;
        }
        /// <summary>
        /// 根据节点包Id获取节点包信息
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public MicroPackageInfo GetPackage(int packageId)
        {
            return _packageDic.ContainsKey(packageId) ? _packageDic[packageId] : null;
        }

        public virtual IMicroGraphClone DeepCopy(IMicroGraphClone clone)
        {
            BaseMicroGraph graph = (BaseMicroGraph)clone;
            graph._onlyId = _onlyId;
            //graph._variables = new List<BaseMicroVariable>();
            //foreach (var item in _variables)
            //    graph._variables.Add((BaseMicroVariable)item.DeepClone());
            //graph._nodes = new List<BaseMicroNode>();
            //foreach (var item in _nodes)
            //    graph._nodes.Add((BaseMicroNode)item.DeepClone());
            //graph._varDic = new Dictionary<string, BaseMicroVariable>();
            //graph._nodeDic = new Dictionary<int, BaseMicroNode>();

            return graph;
        }
        public virtual IMicroGraphClone DeepClone()
        {
            return DeepCopy(Instantiate(this));
        }

        /// <summary>
        /// 移除一个运行时
        /// </summary>
        /// <param name="runtimeUniqueId"></param>
        internal void RemoveRuntime(int runtimeUniqueId)
        {
            if (_runtimeNodes.TryGetValue(runtimeUniqueId, out RuntimeNodeCache cache))
            {
                _runtimeNodes.Remove(runtimeUniqueId);
                cache.Clear();
            }
        }


        /// <summary>
        /// 运行时节点缓存
        /// </summary>
        private class RuntimeNodeCache
        {
            /// <summary>
            /// 运行时Id
            /// </summary>
            public int runtimeUniqueId;
            /// <summary>
            /// 节点缓存字典
            /// </summary>
            private Dictionary<int, BaseMicroNode> _nodeDict = new Dictionary<int, BaseMicroNode>();
            /// <summary>
            /// 微图
            /// </summary>
            private BaseMicroGraph _baseMicroGraph;

            internal RuntimeNodeCache(int runtimeUniqueId, BaseMicroGraph baseMicroGraph)
            {
                this._baseMicroGraph = baseMicroGraph;
            }
            /// <summary>
            /// 获取运行时节点
            /// </summary>
            /// <param name="nodeId"></param>
            /// <returns></returns>
            internal BaseMicroNode GetRuntimeNode(int nodeId)
            {
                if (_nodeDict.ContainsKey(nodeId))
                    return _nodeDict[nodeId];
                else
                {
                    BaseMicroNode originNode = _baseMicroGraph.GetNode(nodeId);
                    if (originNode == null)
                        return null;
                    BaseMicroNode node = (BaseMicroNode)originNode.DeepClone();
                    node.Initialize(_baseMicroGraph);
                    _nodeDict.Add(nodeId, node);
                    return node;
                }
            }

            internal void Clear()
            {
                foreach (var item in _nodeDict.Values)
                {
                    try
                    {
                        item.OnExit();
                    }
                    catch (Exception ex)
                    {
                        MicroGraphLogger.LogError($"节点：{item.GetType().FullName},退出失败：{ex.Message}");
                    }
                }
                _nodeDict.Clear();
            }
        }
    }
}
