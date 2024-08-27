using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 在微图编辑中键盘按下的操作
    /// <para>每张微图独立的</para>
    /// </summary>
    public interface IMicroGraphKeyEvent
    {
        /// <summary>
        /// 是否需要按住Ctrl键
        /// </summary>
        bool IsCtrl { get; }
        /// <summary>
        /// 是否需要按住Alt键
        /// </summary>
        bool IsAlt { get; }
        /// <summary>
        /// 是否需要按住Shift键
        /// </summary>
        bool IsShift { get; }
        /// <summary>
        /// 绑定哪个按键
        /// </summary>
        KeyCode Code { get; }

        bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView);
    }

    public abstract class BaseMicroGraphKeyEvent : IMicroGraphKeyEvent
    {
        public virtual bool IsCtrl => true;

        public virtual bool IsAlt => false;

        public virtual bool IsShift => false;

        public abstract KeyCode Code { get; }

        public abstract bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView);
    }
}
