using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微节点基类
    /// </summary>
    [Serializable]
    public abstract class BaseMicroNode : IMicroGraphClone
    {
        /// <summary>
        /// 空的子节点列表
        /// </summary>
        [NonSerialized]
        public readonly static List<int> EMPTY_CHILD_LIST = new List<int>();

        [SerializeField]
        private int _onlyId;
        /// <summary>
        /// 唯一Id,不可修改
        /// </summary>
        public int OnlyId => _onlyId;
        [SerializeField]
        private List<int> _childs = new List<int>();
        public List<int> Childs => _childs;
        [SerializeReference]
        private List<MicroVariableEdge> _variableEdges = new List<MicroVariableEdge>();
        /// <summary>
        /// 节点所关联的所有变量
        /// </summary>
        public List<MicroVariableEdge> VariableEdges => _variableEdges;

        /// <summary>
        /// 运行时节点版本
        /// <para>节点每运行一次，版本号会发生变化</para>
        /// <para>可以通过变化判断是否需要数据更新或者缓存(由业务层实现)</para>
        /// </summary>
        public int RuntimVersion { get; internal set; }
        /// <summary>
        /// 运行时的唯一ID
        /// </summary>
        internal int RuntimeUniqueId { get; set; }

        [NonSerialized]
        private NodeState _state = NodeState.None;
        /// <summary>
        /// 节点状态
        /// </summary>
        public NodeState State
        {
            get => _state;
            protected set
            {
#if MICRO_GRAPH_DEBUG
                bool isChanged = _state != value;
#endif
                _state = value;
#if MICRO_GRAPH_DEBUG
                if (!isChanged || !MicroGraphDebugger.IsListener)
                    return;
                DebuggerNodeData debuggerData = DebuggerNodeData.Create();
                debuggerData.nodeId = _onlyId;
                debuggerData.runtimeName = _microGraph?.name;
                debuggerData.nodeState = (byte)_state;
                debuggerData.microGraphId = _microGraph?.OnlyId;
                MicroGraphDebugger.AddGraphNodeData(debuggerData);
#endif
            }
        }

        [NonSerialized]
        private BaseMicroGraph _microGraph;
        public BaseMicroGraph microGraph => _microGraph;

        /// <summary>
        /// 自定义用户数据
        /// </summary>
        public object UserData { get; set; }

        public bool Initialize(BaseMicroGraph baseMicroGraph)
        {
            _microGraph = baseMicroGraph;
            _state = NodeState.None;
            return OnInit();
        }
        /// <summary>
        /// 拉取变量值
        /// </summary>
        public virtual void PullVariable() { generatePullVariable(); }
        /// <summary>
        /// 放入变量值
        /// </summary>
        public virtual void PushVariable() { generatePushVariable(); }
        /// <summary>
        /// 节点在创建时执行
        /// <para>仅会执行一次</para>
        /// </summary>
        public virtual bool OnInit() { State = NodeState.Inited; return true; }
        /// <summary>
        /// 节点在执行前候调用
        /// </summary>
        public virtual bool OnEnable() { State = NodeState.Running; return true; }
        /// <summary>
        /// 节点执行调用的方法
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExecute() { State = NodeState.Success; return true; }
        /// <summary>
        /// 当一帧没有执行完毕，下一帧执行Update
        /// </summary>
        /// <returns></returns>
        public virtual bool OnUpdate(float deltaTime, float unscaledDeltaTime) => true;
        /// <summary>
        /// 重置当前节点
        /// 当运行时执行完成时会调用(如果当前节点拥有多个开始节点，则会执行多次)
        /// <para>只有执行过的节点才会被调用</para>
        /// <para>和OnStop互斥</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool OnReset() { State = NodeState.Inited; return true; }
        /// <summary>
        /// 节点停止调用(如果当前节点拥有多个开始节点，则会执行多次)
        /// <para>只有正在执行的节点才会被调用</para>
        /// <para>如果微图正在执行时退出，会先执行正在运行节点的OnStop方法再执行OnExit</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool OnStop() => true;

        /// <summary>
        /// <para>节点退出(如果当前节点拥有多个开始节点，则会执行多次)</para>
        /// <para>无论微图是否正常退出全部节点都会执行</para>
        /// <para>如果微图正在执行时退出，会先执行正在运行节点的OnStop方法再执行OnExit</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExit() { State = NodeState.Exit; return true; }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetChild() => Childs;

        /// <summary>
        /// 获取运行时节点
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        protected BaseMicroNode GetRuntimeNode(int nodeId)
        {
            return microGraph.GetRuntimeNode(RuntimeUniqueId, nodeId);
        }

        /// <summary>
        /// 将当前对象深度拷贝到Target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual IMicroGraphClone DeepCopy(IMicroGraphClone target)
        {
            BaseMicroNode cloneTarget = target as BaseMicroNode;
            if (cloneTarget == null)
            {
                UnityEngine.Debug.LogError("深度拷贝失败:{this.GetType()}");
                return target;
            }
            cloneTarget._onlyId = this._onlyId;
            cloneTarget._childs = new List<int>();
            foreach (var item in this._childs)
                cloneTarget._childs.Add(item);
            cloneTarget._variableEdges = new List<MicroVariableEdge>();
            foreach (var item in this._variableEdges)
                cloneTarget._variableEdges.Add((MicroVariableEdge)item.DeepClone());
            return target;
        }
        /// <summary>
        /// 深度克隆
        /// </summary>
        /// <returns></returns>
        public virtual IMicroGraphClone DeepClone() => DeepCopy((IMicroGraphClone)this.MemberwiseClone());

        /// <summary>
        /// 生成拉去数据代码
        /// </summary>
        protected virtual void generatePullVariable()
        {
            Type type = this.GetType();
            foreach (var variableEdge in this.VariableEdges)
            {
                if (!variableEdge.isInput)
                    continue;
                System.Reflection.FieldInfo fieldInfo = type.GetField(variableEdge.fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fieldInfo == null)
                    continue;
                fieldInfo.SetValue(this, microGraph.GetVariable(variableEdge.varName).GetValue());
            }
        }
        /// <summary>
        /// 生成推送数据代码
        /// </summary>
        protected virtual void generatePushVariable()
        {
            Type type = this.GetType();
            foreach (var variableEdge in this.VariableEdges)
            {
                if (variableEdge.isInput)
                    continue;
                System.Reflection.FieldInfo fieldInfo = type.GetField(variableEdge.fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fieldInfo == null)
                    continue;
                microGraph.GetVariable(variableEdge.varName)?.SetValue(fieldInfo.GetValue(this));
            }
        }

        public override string ToString()
        {
            return $"{OnlyId}-{GetType().FullName}";
        }
    }
}
