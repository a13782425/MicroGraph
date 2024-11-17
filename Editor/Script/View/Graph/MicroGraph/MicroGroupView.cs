using MicroGraph.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图分组
    /// </summary>
    internal sealed partial class MicroGroupView : Group
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroGroupView";
        private BaseMicroGraphView _owner = null;
        private MicroGroupEditorInfo _editorInfo = null;
        internal MicroGroupEditorInfo editorInfo => _editorInfo;
        /// <summary>
        /// 是否可以删除集合
        /// </summary>
        private List<GraphElement> _canRemoveList = new List<GraphElement>();
        /// <summary>
        /// 最后的位置
        /// </summary>
        internal Vector2 LastPos { get; set; }
        private ColorField colorField;


        public MicroGroupView()
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microGroup");
            colorField = new ColorField();
            colorField.AddToClassList("microGroup_palette");
            colorField.RegisterCallback<ChangeEvent<Color>>(m_paletteChange);
            this.headerContainer.Add(colorField);
            m_packageInitialize();
            this.RegisterCallback<GeometryChangedEvent>(m_geometryChanged);
        }

        internal void Initialize(BaseMicroGraphView microGraphView, MicroGroupEditorInfo group)
        {
            this._owner = microGraphView;
            this._editorInfo = group;
            this.title = group.Title;
            this.colorField.value = group.GroupColor;
            m_setColor();
            SetPosition(new Rect(group.Pos, group.Size));
            LastPos = group.Pos;
            m_addNodeView();
            this.AddManipulator(new ContextualMenuManipulator(m_contextMenu));
            if (group.IsPackage)
                m_refreshPackage();
            else
                m_refreshGroup();
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (!editorInfo.IsPackage)
                return base.AcceptsElement(element, ref reasonWhyNotAccepted);
            var nodeView = element as BaseMicroNodeView.InternalNodeView;
            if (nodeView == null)
                return true;
            var packageView = nodeView.nodeView as MicroPackageNodeView;
            if (packageView == null)
                return true;
            if (((MicroPackageNode)packageView.Target).PackageId == editorInfo.GroupId)
            {
                _owner.owner.ShowNotification(new GUIContent("无法添加自身的引用"), MicroGraphUtils.NOTIFICATION_TIME);
                reasonWhyNotAccepted = "无法添加自身包的引用";
                return false;
            }
            return true;
        }

        private void m_refreshGroup()
        {
            _packageContainer.SetDisplay(false);
            _packageIcon.SetDisplay(false);
        }

        private void m_contextMenu(ContextualMenuPopulateEvent evt)
        {
            if (!editorInfo.IsPackage)
            {
                evt.menu.AppendAction("添加模板", (e) =>
                {
                    foreach (var item in this.containedElements)
                        _owner.View.AddToSelection(item);
                    _owner.AddGraphTemplate();
                }, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
            }
            if (editorInfo.IsPackage)
            {
                evt.menu.AppendAction("转为普通分组", m_delPackage, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
                foreach (var item in _owner.editorInfo.Nodes.Where(a => a.Target is MicroPackageNode))
                {
                    MicroPackageNode packageNode = (MicroPackageNode)item.Target;
                    if (packageNode.PackageId == editorInfo.GroupId)
                    {
                        evt.menu.AppendAction($"前往引用/{item.Title}_{item.NodeId}", m_frameNode, DropdownMenuAction.AlwaysEnabled, packageNode);
                    }
                }
            }
            else
            {
                evt.menu.AppendAction("转为包分组", m_addPackage, DropdownMenuAction.AlwaysEnabled);
            }
            evt.StopPropagation();
        }
        private void m_geometryChanged(GeometryChangedEvent evt)
        {
            if (!editorInfo.IsPackage)
                return;
            Rect rect = _packageContainer.layout;
            _packageSymbol.style.left = rect.width - 38;
            _packageSymbol.style.top = rect.height - 38;
            colorField.style.left = rect.width - 56;
            rect = headerContainer.layout;
            int top = (int)((rect.height - colorField.layout.height) * 0.5f - headerContainer.resolvedStyle.borderBottomWidth);
            colorField.style.top = top;
            // _packageSymbol.style.right = 2;
            // _packageSymbol.style.bottom = 2;
            //_packageTooltip.style.right = 38;
            //_packageTooltip.style.bottom = 8;
            //colorField.style.width = 48;
            //_packageSymbol.style.left = StyleKeyword.Auto;
            //_packageSymbol.style.right = 48;
            //_packageSymbol.style.width = 36;
            //_packageSymbol.style.height = 36;
            //_packageTooltip.style.top = rect.height - 48;
            //_packageSymbol.style.left = rect.width - 96;
        }

        private void m_addNodeView()
        {
            foreach (var item in _editorInfo.Nodes)
            {
                GraphElement node = this._owner.GetElement<GraphElement>(item);
                if (node != null)
                {
                    this.AddElement(node);
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _editorInfo.Pos = newPos.position;
            _editorInfo.Size = newPos.size;
        }
        private void m_paletteChange(ChangeEvent<Color> evt)
        {
            _editorInfo.GroupColor = evt.newValue;
            m_setColor();
        }
        private void m_setColor()
        {
            Color groupColor = _editorInfo.GroupColor;
            this.headerContainer.style.backgroundColor = new StyleColor(new Color(groupColor.r, groupColor.g, groupColor.b, 0.5f));
            this.headerContainer.style.borderRightColor = new StyleColor(groupColor);
            this.headerContainer.style.borderLeftColor = new StyleColor(groupColor);
            this.headerContainer.style.borderTopColor = new StyleColor(groupColor);
            this.headerContainer.style.borderBottomColor = new StyleColor(groupColor);
            this.style.borderRightColor = new StyleColor(groupColor);
            this.style.borderLeftColor = new StyleColor(groupColor);
            this.style.borderTopColor = new StyleColor(groupColor);
            this.style.borderBottomColor = new StyleColor(groupColor);
        }
        protected override void OnGroupRenamed(string oldName, string newName)
        {
            if (!MicroGraphUtils.TitleValidity(newName, MicroGraphUtils.EditorConfig.GroupTitleLength))
            {
                title = oldName;
                _owner.owner.ShowNotification(new GUIContent("标题不合法"), MicroGraphUtils.NOTIFICATION_TIME);
                return;
            }
            if (editorInfo.IsPackage)
            {
                foreach (var item in _owner.editorInfo.Groups.Where(a => a.Title == editorInfo.Title))
                {
                    if (item.IsPackage && item.GroupId != this._editorInfo.GroupId)
                    {
                        title = oldName;
                        _owner.owner.ShowNotification(new GUIContent("当前分组名称已被其他分组使用，无法使用"), MicroGraphUtils.NOTIFICATION_TIME);
                        return;
                    }
                }
            }
            _editorInfo.Title = newName;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            IMicroGraphRecordCommand command = null;
            bool isChanged = false;
            foreach (GraphElement element in elements)
            {
                switch (element)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        if (!_editorInfo.Nodes.Contains(nodeView.nodeView.Target.OnlyId))
                        {
                            _editorInfo.Nodes.Add(nodeView.nodeView.Target.OnlyId);
                            command = new MicroGroupAttachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, nodeView.nodeView.Target.OnlyId));
                            _owner.Undo.AddCommand(command);
                            isChanged = true;
                        }
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        if (!_editorInfo.Nodes.Contains(variableView.nodeView.editorInfo.NodeId))
                        {
                            _editorInfo.Nodes.Add(variableView.nodeView.editorInfo.NodeId);
                            command = new MicroGroupAttachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, variableView.nodeView.editorInfo.NodeId));
                            _owner.Undo.AddCommand(command);
                            isChanged = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (isChanged && editorInfo.IsPackage)
                _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
        }
        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {

            base.OnElementsRemoved(elements);
            IMicroGraphRecordCommand command = null;
            bool isChanged = false;
            foreach (GraphElement element in elements)
            {
                switch (element)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        if (_editorInfo.Nodes.Contains(nodeView.nodeView.Target.OnlyId))
                        {
                            _editorInfo.Nodes.Remove(nodeView.nodeView.Target.OnlyId);
                            command = new MicroGroupUnattachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, nodeView.nodeView.Target.OnlyId));
                            _owner.Undo.AddCommand(command);
                            isChanged = true;
                        }
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        if (_editorInfo.Nodes.Contains(variableView.nodeView.editorInfo.NodeId))
                        {
                            _editorInfo.Nodes.Remove(variableView.nodeView.editorInfo.NodeId);
                            command = new MicroGroupUnattachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, variableView.nodeView.editorInfo.NodeId));
                            _owner.Undo.AddCommand(command);
                            isChanged = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (editorInfo.IsPackage && _packageInfo != null)
            {
                foreach (GraphElement element in elements)
                {
                    if (element is BaseMicroNodeView.InternalNodeView nodeView)
                    {
                        bool isContains = false;
                        if (_packageInfo.StartNodes.Contains(nodeView.nodeView.Target.OnlyId))
                        {
                            isContains = true;
                            _owner.View.DeleteElements(_packageStartPort.view.connections);
                        }
                        if (_packageInfo.EndNodes.Contains(nodeView.nodeView.Target.OnlyId))
                        {
                            isContains = true;
                            _owner.View.DeleteElements(_packageEndPort.view.connections);
                        }
                        if (isContains)
                        {
                            _owner.owner.ShowNotification(new GUIContent("关键节点被删除，请查看对应引用"), MicroGraphUtils.NOTIFICATION_TIME);
                        }
                    }
                }
            }
            if (isChanged && editorInfo.IsPackage)
                _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
        }
        public override void OnSelected()
        {
            base.OnSelected();
            Color selectColor = new Color(0.267f, 0.753f, 1);
            this.style.borderRightColor = new StyleColor(selectColor);
            this.style.borderLeftColor = new StyleColor(selectColor);
            this.style.borderTopColor = new StyleColor(selectColor);
            this.style.borderBottomColor = new StyleColor(selectColor);
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            Color groupColor = _editorInfo.GroupColor;
            this.style.borderRightColor = new StyleColor(groupColor);
            this.style.borderLeftColor = new StyleColor(groupColor);
            this.style.borderTopColor = new StyleColor(groupColor);
            this.style.borderBottomColor = new StyleColor(groupColor);
        }

    }
    /// <summary>
    /// 包相关
    /// </summary>
    partial class MicroGroupView
    {
        private VisualElement _packageIcon;
        private VisualElement _packageContainer;
        private VisualElement _packageSymbol;
        private Label _packageTooltip;

        private MicroPort _packageStartPort;
        private MicroPort _packageEndPort;

        private MicroPackageInfo _packageInfo;

        private void m_packageInitialize()
        {
            _packageIcon = new VisualElement();
            _packageIcon.AddToClassList("microGroup_packageicon");
            this.headerContainer.Insert(0, _packageIcon);
            _packageIcon.SetDisplay(false);
            _packageContainer = new VisualElement();
            _packageContainer.AddToClassList("microGroup_packagecontainer");
            this.Q("centralContainer").Add(_packageContainer);
            _packageSymbol = new VisualElement();
            _packageSymbol.AddToClassList("microGroup_packagesymbol");
            _packageContainer.Add(_packageSymbol);
            _packageTooltip = new Label("运行至【结束】退出当前包");
            _packageTooltip.AddToClassList("microGroup_packagetooltip");
            _packageContainer.Add(_packageTooltip);
            _packageStartPort = new MicroPort(MicroPortType.PackagePort, Orientation.Horizontal, Direction.Output, false);
            _packageEndPort = new MicroPort(MicroPortType.PackagePort, Orientation.Horizontal, Direction.Input, false);
            _packageStartPort.view.AddToClassList("microGroup_packagestartport");
            _packageEndPort.view.AddToClassList("microGroup_packageendport");
            _packageStartPort.title = "开始";
            _packageEndPort.title = "结束";
            _packageStartPort.onCanLink += m_packageStartPort_onCanLink;
            _packageStartPort.onConnect += m_packageStartPort_onConnect;
            _packageStartPort.onDisconnect += m_packageStartPort_onDisconnect;

            _packageEndPort.onCanLink += m_packageEndPort_onCanLink;
            _packageEndPort.onConnect += m_packageEndPort_onConnect;
            _packageEndPort.onDisconnect += m_packageEndPort_onDisconnect;

            _packageContainer.Add(_packageStartPort);
            _packageContainer.Add(_packageEndPort);
        }
        private void m_refreshPackage()
        {
            _packageContainer.SetDisplay(true);
            _packageIcon.SetDisplay(true);
            _packageInfo = _owner.Target.GetPackage(editorInfo.GroupId);
            if (_packageInfo == null)
            {
                _packageInfo = new MicroPackageInfo();
                _packageInfo.PackageId = editorInfo.GroupId;
                _owner.Target.Packages.Add(_packageInfo);
            }
            for (int i = _packageInfo.EndNodes.Count - 1; i >= 0; i--)
            {
                var node = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(_packageInfo.EndNodes[i]);
                if (node != null)
                {
                    _packageEndPort.ConnectWithoutNotify(node.nodeView.OutPut);
                }
                else
                {
                    _packageInfo.EndNodes.RemoveAt(i);
                }
            }
            for (int i = _packageInfo.StartNodes.Count - 1; i >= 0; i--)
            {
                var node = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(_packageInfo.StartNodes[i]);
                if (node != null)
                {
                    _packageStartPort.ConnectWithoutNotify(node.nodeView.Input);
                }
                else
                {
                    _packageInfo.StartNodes.RemoveAt(i);
                }
            }
        }

        private bool m_packageStartPort_onCanLink(MicroPort mine, MicroPort target)
        {
            if (target.portType != MicroPortType.BaseNodePort)
                return false;
            var graphElement = target.view.node;
            if (graphElement == null)
                return false;
            return graphElement.GetContainingScope() == this;
        }

        private void m_packageStartPort_onConnect(MicroPort mine, MicroPort target)
        {
            if (_packageInfo == null)
                return;
            if (target.view.node is BaseMicroNodeView.InternalNodeView nodeView)
            {
                if (_packageInfo.StartNodes.Contains(nodeView.nodeView.Target.OnlyId))
                    return;
                _packageInfo.StartNodes.Add(nodeView.nodeView.Target.OnlyId);
                if (editorInfo.IsPackage)
                    _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
            }
        }

        private void m_packageStartPort_onDisconnect(MicroPort mine, MicroPort target)
        {
            if (_packageInfo == null)
                return;
            if (target.view.node is BaseMicroNodeView.InternalNodeView nodeView)
            {
                _packageInfo.StartNodes.Remove(nodeView.nodeView.Target.OnlyId);
                if (editorInfo.IsPackage)
                    _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
            }
        }

        private bool m_packageEndPort_onCanLink(MicroPort mine, MicroPort target)
        {
            if (target.portType != MicroPortType.BaseNodePort)
                return false;
            var graphElement = target.view.node;
            if (graphElement == null)
                return false;
            return graphElement.GetContainingScope() == this;
        }

        private void m_packageEndPort_onConnect(MicroPort mine, MicroPort target)
        {
            if (_packageInfo == null)
                return;
            if (target.view.node is BaseMicroNodeView.InternalNodeView nodeView)
            {
                if (_packageInfo.EndNodes.Contains(nodeView.nodeView.Target.OnlyId))
                    return;
                _packageInfo.EndNodes.Add(nodeView.nodeView.Target.OnlyId);
                if (editorInfo.IsPackage)
                    _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
            }
        }

        private void m_packageEndPort_onDisconnect(MicroPort mine, MicroPort target)
        {
            if (_packageInfo == null)
                return;
            if (target.view.node is BaseMicroNodeView.InternalNodeView nodeView)
            {
                _packageInfo.EndNodes.Remove(nodeView.nodeView.Target.OnlyId);
                if (editorInfo.IsPackage)
                    _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
            }
        }

        /// <summary>
        /// 移除包
        /// </summary>
        /// <param name="action"></param>
        private void m_delPackage(DropdownMenuAction action)
        {
            if (!editorInfo.IsPackage)
            {
                _owner.owner.ShowNotification(new GUIContent("当前分组不是包分组，无法转换"), MicroGraphUtils.NOTIFICATION_TIME);
                return;
            }
            var packagrNodeList = _owner.Target.Nodes.OfType<MicroPackageNode>().ToList();
            bool isHave = false;
            foreach (var item in packagrNodeList)
            {
                if (item.PackageId == editorInfo.GroupId)
                {
                    isHave = true;
                    break;
                }
            }
            if (isHave)
            {
                _owner.owner.ShowNotification(new GUIContent("当前包正在被使用，无法转换"), MicroGraphUtils.NOTIFICATION_TIME);
                return;
            }
            _packageContainer.SetDisplay(false);
            _packageIcon.SetDisplay(false);
            editorInfo.IsPackage = false;
            if (_packageInfo == null)
                return;
            _owner.Target.Packages.Remove(_packageInfo);

            for (int i = _packageInfo.EndNodes.Count - 1; i >= 0; i--)
            {
                var node = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(_packageInfo.EndNodes[i]);
                if (node != null)
                    _owner.View.DeleteElements(_packageEndPort.view.connections);
            }
            for (int i = _packageInfo.StartNodes.Count - 1; i >= 0; i--)
            {
                var node = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(_packageInfo.StartNodes[i]);
                if (node != null)
                    _owner.View.DeleteElements(_packageStartPort.view.connections);
            }
            _packageInfo = null;
            _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_DELETE, _packageInfo);
        }
        /// <summary>
        /// 添加包
        /// </summary>
        /// <param name="action"></param>
        private void m_addPackage(DropdownMenuAction action)
        {
            if (editorInfo.IsPackage)
            {
                _owner.owner.ShowNotification(new GUIContent("当前分组已是包分组，无法转换"), MicroGraphUtils.NOTIFICATION_TIME);
                return;
            }
            foreach (var item in _owner.editorInfo.Groups.Where(a => a.Title == editorInfo.Title))
            {
                if (item.IsPackage)
                {
                    _owner.owner.ShowNotification(new GUIContent("当前分组名称已被其他分组使用，无法转换"), MicroGraphUtils.NOTIFICATION_TIME);
                    return;
                }
            }
            List<Edge> edges = new List<Edge>();
            foreach (var item in editorInfo.Nodes)
            {
                var element = _owner.GetElement<GraphElement>(item);
                if (element is BaseMicroNodeView.InternalNodeView nodeView)
                {
                    if (nodeView.nodeView.Input != null)
                    {
                        foreach (var edge in nodeView.nodeView.Input.view.connections)
                        {
                            if (edge.output.node?.GetContainingScope() != this)
                            {
                                edges.Add(edge);
                            }
                        }
                    }
                    if (nodeView.nodeView.OutPut != null)
                    {
                        foreach (var edge in nodeView.nodeView.OutPut.view.connections)
                        {
                            if (edge.input.node?.GetContainingScope() != this)
                            {
                                edges.Add(edge);
                            }
                        }
                    }
                }
            }
            _owner.View.DeleteElements(edges);
            _packageIcon.SetDisplay(true);
            _packageContainer.SetDisplay(true);
            editorInfo.IsPackage = true;
            _packageInfo = new MicroPackageInfo();
            _packageInfo.PackageId = editorInfo.GroupId;
            _owner.Target.Packages.Add(_packageInfo);
            _owner.listener.OnEvent(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, _packageInfo);
        }
        /// <summary>
        /// 前往引用节点
        /// </summary>
        /// <param name="action"></param>
        private void m_frameNode(DropdownMenuAction action)
        {
            if (action.userData is MicroPackageNode packageNode)
            {
                var nodeView = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(packageNode.OnlyId);
                _owner.View.ClearSelection();
                _owner.View.AddToSelection(nodeView);
                _owner.View.FrameSelection();
            }
        }
    }
}
