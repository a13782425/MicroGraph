using System;
using System.Diagnostics;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 节点入接口
    /// 字段必须是可以序列化的才有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class NodeInputAttribute : Attribute { }

    /// <summary>
    /// 节点出接口
    /// 字段必须是可以序列化的才有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class NodeOutputAttribute : Attribute { }
    /// <summary>
    /// 节点名字
    /// 字段必须是可以序列化的才有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class NodeFieldNameAttribute : Attribute
    {
        public string name { get; }
        public NodeFieldNameAttribute(string name)
        {
            this.name = name;
        }

    }
}
