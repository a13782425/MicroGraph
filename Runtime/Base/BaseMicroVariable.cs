using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微变量
    /// </summary>
    [Serializable]
    public abstract class BaseMicroVariable : IMicroGraphClone
    {
        [SerializeField]
        private string _name;
        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get => _name; set => _name = value; }
        private BaseMicroGraph _microGraph;
        /// <summary>
        /// 微图
        /// </summary>
        public BaseMicroGraph MicroGraph { get => _microGraph; internal set => _microGraph = value; }

        /// <summary>
        /// 获取变量的值
        /// </summary>
        /// <returns></returns>
        public virtual object GetValue() => null;
        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetValue(object value)
        {
#if MICRO_GRAPH_DEBUG
            if (!MicroGraphDebugger.IsListener)
                return;
            DebuggerVarData varData = DebuggerVarData.Create();
            varData.varName = _name;
            varData.microGraphId = _microGraph?.OnlyId;
            varData.runtimeName = _microGraph?.name;
            Type valueType = GetValueType();
            if (valueType == null)
            {
                varData.data = "null";
            }
            else if (valueType.IsClass)
            {
                if (value is UnityEngine.Object unityObj)
                {
                    varData.data = unityObj.name;
                }
                else if (value == null)
                {
                    varData.data = "null";
                }
                else
                {
                    varData.data = value.ToString();
                }
            }
            else
            {
                varData.data = value.ToString();
            }
            MicroGraphDebugger.AddGraphVarData(varData);
#endif
        }
        /// <summary>
        /// 是否存在默认值
        /// </summary>
        public virtual bool HasDefaultValue => true;
        /// <summary>
        /// 获取值的类型
        /// </summary>
        /// <returns></returns>
        public virtual Type GetValueType() => null;
        /// <summary>
        /// 获取显示名
        /// </summary>
        /// <returns></returns>
        public virtual string GetDisplayName() => this.GetValueType()?.Name;
        public virtual IMicroGraphClone DeepCopy(IMicroGraphClone target)
        {
            BaseMicroVariable variable = (BaseMicroVariable)target;
            variable.MicroGraph = MicroGraph;
            variable._name = this._name;
            return variable;
        }

        public virtual IMicroGraphClone DeepClone() => (IMicroGraphClone)this.MemberwiseClone();
    }

    /// <summary>
    /// 微变量
    /// </summary>
    [Serializable]
    public abstract partial class BaseMicroVariable<T> : BaseMicroVariable, IMicroGraphClone
    {
        [SerializeField]
        protected T _value;
        public virtual T value
        {
            get => _value;
            set
            {
                _value = value;
#if MICRO_GRAPH_DEBUG
                base.SetValue(value);
#endif
            }
        }
        public override object GetValue() => value;
        public void SetValue(T value) => this.value = value;
        public override void SetValue(object value)
        {
            _value = (T)value;
#if MICRO_GRAPH_DEBUG
            base.SetValue(value);
#endif
        }
        public override Type GetValueType() => typeof(T);
    }
}
