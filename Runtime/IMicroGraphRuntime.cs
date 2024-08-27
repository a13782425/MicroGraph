//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MicroGraph.Runtime
//{
//    /// <summary>
//    /// 微图运行状态变化
//    /// </summary>
//    /// <param name="runtime"></param>
//    /// <param name="state"></param>
//    public delegate void MicroGraphStateChanged(IMicroGraphRuntime runtime, MicroGraphState oldState, MicroGraphState newState);
//    /// <summary>
//    /// 微图运行时接口
//    /// </summary>
//    public interface IMicroGraphRuntime
//    {
//        /// <summary>
//        /// 状态变化事件
//        /// </summary>
//        event MicroGraphStateChanged onStateChanged;
//        /// <summary>
//        /// 微图状态
//        /// </summary>
//        MicroGraphState GraphState { get; }
//        /// <summary>
//        /// 单次指定最大节点数量
//        /// <para>避免节点过多时卡死</para>
//        /// </summary>
//        int SingleRunNodeCount { get; set; }
//        /// <summary>
//        /// 执行
//        /// </summary>
//        void PlayGraph();
//        /// <summary>
//        /// 更新
//        /// </summary>
//        /// <param name="deltaTime"></param>
//        /// <param name="unscaledDeltaTime"></param>
//        void UpdateGraph(float deltaTime, float unscaledDeltaTime);
//        /// <summary>
//        /// 重置
//        /// </summary>
//        void ResetGraph();
//        /// <summary>
//        /// 暂停
//        /// </summary>
//        void PauseGraph();
//        /// <summary>
//        /// 暂停恢复
//        /// </summary>
//        void ResumeGraph();
//        /// <summary>
//        /// 退出
//        /// </summary>
//        void ExitGraph();
//    }
//}
