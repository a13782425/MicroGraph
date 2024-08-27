using System;
using System.Diagnostics;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 标记该微图的一些特征
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class MicroGraphAttribute : Attribute
    {
        /// <summary>
        /// 微图名字
        /// </summary>
        public string GraphName { get; private set; }
        /// <summary>
        /// 当前微图的颜色
        /// </summary>
        public Color? Color { get; set; }

        public MicroGraphAttribute(string str)
        {
            GraphName = str;
        }
    }
}
