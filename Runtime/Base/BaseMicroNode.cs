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

        [NonSerialized]
        private NodeState _state = NodeState.None;
        /// <summary>
        /// 节点状态
        /// </summary>
        public NodeState State { get => _state; protected set => _state = value; }
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
        /// </summary>
        public virtual bool OnInit() { _state = NodeState.Inited; return true; }
        /// <summary>
        /// 节点在执行前候调用
        /// </summary>
        public virtual bool OnEnable() { _state = NodeState.Running; return true; }
        /// <summary>
        /// 节点执行调用的方法
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExecute() { _state = NodeState.Success; return true; }
        /// <summary>
        /// 当一帧没有执行完毕，下一帧执行Update
        /// </summary>
        /// <returns></returns>
        public virtual bool OnUpdate(float deltaTime, float unscaledDeltaTime) => true;

        /// <summary>
        /// 节点停止调用
        /// 只有正在执行的节点才会被调用
        /// 如果微图正在执行时退出，会先执行正在运行节点的OnStop方法再执行OnExit
        /// </summary>
        /// <returns></returns>
        public virtual bool OnStop() => true;

        /// <summary>
        /// 节点退出
        /// 当微图完成时候
        /// 无论微图是否正常退出全部节点都会执行
        /// 如果微图正在执行时退出，会先执行正在运行节点的OnStop方法再执行OnExit
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExit() => true;
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetChild() => Childs;

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
    }
}
