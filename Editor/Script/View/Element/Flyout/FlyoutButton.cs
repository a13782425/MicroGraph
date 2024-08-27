using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 侧边栏按钮
    /// </summary>
    public sealed class FlyoutButton : VisualElement
    {
        private const string STYLE_PATH = "Uss/Flyout/FlyoutButton";

        public event Action<VisualElement> onClick;

        public Texture icon { get { return _tabIcon.image; } set { _tabIcon.image = value; } }
        public string text { get { return _contentLabel.text; } set { _contentLabel.text = value; } }

        private Image _tabIcon;

        private Label _contentLabel;

        public FlyoutButton()
        {
            this.AddStyleSheet(STYLE_PATH);
            _tabIcon = new Image();
            _tabIcon.name = "tabIcon";
            this.Add(_tabIcon);
            _contentLabel = new Label();
            _contentLabel.name = "contentLabel";
            this.Add(_contentLabel);

            this.AddManipulator(new Clickable(onToggleClick));
            this.RegisterCallback<MouseLeaveEvent>(this.ExecuteDefaultAction);
            this.RegisterCallback<MouseEnterEvent>(this.ExecuteDefaultAction);
        }
        private void onToggleClick()
        {
            onClick?.Invoke(this);
        }

        /// <summary>
        /// 选中当前按钮
        /// </summary>
        public void Select()
        {
            if (!this.ClassListContains("select"))
            {
                this.AddToClassList("select");
            }
        }
        /// <summary>
        /// 取消选中当前按钮
        /// </summary>
        public void UnSelect()
        {
            this.RemoveFromClassList("select");
        }

        /// <summary>
        /// 放大
        /// </summary>
        public void ZoomIn()
        {
            this.RemoveFromClassList("hide");
            _contentLabel.RemoveFromClassList("hide");
        }
        /// <summary>
        /// 缩小
        /// </summary>
        public void ZoomOut()
        {
            if (!this.ClassListContains("hide"))
            {
                this.AddToClassList("hide");
                _contentLabel.AddToClassList("hide");
            }
        }

        ///// <summary>
        ///// 隐藏
        ///// </summary>
        //public void Hide()
        //{
        //    this.style.display = DisplayStyle.None;
        //}
        ///// <summary>
        ///// 缩小
        ///// </summary>
        //public void Show()
        //{
        //    this.style.display = DisplayStyle.Flex;
        //}
    }
}
