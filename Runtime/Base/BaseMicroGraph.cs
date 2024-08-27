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
        private static readonly List<BaseMicroNode> EMPTY_LIST = new List<BaseMicroNode>();

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
        /// <summary>
        /// 自定义用户数据
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object userData { get; set; }


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
            foreach (var item in _variables)
            {
                _varDic.Add(item.Name, item);
            }
            foreach (var item in Nodes)
            {
                _nodeDic.Add(item.OnlyId, item);
            }
            Nodes.ForEach(n => n.Initialize(this));
        }
        /// <summary>
        /// 获取开始节点
        /// 开始节点可以有多个
        /// </summary>
        /// <returns></returns>
        public virtual List<BaseMicroNode> GetStartNode() => EMPTY_LIST;

        /// <summary>
        /// 根据节点Id获取节点
        /// </summary>
        /// <param name="nodeId">节点Id</param>
        /// <returns></returns>
        public BaseMicroNode GetNode(int nodeId)
        {
            return _nodeDic.ContainsKey(nodeId) ? _nodeDic[nodeId] : null;
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

        public virtual IMicroGraphClone DeepCopy(IMicroGraphClone clone)
        {
            BaseMicroGraph graph = (BaseMicroGraph)clone;
            graph._onlyId = _onlyId;
            graph._variables = new List<BaseMicroVariable>();
            foreach (var item in _variables)
                graph._variables.Add((BaseMicroVariable)item.DeepClone());
            graph._nodes = new List<BaseMicroNode>();
            foreach (var item in _nodes)
                graph._nodes.Add((BaseMicroNode)item.DeepClone());
            graph._varDic = new Dictionary<string, BaseMicroVariable>();
            graph._nodeDic = new Dictionary<int, BaseMicroNode>();
            return graph;
        }
        public virtual IMicroGraphClone DeepClone()
        {
            return DeepCopy((IMicroGraphClone)this.MemberwiseClone());
        }
    }
}
