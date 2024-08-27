using MicroGraph.Runtime;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 节点模板实现
    /// </summary>
    internal class MicroNodeTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            BaseMicroNodeView.InternalNodeView nodeView = target as BaseMicroNodeView.InternalNodeView;
            MicroNodeSerializeModel nodeModel = new MicroNodeSerializeModel();
            nodeModel.nodeId = nodeView.nodeView.Target.OnlyId;
            nodeModel.className = nodeView.nodeView.Target.GetType().FullName;
            nodeModel.pos = nodeView.GetPosition().position;
            model.nodes.Add(nodeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroNodeSerializeModel model = data as MicroNodeSerializeModel;
            var nodeCategory = operateData.view.CategoryModel.GetNodeCategory(model.className);
            bool isUnique = operateData.view.CategoryModel.IsUniqueNode(model.className);
            if (isUnique)
            {
                var uniqueNode = operateData.view.Target.Nodes.FirstOrDefault(a => a.GetType() == nodeCategory.NodeClassType);
                if (uniqueNode != null)
                {
                    operateData.view.owner.ShowNotification(new GUIContent("唯一节点不允许重复创建"), 2f);
                    operateData.oldMappingNewIdDic[model.nodeId] = uniqueNode.OnlyId;
                    return;
                }
            }
            if (!nodeCategory.IsEnable)
            {
                operateData.view.owner.ShowNotification(new GUIContent($"节点:{nodeCategory.NodeName} 已不再使用"), 2f);
                return;
            }
            Vector2 offset = operateData.centerPos - model.pos;
            var node = operateData.view.AddNode(nodeCategory.NodeClassType, operateData.mousePos - offset);
            operateData.oldMappingNewIdDic[model.nodeId] = node.OnlyId;
            return;
        }
    }
    /// <summary>
    /// 变量模板实现
    /// target 是 MicroVariableEditorInfo
    /// </summary>
    internal class MicroVarTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            MicroVariableEditorInfo editorInfo = target as MicroVariableEditorInfo;
            MicroVarSerializeModel varModel = new MicroVarSerializeModel();
            varModel.varClassName = editorInfo.Target.GetValueType().FullName;
            varModel.varName = editorInfo.Target.Name;
            varModel.canRename = editorInfo.CanRename;
            varModel.canDelete = editorInfo.CanDelete;
            varModel.canDefaultValue = editorInfo.CanDefaultValue;
            varModel.canAssign = editorInfo.CanAssign;
            model.vars.Add(varModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroVarSerializeModel model = data as MicroVarSerializeModel;
            var varCategory = operateData.view.CategoryModel.VariableCategories.FirstOrDefault(a => a.VarType.FullName == model.varClassName);
            if (varCategory == null)
                return;
            operateData.view.AddVariable(model.varName, varCategory.VarType, model.canDelete, model.canRename, model.canDefaultValue, model.canAssign);
        }
    }
    /// <summary>
    /// 变量节点模板实现
    /// </summary>
    internal class MicroVarNodeTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            MicroVariableNodeView.InternalNodeView varNodeView = target as MicroVariableNodeView.InternalNodeView;
            MicroVarNodeSerializeModel varNodeModel = new MicroVarNodeSerializeModel();
            varNodeModel.nodeId = varNodeView.nodeView.editorInfo.NodeId;
            varNodeModel.varName = varNodeView.nodeView.Target.Name;
            varNodeModel.pos = varNodeView.nodeView.LastPos;
            model.varNodes.Add(varNodeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroVarNodeSerializeModel model = data as MicroVarNodeSerializeModel;
            BaseMicroVariable variable = operateData.view.Target.Variables.FirstOrDefault(a => a.Name == model.varName);
            if (variable == null)
                return;
            Vector2 offset = operateData.centerPos - model.pos;
            MicroVariableNodeView varNodeView = operateData.view.AddVariableNodeView(variable, operateData.mousePos - offset);
            operateData.oldMappingNewIdDic[model.nodeId] = varNodeView.editorInfo.NodeId;
        }
    }
    /// <summary>
    /// 便笺模板
    /// </summary>
    internal class MicroStickyNodeTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            MicroStickyNoteView node = (MicroStickyNoteView)target;
            MicroStickySerializeModel stickyModel = new MicroStickySerializeModel();
            stickyModel.theme = node.editorInfo.Theme;
            stickyModel.pos = node.GetPosition().position;
            stickyModel.size = node.GetPosition().size;
            stickyModel.fontSize = node.editorInfo.FontSize;
            stickyModel.fontStyle = node.editorInfo.FontStyle;
            stickyModel.content = node.editorInfo.Content;
            model.stickys.Add(stickyModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroStickySerializeModel model = data as MicroStickySerializeModel;
            Vector2 offset = operateData.centerPos - model.pos;
            MicroStickyEditorInfo sticky = new MicroStickyEditorInfo();
            sticky.NodeId = operateData.view.editorInfo.GetUniqueId();
            sticky.Pos = operateData.mousePos - offset;
            sticky.Theme = model.theme;
            sticky.FontSize = model.fontSize;
            sticky.FontStyle = model.fontStyle;
            sticky.Size = model.size;
            sticky.Content = model.content;
            operateData.view.AddStickyNodeView(sticky);
        }
    }
    /// <summary>
    /// 连线模板实现
    /// </summary>
    internal class MicroEdgeTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            MicroEdgeView edgeView = target as MicroEdgeView;
            MicroEdgeSerializeModel edgeModel = new MicroEdgeSerializeModel();
            MicroPort.InternalPort inPort = edgeView.input as MicroPort.InternalPort;
            MicroPort.InternalPort outPort = edgeView.output as MicroPort.InternalPort;
            edgeModel.inKey = inPort.microPort.key;
            edgeModel.outKey = outPort.microPort.key;
            if (edgeView.input.node is BaseMicroNodeView.InternalNodeView inNodeView)
                edgeModel.inNodeId = inNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.input.node is MicroVariableNodeView.InternalNodeView inVarView)
                edgeModel.inNodeId = inVarView.nodeView.editorInfo.NodeId;

            if (edgeView.output.node is BaseMicroNodeView.InternalNodeView outNodeView)
                edgeModel.outNodeId = outNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.output.node is MicroVariableNodeView.InternalNodeView outVarView)
                edgeModel.outNodeId = outVarView.nodeView.editorInfo.NodeId;
            model.edges.Add(edgeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroEdgeSerializeModel model = data as MicroEdgeSerializeModel;
            if (!operateData.oldMappingNewIdDic.TryGetValue(model.inNodeId, out int newInId))
                return;
            var inNodeView = operateData.view.GetElement<Node>(newInId);
            if (inNodeView == null)
                return;
            if (!operateData.oldMappingNewIdDic.TryGetValue(model.outNodeId, out int newOutId))
                return;
            var outNodeView = operateData.view.GetElement<Node>(newOutId);
            if (outNodeView == null)
                return;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(model.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inVarView)
                inPort = inVarView.nodeView.Input;
            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(model.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView inVarView)
                outPort = inVarView.nodeView.OutPut;

            if (inPort == outPort || inPort == null || outPort == null)
                return;

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
                return;

            IMicroGraphRecordCommand record = new MicroEdgeAddRecord();
            record.Record(operateData.view, edgeView);
            operateData.view.Undo.AddCommand(record);
            return;
        }
    }
    /// <summary>
    /// 分组模板实现
    /// </summary>
    internal class MicroGroupTemplateImpl : IMicroGraphTemplate
    {
        public void Record(MicroGraphTemplateModel model, object target)
        {
            MicroGroupView groupView = target as MicroGroupView;
            MicroGroupSerializeModel groupModel = new MicroGroupSerializeModel();
            groupModel.title = groupView.editorInfo.Title;
            groupModel.nodeIds.AddRange(groupView.editorInfo.Nodes);
            groupModel.color = groupView.editorInfo.GroupColor;
            groupModel.pos = groupView.editorInfo.Pos;
            model.groups.Add(groupModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroGroupSerializeModel model = data as MicroGroupSerializeModel;
            Vector2 offset = operateData.centerPos - model.pos;
            MicroGroupEditorInfo group = new MicroGroupEditorInfo();
            group.GroupId = operateData.view.editorInfo.GetUniqueId();
            group.Pos = operateData.mousePos - offset;
            group.Title = model.title;
            group.GroupColor = model.color;
            MicroGroupView groupView = operateData.view.AddGroupView(group);
            foreach (int id in model.nodeIds)
            {
                if (!operateData.oldMappingNewIdDic.TryGetValue(id, out int newId))
                    continue;
                var nodeView = operateData.view.GetElement<Node>(newId);
                if (nodeView == null)
                    continue;
                groupView.AddElement(nodeView);
            }
        }
    }
}
