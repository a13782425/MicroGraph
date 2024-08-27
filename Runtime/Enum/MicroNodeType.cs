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
}
