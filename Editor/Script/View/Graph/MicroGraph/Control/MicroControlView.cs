using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 控制器
    /// </summary>
    public interface IMicroSubControl
    {
        string Name { get; }
        VisualElement Panel { get; }
        void Show();
        void Hide();
        void Exit();
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

            foreach (var controlType in TypeCache.GetTypesDerivedFrom<IMicroSubControl>())
            {
                if (controlType.IsEnum || controlType.IsAbstract)
                    continue;
                var control = (IMicroSubControl)System.Activator.CreateInstance(controlType, new object[] { _owner });
                var orderAttr = controlType.GetCustomAttribute<MicroGraphOrderAttribute>();
                var controlModel = new ControlModel();
                if (orderAttr != null)
                    controlModel.order = orderAttr.Order;
                controlModel.name = control.Name;
                controlModel.control = control;
                controlModel.tabButton = new TabButton(controlModel.name, controlModel.control.Panel);
                _controls.Add(controlModel);
            }
            _controls.Sort((a, b) => a.order.CompareTo(b.order));
            for (int i = 0; i < _controls.Count; i++)
            {
                var controlModel = _controls[i];
                _tabbedView.AddTab(controlModel.tabButton, i == 0);
                controlModel.tabButton.OnSelect += m_tabbutton_OnSelect;
                controlModel.tabButton.OnClose += m_tabbutton_OnClose;
            }
            _tabbedView.scrollable = true;
            GetControl<MicroVariableControlSubView>()?.Show();
        }

        public T GetControl<T>() where T : IMicroSubControl
        {
            foreach (var controlModel in _controls)
            {
                if (controlModel.control is T)
                {
                    return (T)controlModel.control;
                }
            }
            return default;
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
            foreach (var controlModel in _controls)
            {
                controlModel.control.Exit();
            }
        }

        private class ControlModel
        {
            public string name;
            public int order;
            public IMicroSubControl control;
            public TabButton tabButton;
        }
    }
}
