namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图运行时状态
    /// </summary>
    public enum MicroGraphRuntimeState
    {
        /// <summary>
        /// 非法的
        /// <para>例如该运行时不存在等</para>
        /// </summary>
        Illegality = -1,
        /// <summary>
        /// 未运行
        /// </summary>
        Idle,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 暂停中
        /// 业务手动暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 完成运行
        /// </summary>
        Complete,
        /// <summary>
        /// 退出
        /// <para>调用退出或者释放时候切换此状态</para>
        /// </summary>
        Exit,
        ///// <summary>
        ///// 初始化失败
        ///// </summary>
        //InitFailure,
        /// <summary>
        /// 执行失败
        /// </summary>
        RunningFailure,
        ///// <summary>
        ///// 退出失败
        ///// </summary>
        //ExitFailure
    }

    /// <summary>
    /// 微图运行时结束模式
    /// </summary>
    public enum MicroGraphRuntimeEndMode
    {
        /// <summary>
        /// 没有结束模式
        /// </summary>
        None = 0,
        /// <summary>
        /// 自动结束
        /// </summary>
        Auto,
        /// <summary>
        /// 手动调用Exit方法
        /// </summary>
        Manual,
        /// <summary>
        /// 结束节点模式
        /// </summary>
        EndNode
    }
}
