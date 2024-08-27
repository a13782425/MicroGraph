using System;
using System.Collections.Generic;
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
        /// <summary>
        /// 获取变量的值
        /// </summary>
        /// <returns></returns>
        public virtual object GetValue() => null;
        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetValue(object value) { }
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
            BaseMicroVariable var = (BaseMicroVariable)target;
            var._name = this._name;
            return var;
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
        public virtual T value { get => _value; set => _value = value; }
        public override object GetValue() => value;
        public void SetValue(T value) => _value = value;
        public override void SetValue(object value) => _value = (T)value;

        public override Type GetValueType() => typeof(T);
    }
}
