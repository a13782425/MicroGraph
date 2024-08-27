using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public class TabbedView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabbedView, UxmlTraits> { }
        private const string STYLE_PATH = "Uss/Element/TabbedView";
        private const string k_styleName = "TabbedView";
        private const string s_UssClassName = "unity-tabbed-view";
        private const string s_ContentContainerClassName = "unity-tabbed-view__content-container";
        private const string s_TabsContainerClassName = "unity-tabbed-view__tabs-container";

        private readonly VisualElement m_TabContent;
        private readonly VisualElement m_Content;

        private readonly List<TabButton> m_Tabs = new List<TabButton>();
        private TabButton m_ActiveTab;

        public override VisualElement contentContainer => m_Content;

        private ScrollView _scrollView;

        private bool _scrollable = false;
        public bool scrollable
        {
            get => _scrollable;
            set
            {
                _scrollable = true;
                if (_scrollable)
                {
                    if (_scrollView == null)
                    {
                        _scrollView = new ScrollView(ScrollViewMode.Horizontal);
                        _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                        _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                        _scrollView.AddToClassList("unity-tabs-scrollview");
                    }
                    m_TabContent.RemoveFromHierarchy();
                    _scrollView.Add(m_TabContent);
                    hierarchy.Insert(0, _scrollView);
                    foreach (var item in m_Tabs)
                        item.AddToClassList("scrollable");
                }
                else
                {
                    m_TabContent.RemoveFromHierarchy();
                    hierarchy.Add(m_TabContent);
                    foreach (var item in m_Tabs)
                        item.RemoveFromClassList("scrollable");
                }
            }
        }

        public TabbedView()
        {
            AddToClassList(s_UssClassName);

            this.AddStyleSheet(STYLE_PATH);

            m_TabContent = new VisualElement();
            m_TabContent.name = "unity-tabs-container";
            m_TabContent.AddToClassList(s_TabsContainerClassName);
            hierarchy.Add(m_TabContent);

            m_Content = new VisualElement();
            m_Content.name = "unity-content-container";
            m_Content.AddToClassList(s_ContentContainerClassName);
            hierarchy.Add(m_Content);

            RegisterCallback<AttachToPanelEvent>(ProcessEvent);
            RegisterCallback<WheelEvent>(e => e.StopPropagation());
        }

        public void AddTab(TabButton tabButton, bool activate)
        {
            m_Tabs.Add(tabButton);
            m_TabContent.Add(tabButton);

            tabButton.OnClose += RemoveTab;
            tabButton.OnSelect += Activate;

            if (activate)
            {
                Activate(tabButton);
            }
        }

        public void RemoveTab(TabButton tabButton)
        {
            int index = m_Tabs.IndexOf(tabButton);

            // If this tab is the active one make sure we deselect it first...
            if (m_ActiveTab == tabButton)
            {
                DeselectTab(tabButton);
                m_ActiveTab = null;
            }

            m_Tabs.RemoveAt(index);
            m_TabContent.Remove(tabButton);

            tabButton.OnClose -= RemoveTab;
            tabButton.OnSelect -= Activate;

            // If we closed the active tab AND we have any tabs left - active the next valid one...
            if ((m_ActiveTab == null) && m_Tabs.Any())
            {
                int clampedIndex = Mathf.Clamp(index, 0, m_Tabs.Count - 1);
                TabButton tabToActivate = m_Tabs[clampedIndex];

                Activate(tabToActivate);
            }
        }

        private void ProcessEvent(AttachToPanelEvent e)
        {
            // This code takes any existing tab buttons and hooks them into the system...
            for (int i = 0; i < m_Content.childCount; ++i)
            {
                VisualElement element = m_Content[i];

                if (element is TabButton button)
                {
                    m_Content.Remove(element);

                    if (button.Target == null)
                    {
                        string targetId = button.TargetId;

                        button.Target = this.Q(targetId);
                    }
                    AddTab(button, false);
                    --i;
                }
                else
                {
                    element.style.display = DisplayStyle.None;
                }
            }

            // Finally, if we need to, activate this tab...
            if (m_ActiveTab != null)
            {
                SelectTab(m_ActiveTab);
            }
            else if (m_TabContent.childCount > 0)
            {
                m_ActiveTab = (TabButton)m_TabContent[0];

                SelectTab(m_ActiveTab);
            }
        }

        private void SelectTab(TabButton tabButton)
        {
            VisualElement target = tabButton.Target;

            tabButton.Select();
            if (target != null)
                Add(target);
        }

        private void DeselectTab(TabButton tabButton)
        {
            VisualElement target = tabButton.Target;

            if (target != null)
                Remove(target);
            tabButton.Deselect();
        }

        public void Activate(TabButton button)
        {
            if (m_ActiveTab != null)
            {
                DeselectTab(m_ActiveTab);
            }

            m_ActiveTab = button;
            SelectTab(m_ActiveTab);
        }
    }
}
