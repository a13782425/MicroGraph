using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(BaseMicroNodeView))]
    internal sealed class MicroNodeCopyPaste : IMicroGraphCopyPaste
    {
        private Vector2 copyPos;
        private Type nodeType;
        private int oldId;

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            BaseMicroNodeView nodeView = (BaseMicroNodeView)target;
            this.copyPos = nodeView.view.GetPosition().position;
            this.nodeType = nodeView.Target.GetType();
            this.oldId = nodeView.Target.OnlyId;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            bool isUnique = copyOperateData.view.CategoryModel.IsUniqueNode(nodeType);
            if (isUnique)
            {
                if (copyOperateData.view.Target.Nodes.FirstOrDefault(a => a.GetType() == nodeType) != null)
                {
                    copyOperateData.view.owner.ShowNotification(new GUIContent("唯一节点不允许重复创建"), 2f);
                    return false;
                }
            }

            Vector2 offset = copyOperateData.centerPos - this.copyPos;
            var node = copyOperateData.view.AddNode(this.nodeType, copyOperateData.mousePos - offset);
            copyOperateData.oldMappingNewIdDic[this.oldId] = node.OnlyId;
            return true;
        }
    }

    [MicroGraphEditor(typeof(MicroVariableNodeView))]
    internal sealed class MicroVariableNodeCopyPaste : IMicroGraphCopyPaste
    {
        private Vector2 copyPos;
        private string varName;
        private int oldId;

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            MicroVariableNodeView node = (MicroVariableNodeView)target;
            this.copyPos = node.view.GetPosition().position;
            this.oldId = node.editorInfo.NodeId;
            this.varName = node.editorInfo.Name;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            BaseMicroVariable variable = copyOperateData.view.Target.Variables.FirstOrDefault(a => a.Name == this.varName);
            if (variable == null)
                return false;
            Vector2 offset = copyOperateData.centerPos - this.copyPos;
            MicroVariableNodeView varNodeView = copyOperateData.view.AddVariableNodeView(variable, copyOperateData.mousePos - offset);
            copyOperateData.oldMappingNewIdDic[this.oldId] = varNodeView.editorInfo.NodeId;
            return true;
        }
    }

    [MicroGraphEditor(typeof(MicroVariableItemView))]
    internal sealed class MicroVariableItemCopyPaste : IMicroGraphCopyPaste
    {
        private string varName;
        private Type varType;

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            MicroVariableItemView node = (MicroVariableItemView)target;
            this.varType = node.editorInfo.Target.GetType();
            this.varName = node.editorInfo.Name;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            string uniqueName = this.varName;
            string varName = uniqueName;
            int i = 0;
            while (copyOperateData.view.Target.Variables.Any(e => e.Name == varName))
                varName = uniqueName + (i++);
            copyOperateData.view.AddVariable(varName, this.varType);
            return true;
        }
    }

    [MicroGraphEditor(typeof(MicroGroupView))]
    internal sealed class MicroGroupCopyPaste : IMicroGraphCopyPaste
    {
        private Vector2 pos = default;
        private Color color = default;
        private string title = default;
        private List<int> nodeIds = new List<int>();

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            MicroGroupView node = (MicroGroupView)target;
            this.title = node.editorInfo.Title;
            this.nodeIds.AddRange(node.editorInfo.Nodes);
            this.color = node.editorInfo.GroupColor;
            this.pos = node.editorInfo.Pos;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            Vector2 offset = copyOperateData.centerPos - this.pos;
            MicroGroupEditorInfo group = new MicroGroupEditorInfo();
            group.GroupId = copyOperateData.view.editorInfo.GetUniqueId();
            group.Pos = copyOperateData.mousePos - offset;
            group.Title = this.title;
            group.GroupColor = color;
            MicroGroupView groupView = copyOperateData.view.AddGroupView(group);
            foreach (int id in this.nodeIds)
            {
                if (!copyOperateData.oldMappingNewIdDic.TryGetValue(id, out int newId))
                    continue;
                var nodeView = copyOperateData.view.GetElement<Node>(newId);
                if (nodeView == null)
                    continue;
                groupView.AddElement(nodeView);
            }
            return true;
        }
    }
    [MicroGraphEditor(typeof(MicroStickyNoteView))]
    internal sealed class MicroStickyNoteCopyPaste : IMicroGraphCopyPaste
    {
        private string content;
        private int theme;
        private int fontStyle;
        private int fontSize;
        private Vector2 pos = Vector2.zero;
        private Vector2 size = Vector2.zero;

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            MicroStickyNoteView node = (MicroStickyNoteView)target;
            content = node.editorInfo.Content;
            theme = node.editorInfo.Theme;
            fontStyle = node.editorInfo.FontStyle;
            fontSize = node.editorInfo.FontSize;
            pos = node.editorInfo.Pos;
            size = node.editorInfo.Size;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            Vector2 offset = copyOperateData.centerPos - this.pos;
            MicroStickyEditorInfo sticky = new MicroStickyEditorInfo();
            sticky.NodeId = copyOperateData.view.editorInfo.GetUniqueId();
            sticky.Pos = copyOperateData.mousePos - offset;
            sticky.Theme = this.theme;
            sticky.FontSize = this.fontSize;
            sticky.FontStyle = this.fontStyle;
            sticky.Size = this.size;
            sticky.Content = this.content;
            copyOperateData.view.AddStickyNodeView(sticky);
            return true;
        }
    }

    [MicroGraphEditor(typeof(MicroEdgeView))]
    internal sealed class MicroEdgeCopyPaste : IMicroGraphCopyPaste
    {
        private int inNodeId;
        private int outNodeId;
        private string inKey;
        private string outKey;

        public void Copy(BaseMicroGraphView graphView, object target)
        {
            MicroEdgeView node = (MicroEdgeView)target;
            MicroPort.InternalPort inPort = node.input as MicroPort.InternalPort;
            MicroPort.InternalPort outPort = node.output as MicroPort.InternalPort;
            this.inKey = inPort.microPort.key;
            this.outKey = outPort.microPort.key;

            if (node.input.node is BaseMicroNodeView.InternalNodeView inNodeView)
                this.inNodeId = inNodeView.nodeView.editorInfo.NodeId;
            else if (node.input.node is MicroVariableNodeView.InternalNodeView inVarView)
                this.inNodeId = inVarView.nodeView.editorInfo.NodeId;

            if (node.output.node is BaseMicroNodeView.InternalNodeView outNodeView)
                this.outNodeId = outNodeView.nodeView.editorInfo.NodeId;
            else if (node.output.node is MicroVariableNodeView.InternalNodeView outVarView)
                this.outNodeId = outVarView.nodeView.editorInfo.NodeId;
        }

        public bool Paste(MicroCopyPasteOperateData copyOperateData)
        {
            if (!copyOperateData.oldMappingNewIdDic.TryGetValue(this.inNodeId, out int newInId))
                return false;
            var inNodeView = copyOperateData.view.GetElement<Node>(newInId);
            if (inNodeView == null)
                return false;
            if (!copyOperateData.oldMappingNewIdDic.TryGetValue(this.outNodeId, out int newOutId))
                return false;
            var outNodeView = copyOperateData.view.GetElement<Node>(newOutId);
            if (outNodeView == null)
                return false;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(this.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inVarView)
                inPort = inVarView.nodeView.Input;
            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(this.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView inVarView)
                outPort = inVarView.nodeView.OutPut;

            if (inPort == outPort || inPort == null || outPort == null)
                return false;

            outPort.Connect(inPort);

            MicroEdgeView edgeView = null;
            foreach (var item in outPort.view.connections)
            {
                if (item.input == inPort.view)
                {
                    edgeView = (MicroEdgeView)item;
                    break;
                }
            }
            if (edgeView == null)
                return true;

            IMicroGraphRecordCommand record = new MicroEdgeAddRecord();
            record.Record(copyOperateData.view, edgeView);
            copyOperateData.view.Undo.AddCommand(record);
            return true;
        }
    }
}