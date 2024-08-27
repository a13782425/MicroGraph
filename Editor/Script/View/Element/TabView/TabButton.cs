﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public class TabButton : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<TabButton, UxmlTraits> { }

        internal new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };
            private readonly UxmlStringAttributeDescription m_Target = new UxmlStringAttributeDescription { name = "target" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                TabButton item = ve as TabButton;

                item.m_Label.text = m_Text.GetValueFromBag(bag, cc);
                item.TargetId = m_Target.GetValueFromBag(bag, cc);
            }
        }

        static readonly string s_UssClassName = "unity-tab-button";
        static readonly string s_UssActiveClassName = s_UssClassName + "--active";
        private const string STYLE_PATH = "Uss/Element/TabButton";
        private const string UXML_PATH = "UXML/Element/TabButton";

        private Label m_Label;

        public bool IsCloseable { get; set; }
        public string TargetId { get; private set; }
        public VisualElement Target { get; set; }

        public event Action<TabButton> OnSelect;
        public event Action<TabButton> OnClose;

        public TabButton()
        {
            Init();
        }

        public TabButton(string text, VisualElement target)
        {
            Init();
            m_Label.text = text;
            this.tooltip = text;
            Target = target;
        }

        private void PopulateContextMenu(ContextualMenuPopulateEvent populateEvent)
        {
            DropdownMenu dropdownMenu = populateEvent.menu;

            if (IsCloseable)
            {
                dropdownMenu.AppendAction("Close Tab", e => OnClose(this));
            }
        }

        private void CreateContextMenu(VisualElement visualElement)
        {
            ContextualMenuManipulator menuManipulator = new ContextualMenuManipulator(PopulateContextMenu);

            visualElement.focusable = true;
            visualElement.pickingMode = PickingMode.Position;
            visualElement.AddManipulator(menuManipulator);

            visualElement.AddManipulator(menuManipulator);
        }

        private void Init()
        {
            AddToClassList(s_UssClassName);
            this.AddStyleSheet(STYLE_PATH);

            VisualTreeAsset visualTree = MicroGraphUtils.LoadRes<VisualTreeAsset>(UXML_PATH);
            visualTree.CloneTree(this);

            m_Label = this.Q<Label>("Label");

            CreateContextMenu(this);

            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        public void Select()
        {
            AddToClassList(s_UssActiveClassName);

            if (Target != null)
            {
                Target.style.display = DisplayStyle.Flex;
                Target.style.flexGrow = 1;
            }
        }

        public void Deselect()
        {
            RemoveFromClassList(s_UssActiveClassName);
            MarkDirtyRepaint();

            if (Target != null)
            {
                Target.style.display = DisplayStyle.None;
                Target.style.flexGrow = 0;
            }
        }

        private void OnMouseDownEvent(MouseDownEvent e)
        {
            switch (e.button)
            {
                case 0:
                    {
                        OnSelect?.Invoke(this);
                        break;
                    }

                case 2 when IsCloseable:
                    {
                        OnClose?.Invoke(this);
                        break;
                    }
            }        // End of switch.

            e.StopImmediatePropagation();
        }
    }
}
