namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图状态
    /// </summary>
    public enum MicroGraphState
    {
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
        /// 调试暂停中
        /// 调试器中断
        /// </summary>
        DebugPause,
        /// <summary>
        /// 退出
        /// </summary>
        Exit,
        /// <summary>
        /// 初始化失败
        /// </summary>
        InitFailure,
        /// <summary>
        /// 执行失败
        /// </summary>
        RunningFailure,
        /// <summary>
        /// 退出失败
        /// </summary>
        ExitFailure
    }
}
