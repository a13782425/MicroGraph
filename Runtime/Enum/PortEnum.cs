using System;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 端口方向枚举
    /// </summary>
    [Flags]
    public enum PortDirEnum
    {
        /// <summary>
        /// 没有端口
        /// </summary>
        None = 0,
        /// <summary>
        /// 只有进
        /// </summary>
        In = 1,
        /// <summary>
        /// 只有出
        /// </summary>
        Out = 2,
        /// <summary>
        /// 二者皆有
        /// </summary>
        All = In | Out
    }
}
