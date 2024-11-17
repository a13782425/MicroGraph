using System;
using System.Diagnostics;

namespace MicroGraph.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class MicroNodeAttribute : Attribute
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
        /// 节点标题颜色
        /// </summary>
        public NodeTitleColorType NodeTitleColor = NodeTitleColorType.Default;
        /// <summary>
        /// 节点端口是否横向显示
        /// 默认:横向
        /// </summary>
        public bool IsHorizontal = true;
        /// <summary>
        /// 最小宽度
        /// <para>默认: -1 自动计算</para>
        /// </summary>
        public int MinWidth = -1;
        /// <summary>
        /// 节点启用状态
        /// </summary>
        public MicroNodeEnableState EnableState = MicroNodeEnableState.Enabled;
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
