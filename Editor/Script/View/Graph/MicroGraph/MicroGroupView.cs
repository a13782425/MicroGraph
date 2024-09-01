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
    internal sealed class MicroGroupView : Group
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroGroupView";
        private BaseMicroGraphView _owner = null;
        private MicroGroupEditorInfo _editorInfo = null;
        internal MicroGroupEditorInfo editorInfo => _editorInfo;
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
        }

        private void m_contextMenu(ContextualMenuPopulateEvent evt)
        {
            if (_owner.View.selection.OfType<Node>().Count() > 1)
                evt.menu.AppendAction("添加模板", (e) => _owner.AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
            evt.StopPropagation();
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
            _editorInfo.Title = newName;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            IMicroGraphRecordCommand command = null;
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
                        }
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        if (!_editorInfo.Nodes.Contains(variableView.nodeView.editorInfo.NodeId))
                        {
                            _editorInfo.Nodes.Add(variableView.nodeView.editorInfo.NodeId);
                            command = new MicroGroupAttachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, variableView.nodeView.editorInfo.NodeId));
                            _owner.Undo.AddCommand(command);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            IMicroGraphRecordCommand command = null;
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
                        }
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        if (_editorInfo.Nodes.Contains(variableView.nodeView.editorInfo.NodeId))
                        {
                            _editorInfo.Nodes.Remove(variableView.nodeView.editorInfo.NodeId);
                            command = new MicroGroupUnattachRecord();
                            command.Record(_owner, new MicroGroupAttachRecordData(_editorInfo.GroupId, variableView.nodeView.editorInfo.NodeId));
                            _owner.Undo.AddCommand(command);
                        }
                        break;
                    default:
                        break;
                }
            }
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
}
