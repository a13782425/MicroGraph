using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 控制器
    /// </summary>
    internal interface IMicroSubControl
    {
        void Show();
        void Hide();
    }

    /// <summary>
    /// 控制面板
    /// </summary>
    internal sealed partial class MicroControlView : GraphElement
    {
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroControlView";
        private BaseMicroGraphView _owner;
        private VisualElement _contentContainer;
        private VisualElement _titleContainer;
        private List<ControlModel> _controls = new List<ControlModel>();
        private TabbedView _tabbedView;
        private TabButton _tabVarButton;
        private TabButton _tabGraphButton;
        private TabButton _tabNodeButton;
        private TabButton _tabTemplateButton;

        public MicroVariableControlSubView VariableControlView { get; private set; }
        public MicroGraphControlSubView GraphControlView { get; private set; }
        public MicroNodeControlSubView NodeControlView { get; private set; }
        public MicroTemplateControlSubView TemplateControlSubView { get; private set; }

        public override VisualElement contentContainer => _contentContainer;
        public MicroControlView(BaseMicroGraphView graph)
        {
            this._owner = graph;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microcontrol");
            this.capabilities |= Capabilities.Movable;
            this.AddManipulator(new Dragger() { clampToParentEdges = true });
            this.AddManipulator(new ContextualMenuManipulator(onBuildContextualMenu));
            this.capabilities |= Capabilities.Resizable;
            _titleContainer = new VisualElement();
            _titleContainer.AddToClassList("microcontrol_titlecontainer");
            Label label = new Label("控制面板");
            _titleContainer.Add(label);
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("microcontrol_contentcontainer");
            base.hierarchy.Add(_titleContainer);
            base.hierarchy.Add(_contentContainer);
            base.hierarchy.Add(new ResizableElement());
            _tabbedView = new TabbedView();
            this._contentContainer.Add(_tabbedView);

            VariableControlView = new MicroVariableControlSubView(_owner);
            GraphControlView = new MicroGraphControlSubView(_owner);
            NodeControlView = new MicroNodeControlSubView(_owner);
            TemplateControlSubView = new MicroTemplateControlSubView(_owner);

            _tabVarButton = new TabButton("变量控制", VariableControlView);
            _tabTemplateButton = new TabButton("微图模板", TemplateControlSubView);
            _tabNodeButton = new TabButton("节点控制", NodeControlView);
            _tabGraphButton = new TabButton("微图控制", GraphControlView);

            _tabVarButton.OnSelect += m_tabbutton_OnSelect;
            _tabVarButton.OnClose += m_tabbutton_OnClose;
            _tabGraphButton.OnSelect += m_tabbutton_OnSelect;
            _tabGraphButton.OnClose += m_tabbutton_OnClose;
            _tabNodeButton.OnSelect += m_tabbutton_OnSelect;
            _tabNodeButton.OnClose += m_tabbutton_OnClose;
            _tabTemplateButton.OnSelect += m_tabbutton_OnSelect;
            _tabTemplateButton.OnClose += m_tabbutton_OnClose;

            _tabbedView.AddTab(_tabVarButton, true);
            _tabbedView.AddTab(_tabTemplateButton, false);
            _tabbedView.AddTab(_tabNodeButton, false);
            _tabbedView.AddTab(_tabGraphButton, false);
            _tabbedView.scrollable = true;
            //SetPosition(new Rect(new Vector2(0, 36), new Vector2(200, 320)));
        }

        private void m_tabbutton_OnClose(TabButton obj)
        {
            if (obj.Target is IMicroSubControl control)
            {
                control.Hide();
            }
        }

        private void m_tabbutton_OnSelect(TabButton obj)
        {
            if (obj.Target is IMicroSubControl control)
            {
                control.Show();
            }
        }

        private void onBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopPropagation();
        }
        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);
            if (evt is WheelEvent)
            {
                evt.StopPropagation();
            }
        }
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt is WheelEvent)
            {
                evt.StopPropagation();
            }
        }

        internal void Exit()
        {
            VariableControlView.Exit();
            GraphControlView.Exit();
            NodeControlView.Exit();
            TemplateControlSubView.Exit();
        }

        private class ControlModel
        {
            public string name;
            public Button button;
            public VisualElement panel;
        }
    }
}
