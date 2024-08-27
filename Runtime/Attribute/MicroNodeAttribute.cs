using System;
using System.Diagnostics;

namespace MicroGraph.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class MicroNodeAttribute : Attribute
    {
        /// <summary>
        /// 微图节点名字
        /// </summary>
        public string NodeName { get; private set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Describe = string.Empty;

        /// <summary>
        /// 拥有什么端口
        /// </summary>
        public PortDirEnum PortDir = PortDirEnum.All;
        /// <summary>
        /// 节点端口是否横向显示
        /// 默认:横向
        /// </summary>
        public bool IsHorizontal = true;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable = true;
        /// <summary>
        /// 节点类型
        /// </summary>
        public MicroNodeType NodeType = MicroNodeType.Flow;
        public MicroNodeAttribute(string str)
        {
            NodeName = str;
        }
    }
}
