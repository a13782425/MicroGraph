namespace MicroGraph.Runtime
{
    /// <summary>
    /// 节点的状态
    /// </summary>
    public enum NodeState : byte
    {
        None,
        /// <summary>
        /// 完成初始化
        /// </summary>
        Inited,
        /// <summary>
        /// 运行时
        /// </summary>
        Running,
        /// <summary>
        /// 跳过，会跳过该节点的后续节点
        /// </summary>
        Skip,
        /// <summary>
        /// 运行成功
        /// </summary>
        Success,
        /// <summary>
        /// 运行失败
        /// </summary>
        Failed,
        /// <summary>
        /// 退出
        /// </summary>
        Exit,
    }
}
