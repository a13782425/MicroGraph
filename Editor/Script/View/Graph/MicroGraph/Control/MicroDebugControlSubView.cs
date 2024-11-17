#if MICRO_GRAPH_DEBUG
using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroDebuggerEditorUtils;
using static MicroGraph.Editor.MicroGraphEditorDebugger;

namespace MicroGraph.Editor
{
    [MicroGraphOrder(int.MaxValue)]
    internal class MicroDebugControlSubView : VisualElement, IMicroSubControl
    {
        private BaseMicroGraphView _owner;
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroDebugControlSubView";

        public VisualElement Panel => this;

        public string Name => "调试面板";

        private TextField _ipField;
        private IntegerField _portField;
        /// <summary>
        /// 链接调试器
        /// </summary>
        private Button _debuggerButton;
        /// <summary>
        /// 附加到调试器
        /// </summary>
        private Button _attachButton;
        /// <summary>
        /// 需要调试的微图
        /// </summary>
        private PopupField<string> _debugGraphPopup;
        /// <summary>
        /// 调试器图例
        /// </summary>
        private ScrollView _debuggerLegendView;
        private DebuggerGraphContainerData _containerData;
        private List<string> _debugGraphList = new List<string>();
        private Dictionary<int, GraphView.Layer> _layerDic;
        private string _curDebugGraphName = "";
        public MicroDebugControlSubView(BaseMicroGraphView owner)
        {
            this._owner = owner;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microdebugcontrol");
            this._ipField = new TextField("Ip: ");
            this._ipField.value = "127.0.0.1";
            this._ipField.maxLength = "127.000.000.001".Length;
            this._portField = new IntegerField("Port: ");
            this._portField.maxLength = 5;
            this._portField.value = 65500;
            this._debuggerButton = new Button(m_debuggerClick);
            this._attachButton = new Button(m_attachClick);
            this._attachButton.text = "附加到调试器";
            this._debugGraphPopup = new PopupField<string>("微图名:", _debugGraphList, -1);
            this._debugGraphPopup.AddToClassList("debug_graph_popup");
            this._debugGraphPopup.RegisterValueChangedCallback(m_debugGraphPopupChanged);
            this._debuggerLegendView = new ScrollView(ScrollViewMode.Vertical);
            this._debuggerLegendView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            this._debuggerLegendView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            this.Add(this._ipField);
            this.Add(this._portField);
            this.Add(this._debuggerButton);
            this.Add(this._attachButton);
            this.Add(this._debugGraphPopup);
            this.Add(this._debuggerLegendView);
            owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_STATE_CHANGED, m_onDebuggerStateChanged);
            FieldInfo fieldInfo = typeof(GraphView).GetField("m_ContainerLayers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _layerDic = (Dictionary<int, GraphView.Layer>)fieldInfo.GetValue(owner.View);
            m_refreshConnectState();
            m_refreshLegend();
        }

