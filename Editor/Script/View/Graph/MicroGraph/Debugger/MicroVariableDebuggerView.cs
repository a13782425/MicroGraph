#if MICRO_GRAPH_DEBUG
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 变量节点调试视图
    /// </summary>
    internal class MicroVariableDebuggerView : GraphElement
    {
        private const string STYLE_PATH = "Uss/MicroGraph/Debugger/MicroVariableDebuggerView";
        private VisualElement _contentContainer;
        private MicroVariableNodeView _view;
        private Label _debuggerInfo;
        private Attacher _attacher;
        private string _graphId;
        private DebuggerGraphContainerData _container;

        public MicroVariableDebuggerView()
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_debugger_var_view");
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("micro_debugger_var_container");
            this.Add(_contentContainer);
            _debuggerInfo = new Label();
            _debuggerInfo.AddToClassList("micro_debugger_varinfo_label");
            _debuggerInfo.RegisterCallback<GeometryChangedEvent>(m_infoGeometryChanged);
            this._contentContainer.Add(_debuggerInfo);
            this.layer = MicroDebuggerEditorUtils.DEBUGGER_LAYER;
            this.AddManipulator(new ContextualMenuManipulator(m_onBuildContextualMenu));
        }

        private void m_infoGeometryChanged(GeometryChangedEvent evt)
        {
        }

        public void Initialize(MicroVariableNodeView nodeView)
        {
            this.RegisterCallback<AttachToPanelEvent>(m_attachToPanel);
            _view = nodeView;
            this._graphId = this._view.owner.Target.OnlyId;
            if (_container != null && _container.microGraphId != this._graphId)
                _container = null;
            this._view.owner.AddElement(this);
            this._view.owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED, m_debuggerDataChanged);
        }
        /// <summary>
        /// 禁用
        /// </summary>
        internal void Disable()
        {
            _attacher?.Detach();
            this.UnregisterCallback<AttachToPanelEvent>(m_attachToPanel);
            this._view.owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED, m_debuggerDataChanged);
            this.RemoveFromHierarchy();
        }
        private bool m_debuggerDataChanged(object args)
        {
            if (_container == null)
                MicroGraphEditorDebugger.DebugGraphContainerDatas.TryGetValue(_graphId, out _container);
            if (_container == null)
                return true;
            string runtimeName = args as string;
            if (!(string.IsNullOrWhiteSpace(runtimeName)))
            {
                if (_container.DebugGraphEditorData.TryGetValue(runtimeName, out DebuggerGraphEditorData editorData))
                {
                    if (editorData.varDatas.TryGetValue(this._view.editorInfo.Name, out string value))
                    {
                        _debuggerInfo.text = value;
                        _debuggerInfo.tooltip = value;
                        this.style.width = StyleKeyword.Auto;
                    }
                }
            }
            else
            {
                _debuggerInfo.text = "";
                _debuggerInfo.tooltip = "";
            }
            return true;
        }
        private void m_attachToPanel(AttachToPanelEvent evt)
        {
            m_attachToPanel();
        }

        private void m_attachToPanel()
        {
            if (float.IsNaN(this._view.NodeBorder.localBound.height) || float.IsNaN(this.localBound.height))
            {
                this.schedule.Execute(m_attachToPanel).StartingIn(10);
                return;
            }
            if (_attacher == null)
            {
                _attacher = new Attacher(this, _view.view, UnityEngine.SpriteAlignment.RightCenter);
                _attacher.distance = 0;
                _attacher.offset = new Vector2(-4, -6f);
            }
            else
                _attacher.Reattach();
            this.style.height = _view.NodeBorder.localBound.height - 6;
            this.style.minWidth = 48;
            this.UnregisterCallback<AttachToPanelEvent>(m_attachToPanel);
        }
        private void m_onBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopPropagation();
        }
    }
}
#endif
