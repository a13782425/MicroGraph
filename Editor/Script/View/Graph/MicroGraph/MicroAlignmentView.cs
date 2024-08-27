using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 对其视图
    /// </summary>
    internal sealed class MicroAlignmentView : GraphElement
    {
        private enum MicroAlignmentEnum
        {
            Top,
            Left,
            Right,
            Bottom,
            Horizontal,
            Vertical,
            Grid_Horizontal = 10,
            Grid_Vertical,
        }

        private const string STYLE_PATH = "Uss/MicroGraph/MicroAlignmentView";
        private BaseMicroGraphView _owner;
        private Image _headImage;
        private List<Button> _buttons = new List<Button>();
        private SeparatorElement _separator;

        public MicroAlignmentView(BaseMicroGraphView graph)
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_alignment");
            this.AddToClassList("horizontal");
            this.capabilities |= Capabilities.Movable;
            this._owner = graph;
            this.AddManipulator(new Dragger() { clampToParentEdges = true });
            this.AddManipulator(new ContextualMenuManipulator(onBuildContextualMenu));
            m_createElement();
            graph.onSelectChanged += m_onSelectChanged;
            UpdateAlign();
        }

        private void m_onSelectChanged(List<ISelectable> list)
        {
            UpdateAlign();
        }

        private void onBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (this.ClassListContains("vertical"))
            {
                evt.menu.AppendAction("横向显示", (a) =>
                {
                    _separator.direction = SeparatorDirection.Vertical;
                    this.RemoveFromClassList("vertical");
                    this.AddToClassList("horizontal");
                }, DropdownMenuAction.AlwaysEnabled);
            }
            else
            {
                evt.menu.AppendAction("纵向显示", (a) =>
                {
                    _separator.direction = SeparatorDirection.Horizontal;
                    this.RemoveFromClassList("horizontal");
                    this.AddToClassList("vertical");
                }, DropdownMenuAction.AlwaysEnabled);
            }

            evt.StopPropagation();
        }

        public void UpdateAlign()
        {
            int count = _owner.View.selection.OfType<Node>().Count();
            foreach (var item in _buttons)
            {
                if ((MicroAlignmentEnum)item.userData >= MicroAlignmentEnum.Grid_Horizontal)
                {
                    item.SetEnabled(count > 2);
                }
                else
                {
                    item.SetEnabled(count > 1);
                }
            }
        }

        private void m_onGridVertical()
        {
            List<Node> nodes = _owner.View.selection.OfType<Node>().ToList();
            nodes.Sort((a, b) => { return a.GetPosition().position.y.CompareTo(b.GetPosition().position.y); });
            float top = GetTop(nodes);
            float bottom = GetBottom(nodes);
            float elementHeight = 0;
            int count = nodes.Count;
            IMicroGraphRecordCommand record;
            for (int i = 0; i < count - 1; i++)
            {
                elementHeight += nodes[i].layout.height;
            }
            float intervalY = (bottom - top - elementHeight) / (count - 1);
            float lastHeight = 0;
            for (int i = 0; i < count; i++)
            {
                var node = nodes[i];
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                top = top + lastHeight;
                pos.y = top + intervalY * i;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
                lastHeight = node.layout.height;
            }
        }

        private void m_onGridHorizontal()
        {
            List<Node> nodes = _owner.View.selection.OfType<Node>().ToList();
            nodes.Sort((a, b) => { return a.GetPosition().position.x.CompareTo(b.GetPosition().position.x); });
            float left = GetLeft(nodes);
            float right = GetRight(nodes);
            float elementWidth = 0;
            int count = nodes.Count;
            IMicroGraphRecordCommand record;
            for (int i = 0; i < count - 1; i++)
            {
                elementWidth += nodes[i].layout.width;
            }
            float intervalX = (right - left - elementWidth) / (count - 1);
            float lastWidth = 0;
            for (int i = 0; i < count; i++)
            {
                var node = nodes[i];
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                left = left + lastWidth;
                pos.x = left + intervalX * i;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
                lastWidth = node.layout.width;
            }
        }

        private void m_onVerticalAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float left = GetLeft(nodes);
            float right = GetRight(nodes);
            float mid = (left + right) / 2;
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.x = mid;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }

        private void m_onHorizontalAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float top = GetTop(nodes);
            float bottom = GetBottom(nodes);
            float mid = (top + bottom) / 2;
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.y = mid;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }
        private void m_onBottomAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float bottomY = GetBottom(nodes);
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.y = bottomY;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }

        private void m_onRightAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float rightX = GetRight(nodes);
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.x = rightX;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }

        private void m_onLeftAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float leftX = GetLeft(nodes);
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.x = leftX;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }

        private void m_onTopAlign()
        {
            IEnumerable<Node> nodes = _owner.View.selection.OfType<Node>();
            float topY = GetTop(nodes);
            IMicroGraphRecordCommand record;
            foreach (var node in nodes)
            {
                Rect rec = node.GetPosition();
                Vector2 pos = rec.position;
                Vector2 size = rec.size;
                pos.y = topY;
                node.SetPosition(new Rect(pos, size));
                switch (node)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        record = new MicroNodeMoveRecord();
                        record.Record(_owner, nodeView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                    case MicroVariableNodeView.InternalNodeView variableView:
                        record = new MicroVarNodeMoveRecord();
                        record.Record(_owner, variableView.nodeView);
                        _owner.Undo?.AddCommand(record);
                        break;
                }
            }
        }

        private float GetTop(IEnumerable<Node> nodes)
        {
            float max = float.MaxValue;
            foreach (var item in nodes)
            {
                Vector2 pos = item.GetPosition().position;
                if (pos.y < max)
                {
                    max = pos.y;
                }
            }
            return max;
        }

        private float GetBottom(IEnumerable<Node> nodes)
        {
            float max = float.MinValue;
            foreach (var item in nodes)
            {
                Vector2 pos = item.GetPosition().position;
                if (pos.y > max)
                {
                    max = pos.y;
                }
            }
            return max;
        }
        private float GetLeft(IEnumerable<Node> nodes)
        {
            float max = float.MaxValue;
            foreach (var item in nodes)
            {
                Vector2 pos = item.GetPosition().position;
                if (pos.x < max)
                {
                    max = pos.x;
                }
            }
            return max;
        }
        private float GetRight(IEnumerable<Node> nodes)
        {
            float max = float.MinValue;
            foreach (var item in nodes)
            {
                Vector2 pos = item.GetPosition().position;
                if (pos.x > max)
                {
                    max = pos.x;
                }
            }
            return max;
        }
        private void m_createElement()
        {
            _headImage = new Image();
            this._headImage.AddToClassList("micro_alignment_headicon");
            this.Add(_headImage);

            m_addButton(m_onTopAlign, MicroAlignmentEnum.Top).AddToClassList("first");
            m_addButton(m_onLeftAlign, MicroAlignmentEnum.Left);
            m_addButton(m_onRightAlign, MicroAlignmentEnum.Right);
            m_addButton(m_onBottomAlign, MicroAlignmentEnum.Bottom);
            m_addButton(m_onHorizontalAlign, MicroAlignmentEnum.Horizontal);
            m_addButton(m_onVerticalAlign, MicroAlignmentEnum.Vertical);

            _separator = new SeparatorElement();
            _separator.direction = SeparatorDirection.Vertical;
            this.Add(_separator);

            m_addButton(m_onGridHorizontal, MicroAlignmentEnum.Grid_Horizontal);
            m_addButton(m_onGridVertical, MicroAlignmentEnum.Grid_Vertical).AddToClassList("last");
        }
        private Button m_addButton(Action action, MicroAlignmentEnum type)
        {
            Button btn = new Button(action);
            btn.AddToClassList("micro_alignment_button");
            btn.tooltip = GetTooltipByAlignType(type);
            btn.userData = type;
            Image btnImage = new Image();
            btnImage.style.backgroundImage = MicroGraphUtils.LoadRes<Sprite>("Texture/Common/align_" + type.ToString()).texture;
            btn.Add(btnImage);
            _buttons.Add(btn);
            this.Add(btn);
            return btn;
        }

        private string GetTooltipByAlignType(MicroAlignmentEnum type) => type switch
        {
            MicroAlignmentEnum.Bottom => "下对齐",
            MicroAlignmentEnum.Top => "上对齐",
            MicroAlignmentEnum.Left => "左对齐",
            MicroAlignmentEnum.Right => "右对齐",
            MicroAlignmentEnum.Vertical => "垂直居中对齐",
            MicroAlignmentEnum.Horizontal => "水平居中对齐",
            MicroAlignmentEnum.Grid_Vertical => "纵向分布",
            MicroAlignmentEnum.Grid_Horizontal => "横向分布",
            _ => ""
        };
    }
}