        private void m_debugGraphPopupChanged(ChangeEvent<string> evt)
        {
            _curDebugGraphName = evt.newValue;
            if (string.IsNullOrWhiteSpace(_curDebugGraphName))
            {
                _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_NODE_DATA_CHANGED);
                _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED);
            }
            else
            {
                if (!_containerData.DebugGraphEditorData.TryGetValue(_curDebugGraphName, out DebuggerGraphEditorData editorData))
                    return;
                _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_NODE_DATA_CHANGED, _curDebugGraphName);
                _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED, _curDebugGraphName);
            }
        }
        private void m_attachClick()
        {
            switch (_owner.DebuggerState)
            {
                case BaseMicroGraphView.MicroGraphDebuggerState.None:
                    _owner.DebuggerState = BaseMicroGraphView.MicroGraphDebuggerState.Attach;
                    m_registerReceiveNetData();
                    this._attachButton.text = "从调试器分离";
                    this._debugGraphPopup.SetDisplay(true);
                    _curDebugGraphName = null;
                    this._debugGraphPopup.index = -1;
                    this.m_setGraphViewEnable(false);
                    break;
                case BaseMicroGraphView.MicroGraphDebuggerState.Attach:
                    _owner.DebuggerState = BaseMicroGraphView.MicroGraphDebuggerState.None;
                    m_unregisterReceiveNetData();
                    this._attachButton.text = "附加到调试器";
                    this._debugGraphPopup.SetDisplay(false);
                    this.m_setGraphViewEnable(true);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 设置是否enable
        /// </summary>
        /// <param name="isEnable"></param>
        private void m_setGraphViewEnable(bool isEnable)
        {
            foreach (var item in _layerDic)
            {
                if (item.Key != MicroDebuggerEditorUtils.DEBUGGER_LAYER)
                {
                    item.Value.SetEnabled(isEnable);
                }
            }
        }
        private void m_debuggerClick()
        {
            switch (MicroGraphEditorDebugger.NetState)
            {
                case TcpConnectState.Disconnect:
                    MicroGraphEditorDebugger.Connect(this._ipField.value, _portField.value);
                    break;
                case TcpConnectState.Connecting:
                    break;
                case TcpConnectState.Connected:
                    MicroGraphEditorDebugger.Disconnect();
                    break;
                default:
                    break;
            }
        }
        private bool m_onDebuggerStateChanged(object args)
        {
            m_refreshConnectState();
            return true;
        }

        private void m_refreshConnectState()
        {
            switch (MicroGraphEditorDebugger.NetState)
            {
                case TcpConnectState.Disconnect:
                    this._debuggerButton.text = "开始调试";
                    this._debuggerButton.SetEnabled(true);
                    this._ipField.SetEnabled(true);
                    this._portField.SetEnabled(true);
                    this._attachButton.SetEnabled(false);
                    this._debugGraphPopup.SetDisplay(false);
                    _owner.DebuggerState = BaseMicroGraphView.MicroGraphDebuggerState.None;
                    this._attachButton.text = "附加到调试器";
                    this.m_setGraphViewEnable(true);
                    break;
                case TcpConnectState.Connecting:
                    this._debuggerButton.text = "连接中...";
                    this._debuggerButton.SetEnabled(false);
                    this._ipField.SetEnabled(false);
                    this._portField.SetEnabled(false);
                    this._attachButton.SetEnabled(false);
                    this._debugGraphPopup.SetDisplay(false);
                    break;
                case TcpConnectState.Connected:
                    this._debuggerButton.text = "停止调试";
                    this._debuggerButton.SetEnabled(true);
                    this._ipField.SetEnabled(false);
                    this._portField.SetEnabled(false);
                    this._attachButton.SetEnabled(true);
                    this._debugGraphPopup.SetDisplay(false);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 刷新图例
        /// </summary>
        private void m_refreshLegend()
        {
            foreach (var item in Enum.GetValues(typeof(NodeState)))
            {
                NodeState state = (NodeState)item;
                if (DebuggerNodeStates.TryGetValue(state, out MicroDebuggerNodeState nodeState))
                {
                    MicroNodeStateLegendVisualElement legendVisual = new MicroNodeStateLegendVisualElement(nodeState);
                    _debuggerLegendView.Add(legendVisual);
                }
            }
        }
        private bool m_onDebuggerGraphDataChanged(object args)
        {
            if (args is not DebuggerGraphData graphData)
                return true;
            if (graphData.microGraphId != _owner.Target.OnlyId)
                return true;
            if (_containerData == null)
                MicroGraphEditorDebugger.DebugGraphContainerDatas.TryGetValue(_owner.Target.OnlyId, out _containerData);
            if (!this._debugGraphList.Contains(graphData.runtimeName))
                this._debugGraphList.Add(graphData.runtimeName);
            if (!_containerData.DebugGraphEditorData.TryGetValue(graphData.runtimeName, out DebuggerGraphEditorData graphEditorData))
                return true;
            bool isSend = _curDebugGraphName == graphData.runtimeName;
            if (isSend)
            {
                foreach (var item in graphData.nodeDatas)
                    _owner.listener.OnEvent(item.nodeId, _curDebugGraphName);
                if (graphEditorData.varDatas.Count > 0)
                    _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED, _curDebugGraphName);
            }
            return true;
        }
        private bool m_onDebuggerGraphDeleteDataChanged(object args)
        {
            if (args is not DebuggerGraphDeleteData graphData)
                return true;
            if (graphData.microGraphId != _owner.Target.OnlyId)
                return true;
            this._debugGraphList.Remove(graphData.runtimeName);

            if (_curDebugGraphName == graphData.runtimeName)
                this._debugGraphPopup.index = -1;
            return true;
        }
        private bool m_onDebuggerGraphRenameDataChanged(object args)
        {
            if (args is not DebuggerGraphRenameData graphData)
                return true;
            if (graphData.microGraphId != _owner.Target.OnlyId)
                return true;
            if (_containerData == null)
                MicroGraphEditorDebugger.DebugGraphContainerDatas.TryGetValue(_owner.Target.OnlyId, out _containerData);
            this._debugGraphList.Remove(graphData.oldName);
            this._debugGraphList.Add(graphData.newName);
            if (_curDebugGraphName == graphData.oldName)
            {
                this._debugGraphPopup.index = -1;
            }
            return true;
        }
        private bool m_onDebuggerVarDataChanged(object args)
        {
            if (args is not DebuggerVarData varData)
                return true;
            if (varData.microGraphId != _owner.Target.OnlyId)
                return true;
            if (varData.runtimeName != this._curDebugGraphName)
                return true;
            _owner.listener.OnEvent(MicroGraphEventIds.DEBUGGER_LOCAL_VAR_DATA_CHANGED, varData.runtimeName);
            return true;
        }

        private bool m_onDebuggerNodeDataChanged(object args)
        {
            if (args is not DebuggerNodeData nodeData)
                return true;
            if (nodeData.microGraphId != _owner.Target.OnlyId)
                return true;
            if (nodeData.runtimeName != this._curDebugGraphName)
                return true;
            _owner.listener.OnEvent(nodeData.nodeId, nodeData.runtimeName);
            return true;
        }
        /// <summary>
        /// 注册网络消息回调
        /// </summary>
        private void m_registerReceiveNetData()
        {
            _debugGraphList.Clear();
            if (MicroGraphEditorDebugger.DebugGraphContainerDatas.TryGetValue(_owner.Target.OnlyId, out DebuggerGraphContainerData value))
            {
                _containerData = value;
                _debugGraphList.AddRange(_containerData.DebugGraphEditorData.Keys);
            }
            _owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPH_DATA_CHANGED, m_onDebuggerGraphDataChanged);
            _owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHRENAME_DATA_CHANGED, m_onDebuggerGraphRenameDataChanged);
            _owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHDELETE_DATA_CHANGED, m_onDebuggerGraphDeleteDataChanged);
            _owner.listener.AddListener(MicroGraphEventIds.DEBUGGER_GLOBAL_VAR_DATA_CHANGED, m_onDebuggerVarDataChanged);
        }

        /// <summary>
        /// 取消注册网络消息回调
        /// </summary>
        private void m_unregisterReceiveNetData()
        {
            _owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPH_DATA_CHANGED, m_onDebuggerGraphDataChanged);
            _owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHRENAME_DATA_CHANGED, m_onDebuggerGraphRenameDataChanged);
            _owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_GLOBAL_GRAPHDELETE_DATA_CHANGED, m_onDebuggerGraphDeleteDataChanged);
            _owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_GLOBAL_VAR_DATA_CHANGED, m_onDebuggerVarDataChanged);
            _debugGraphList.Clear();
            _containerData = null;
        }
        public void Show()
        {
        }

        public void Hide()
        {
        }
        public void Exit()
        {
            m_unregisterReceiveNetData();
            _owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_STATE_CHANGED, m_onDebuggerStateChanged);
        }

        /// <summary>
        /// 节点状态示例
        /// </summary>
        private class MicroNodeStateLegendVisualElement : VisualElement
        {
            private MicroDebuggerNodeState nodeState;
            private VisualElement bg;
            private Label label;
            public MicroNodeStateLegendVisualElement(MicroDebuggerNodeState nodeState)
            {
                this.AddToClassList("state_legend");
                this.nodeState = nodeState;
                bg = new VisualElement();
                bg.style.backgroundColor = nodeState.color;
                bg.AddToClassList("legend_bg");
                label = new Label();
                label.text = nodeState.name;
                label.AddToClassList("legend_label");
                this.Add(bg);
                this.Add(label);
            }
        }
    }
}

#endif