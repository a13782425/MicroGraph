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
            nodeModel.NodeId = nodeView.nodeView.Target.OnlyId;
            nodeModel.ClassName = nodeView.nodeView.Target.GetType().FullName;
            nodeModel.Pos = nodeView.GetPosition().position;
            model.Nodes.Add(nodeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroNodeSerializeModel model = data as MicroNodeSerializeModel;
            var nodeCategory = operateData.view.CategoryModel.GetNodeCategory(model.ClassName);
            bool isUnique = operateData.view.CategoryModel.IsUniqueNode(model.ClassName);
            if (isUnique)
            {
                var uniqueNode = operateData.view.Target.Nodes.FirstOrDefault(a => a.GetType() == nodeCategory.NodeClassType);
                if (uniqueNode != null)
                {
                    operateData.view.owner.ShowNotification(new GUIContent("唯一节点不允许重复创建"), 2f);
                    operateData.oldMappingNewIdDic[model.NodeId] = uniqueNode.OnlyId;
                    return;
                }
            }
            if (nodeCategory.EnableState == MicroNodeEnableState.Disabled)
            {
                operateData.view.owner.ShowNotification(new GUIContent($"节点:{nodeCategory.NodeName} 已不再使用"), 2f);
                return;
            }
            Vector2 offset = operateData.centerPos - model.Pos;
            var node = operateData.view.AddNode(nodeCategory.NodeClassType, operateData.mousePos - offset);
            operateData.oldMappingNewIdDic[model.NodeId] = node.OnlyId;
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
            varModel.VarClassName = editorInfo.Target.GetValueType().FullName;
            varModel.VarName = editorInfo.Target.Name;
            varModel.CanRename = editorInfo.CanRename;
            varModel.CanDelete = editorInfo.CanDelete;
            varModel.CanDefaultValue = editorInfo.CanDefaultValue;
            varModel.CanAssign = editorInfo.CanAssign;
            model.Vars.Add(varModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroVarSerializeModel model = data as MicroVarSerializeModel;
            var varCategory = operateData.view.CategoryModel.VariableCategories.FirstOrDefault(a => a.VarType.FullName == model.VarClassName);
            if (varCategory == null)
                return;
            operateData.view.AddVariable(model.VarName, varCategory.VarType, model.CanDelete, model.CanRename, model.CanDefaultValue, model.CanAssign);
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
            varNodeModel.NodeId = varNodeView.nodeView.editorInfo.NodeId;
            varNodeModel.VarName = varNodeView.nodeView.Target.Name;
            varNodeModel.Pos = varNodeView.nodeView.LastPos;
            model.VarNodes.Add(varNodeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroVarNodeSerializeModel model = data as MicroVarNodeSerializeModel;
            BaseMicroVariable variable = operateData.view.Target.Variables.FirstOrDefault(a => a.Name == model.VarName);
            if (variable == null)
                return;
            Vector2 offset = operateData.centerPos - model.Pos;
            MicroVariableNodeView varNodeView = operateData.view.AddVariableNodeView(variable, operateData.mousePos - offset);
            operateData.oldMappingNewIdDic[model.NodeId] = varNodeView.editorInfo.NodeId;
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
            stickyModel.Theme = node.editorInfo.Theme;
            stickyModel.Pos = node.GetPosition().position;
            stickyModel.Size = node.GetPosition().size;
            stickyModel.FontSize = node.editorInfo.FontSize;
            stickyModel.FontStyle = node.editorInfo.FontStyle;
            stickyModel.Content = node.editorInfo.Content;
            model.Stickys.Add(stickyModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroStickySerializeModel model = data as MicroStickySerializeModel;
            Vector2 offset = operateData.centerPos - model.Pos;
            MicroStickyEditorInfo sticky = new MicroStickyEditorInfo();
            sticky.NodeId = operateData.view.editorInfo.GetNodeUniqueId();
            sticky.Pos = operateData.mousePos - offset;
            sticky.Theme = model.Theme;
            sticky.FontSize = model.FontSize;
            sticky.FontStyle = model.FontStyle;
            sticky.Size = model.Size;
            sticky.Content = model.Content;
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
            edgeModel.InKey = inPort.microPort.key;
            edgeModel.OutKey = outPort.microPort.key;
            if (edgeView.input.node is BaseMicroNodeView.InternalNodeView inNodeView)
                edgeModel.InNodeId = inNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.input.node is MicroVariableNodeView.InternalNodeView inVarView)
                edgeModel.InNodeId = inVarView.nodeView.editorInfo.NodeId;

            if (edgeView.output.node is BaseMicroNodeView.InternalNodeView outNodeView)
                edgeModel.OutNodeId = outNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.output.node is MicroVariableNodeView.InternalNodeView outVarView)
                edgeModel.OutNodeId = outVarView.nodeView.editorInfo.NodeId;
            model.Edges.Add(edgeModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroEdgeSerializeModel model = data as MicroEdgeSerializeModel;
            if (!operateData.oldMappingNewIdDic.TryGetValue(model.InNodeId, out int newInId))
                return;
            var inNodeView = operateData.view.GetElement<Node>(newInId);
            if (inNodeView == null)
                return;
            if (!operateData.oldMappingNewIdDic.TryGetValue(model.OutNodeId, out int newOutId))
                return;
            var outNodeView = operateData.view.GetElement<Node>(newOutId);
            if (outNodeView == null)
                return;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(model.InKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inVarView)
                inPort = inVarView.nodeView.Input;
            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(model.OutKey, false);
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
            groupModel.Title = groupView.editorInfo.Title;
            groupModel.NodeIds.AddRange(groupView.editorInfo.Nodes);
            groupModel.Color = groupView.editorInfo.GroupColor;
            groupModel.Pos = groupView.editorInfo.Pos;
            model.Groups.Add(groupModel);
        }

        public void Restore(MicroTemplateOperateData operateData, object data)
        {
            MicroGroupSerializeModel model = data as MicroGroupSerializeModel;
            Vector2 offset = operateData.centerPos - model.Pos;
            MicroGroupEditorInfo group = new MicroGroupEditorInfo();
            group.GroupId = operateData.view.editorInfo.GetNodeUniqueId();
            group.Pos = operateData.mousePos - offset;
            group.Title = model.Title;
            group.GroupColor = model.Color;
            MicroGroupView groupView = operateData.view.AddGroupView(group);
            foreach (int id in model.NodeIds)
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
