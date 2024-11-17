using System;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 端口方向枚举
    /// </summary>
    [Flags]
    public enum MicroNodeType
    {
        /// <summary>
        /// 根节点
        /// </summary>
        Root = 0,
        /// <summary>
        /// 流程节点
        /// </summary>
        Flow = 1,
        /// <summary>
        /// 条件节点
        /// </summary>
        Condition = 2,
        /// <summary>
        /// 等待节点
        /// </summary>
        Wait = 3,
    }
    /// <summary>
    /// 节点启用状态
    /// </summary>
    public enum MicroNodeEnableState
    {
        /// <summary>
        /// 节点启用
        /// </summary>
        Enabled = 0,
        /// <summary>
        /// 节点禁用(不能使用但可以查到)
        /// </summary>
        Disabled,
        /// <summary>
        /// 节点排除(既不能使用也不能查到)
        /// </summary>
        Exclude,
    }
    /// <summary>
    /// 节点颜色
    /// </summary>
    public enum NodeTitleColorType
    {
        Default = 0,
        Slate,
        Gray,
        Zinc,
        Neutral,
        Stone,
        Red,
        Orange,
        Amber,
        Yellow,
        Lime,
        Green,
        Emerald,
        Teal,
        Cyan,
        Sky,
        Blue,
        Indigo,
        Violet,
        Purple,
        Fuchsia,
        Pink,
        Rose
    }
}
