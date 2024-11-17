#if MICRO_GRAPH_DEBUG
using MicroGraph.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroDebuggerEditorUtils;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 节点调试视图
    /// </summary>
    internal class MicroNodeDebuggerView : GraphElement
    {
        private const string STYLE_PATH = "Uss/MicroGraph/Debugger/MicroNodeDebuggerView";
        private VisualElement _contentContainer;
        private BaseMicroNodeView _view;
        private Attacher _attacher;
        private MicroDebuggerNodeState _debuggerNodeState;
        private bool _isPlayAnim = false;
        private bool _isOut = false;
        private float _intervalTime;
        private string _graphId;
        private DebuggerGraphContainerData _container;
        public MicroNodeDebuggerView()
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_debugger_node_view");
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("micro_debugger_node_container");
            this.Add(_contentContainer);
            this.layer = MicroDebuggerEditorUtils.DEBUGGER_LAYER;
            this.AddManipulator(new ContextualMenuManipulator(m_onBuildContextualMenu));
        }

        public void Initialize(BaseMicroNodeView nodeView)
        {
            this.RegisterCallback<AttachToPanelEvent>(m_attachToPanel);
            _view = nodeView;
            this._graphId = this._view.owner.Target.OnlyId;
            if (_container != null && _container.microGraphId != this._graphId)
                _container = null;
            MicroGraphUtils.onUpdate -= m_update;
            MicroGraphUtils.onUpdate += m_update;
            m_refreshDebugState(NodeState.None);
            this._view.owner.AddElement(this);
            this._view.owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_LOCAL_NODE_DATA_CHANGED, m_debuggerDataChanged);
            this._view.owner.listener.AddListener(this._view.Target.OnlyId, m_debuggerDataChanged);
        }


        /// <summary>
        /// 禁用
        /// </summary>
        internal void Disable()
        {
            MicroGraphUtils.onUpdate -= m_update;
            _attacher?.Detach();
            this._view.owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_LOCAL_NODE_DATA_CHANGED, m_debuggerDataChanged);
            this._view.owner.listener.RemoveListener(this._view.Target.OnlyId, m_debuggerDataChanged);
            this.RemoveFromHierarchy();
        }
        private bool m_debuggerDataChanged(object args)
        {
            if (_container == null)
                MicroGraphEditorDebugger.DebugGraphContainerDatas.TryGetValue(_graphId, out _container);
            if (_container == null)
                return true;
            string runtimeName = args as string;
            if (!string.IsNullOrWhiteSpace(runtimeName))
            {
                if (_container.DebugGraphEditorData.TryGetValue(runtimeName, out DebuggerGraphEditorData editorData))
                {
                    if (editorData.nodeDatas.TryGetValue(this._view.Target.OnlyId, out NodeState state))
                    {
                        m_refreshDebugState(state);
                    }
                }
            }
            else
            {
                m_refreshDebugState(NodeState.None);
            }
            return true;
        }

        private void m_refreshDebugState(NodeState state)
        {
            _isPlayAnim = false;
            _debuggerNodeState = MicroDebuggerEditorUtils.DebuggerNodeStates[state];
            this.style.backgroundColor = _debuggerNodeState.color;
            if (state == NodeState.Running)
            {
                _isPlayAnim = true;
                _isOut = true;
            }
        }
        private void m_update()
        {
            if (_isPlayAnim)
            {
                Color first, second;
                if (_isOut)
                {
                    first = this._debuggerNodeState.color;
                    second = first;
                    second.a = 0.2f;
                }
                else
                {
                    first = this._debuggerNodeState.color;
                    second = first;
                    first.a = 0.2f;
                }
                _intervalTime += 20;
                this.style.backgroundColor = Color.Lerp(first, second, _intervalTime * 0.001f);
                if (_intervalTime >= 1000)
                {
                    _isOut = !_isOut;
                    _intervalTime = 0;
                }
            }
        }

        private void m_attachToPanel(AttachToPanelEvent evt)
        {
            m_attachToPanel();
        }

        private void m_attachToPanel()
        {
            if (float.IsNaN(this._view.view.localBound.width) || float.IsNaN(this.localBound.width))
            {
                this.schedule.Execute(m_attachToPanel).StartingIn(10);
                return;
            }
            if (_attacher == null)
            {
                _attacher = new Attacher(this, _view.view, UnityEngine.SpriteAlignment.TopCenter);
                _attacher.distance = 0;
            }
            else
                _attacher.Reattach();
            this.style.width = _view.view.localBound.width - 16;
            this.UnregisterCallback<AttachToPanelEvent>(m_attachToPanel);
        }
        private void m_onBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopPropagation();
        }
    }
}

#endif