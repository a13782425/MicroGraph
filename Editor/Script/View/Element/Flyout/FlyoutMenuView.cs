using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 侧边栏
    /// </summary>
    public sealed class FlyoutMenuView : VisualElement
    {
        private const string STYLE_PATH = "Uss/Flyout/FlyoutMenuView";

        public string title
        {
            get
            {
                return headerLabel.text;
            }
            set
            {
                headerLabel.text = value;
            }
        }

        private VisualElement layoutContainer;
        private VisualElement headerContainer;
        private ScrollView buttonsScrollViewContainer;

        /// <summary>
        /// 自定义按钮
        /// </summary>
        private List<FlyoutButton> buttons;
        private Image headerIcon;
        private Label headerLabel;

        public FlyoutMenuView()
        {
            this.AddStyleSheet(STYLE_PATH);
            buttons = new List<FlyoutButton>();
            layoutContainer = new VisualElement();
            layoutContainer.AddToClassList("FlyoutMenuView");
            layoutContainer.name = "layoutContainer";
            this.Add(layoutContainer);

            headerContainer = new VisualElement();
            headerContainer.name = "headerContainer";
            headerContainer.AddToClassList("FlyoutMenuView");
            headerContainer.AddToClassList("OptionalContainer");
            layoutContainer.Add(headerContainer);

            headerIcon = new Image();
            headerIcon.name = "headerIcon";
            headerIcon.AddToClassList("headerIcon");
            headerContainer.Add(headerIcon);
            headerLabel = new Label();
            headerLabel.name = "headerLabel";
            headerLabel.AddToClassList("headerLabel");
            headerContainer.Add(headerLabel);

            buttonsScrollViewContainer = new ScrollView();
            buttonsScrollViewContainer.name = "buttonsScrollViewContainer";
            buttonsScrollViewContainer.AddToClassList("FlyoutMenuView");
#if UNITY_2021_1_OR_NEWER
            buttonsScrollViewContainer.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            buttonsScrollViewContainer.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
#else
            buttonsScrollViewContainer.showHorizontal = false;
            buttonsScrollViewContainer.showVertical = false;
#endif
            layoutContainer.Add(buttonsScrollViewContainer);
            headerIcon.AddManipulator(new Clickable(onHeaderClick));
        }


        public FlyoutButton AddButton(string text, string btnTooltip = null)
        {
            FlyoutButton tabButton = new FlyoutButton();
            tabButton.text = text;
            tabButton.tooltip = btnTooltip ?? text;
            buttons.Add(tabButton);
            buttonsScrollViewContainer.Add(tabButton);
            var spaceBlock = new VisualElement();
            spaceBlock.name = "spaceBlock";
            spaceBlock.style.height = 8;
            spaceBlock.style.alignSelf = Align.Center;
            spaceBlock.style.flexShrink = 0;
            buttonsScrollViewContainer.Add(spaceBlock);
            return tabButton;
        }
        private void onHeaderClick()
        {
            if (this.ClassListContains("hide"))
            {
                this.RemoveFromClassList("hide");
                headerContainer.RemoveFromClassList("hide");
                buttonsScrollViewContainer.RemoveFromClassList("hide");
                headerIcon.RemoveFromClassList("hide");
                headerLabel.RemoveFromClassList("hide");
                foreach (var item in buttons)
                {
                    item.ZoomIn();
                }
            }
            else
            {
                this.AddToClassList("hide");
                headerContainer.AddToClassList("hide");
                buttonsScrollViewContainer.AddToClassList("hide");
                headerIcon.AddToClassList("hide");
                headerLabel.AddToClassList("hide");
                foreach (var item in buttons)
                {
                    item.ZoomOut();
                }
            }
        }
    }
}
