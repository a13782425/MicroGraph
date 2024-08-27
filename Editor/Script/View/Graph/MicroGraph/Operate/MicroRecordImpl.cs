using MicroGraph.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// node移动记录
    /// </summary>
    internal class MicroNodeMoveRecord : IMicroGraphRecordCommand
    {
        private Vector3 _lastPostion;
        private Vector3 _curPostion;
        private int _nodeId;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            BaseMicroNodeView nodeView = (BaseMicroNodeView)target;
            _lastPostion = nodeView.LastPos;
            _curPostion = nodeView.view.GetPosition().position;
            _nodeId = nodeView.editorInfo.NodeId;
            nodeView.LastPos = _curPostion;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            BaseMicroNodeView.InternalNodeView node = graphView.GetElement<BaseMicroNodeView.InternalNodeView>(_nodeId);
            if (node != null)
            {
                node.SetPosition(new Rect(_curPostion, Vector2.one));
                node.nodeView.LastPos = _curPostion;
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            BaseMicroNodeView.InternalNodeView node = graphView.GetElement<BaseMicroNodeView.InternalNodeView>(_nodeId);
            if (node != null)
            {
                node.SetPosition(new Rect(_lastPostion, Vector2.one));
                node.nodeView.LastPos = _lastPostion;
            }
            return true;
        }
    }
    /// <summary>
    /// varNode移动记录
    /// </summary>
    internal class MicroVarNodeMoveRecord : IMicroGraphRecordCommand
    {
        private Vector3 _lastPostion;
        private Vector3 _curPostion;
        private int _nodeId;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;

        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroVariableNodeView nodeView = (MicroVariableNodeView)target;
            _lastPostion = nodeView.LastPos;
            _curPostion = nodeView.view.GetPosition().position;
            _nodeId = nodeView.editorInfo.NodeId;
            nodeView.LastPos = _curPostion;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            MicroVariableNodeView.InternalNodeView node = graphView.GetElement<MicroVariableNodeView.InternalNodeView>(_nodeId);
            if (node != null)
            {
                node.SetPosition(new Rect(_curPostion, Vector2.one));
                node.nodeView.LastPos = _curPostion;
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            MicroVariableNodeView.InternalNodeView node = graphView.GetElement<MicroVariableNodeView.InternalNodeView>(_nodeId);
            if (node != null)
            {
                node.SetPosition(new Rect(_lastPostion, Vector2.one));
                node.nodeView.LastPos = _lastPostion;
            }
            return true;
        }
    }

    /// <summary>
    /// Group移动记录
    /// </summary>
    internal class MicroGroupMoveRecord : IMicroGraphRecordCommand
    {
        private Vector3 _lastPostion;
        private Vector3 _curPostion;
        private MicroGroupView _groupView;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            _groupView = (MicroGroupView)target;
            _lastPostion = _groupView.LastPos;
            _curPostion = _groupView.GetPosition().position;
            _groupView.LastPos = _curPostion;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {

            if (_groupView != null)
            {
                _groupView.SetPosition(new Rect(_curPostion, Vector2.one));
                _groupView.LastPos = _curPostion;
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            if (_groupView != null)
            {
                _groupView.SetPosition(new Rect(_lastPostion, Vector2.one));
                _groupView.LastPos = _lastPostion;
            }
            return true;
        }
    }
    /// <summary>
    /// 节点删除撤销
    /// </summary>
    internal class MicroNodeDeleteRecord : IMicroGraphRecordCommand
    {
        private MicroNodeEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            BaseMicroNodeView nodeView = (BaseMicroNodeView)target;
            editorInfo = nodeView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.RemoveNode(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.AddNode(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 节点添加撤销
    /// </summary>
    internal class MicroNodeAddRecord : IMicroGraphRecordCommand
    {
        private MicroNodeEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            BaseMicroNodeView nodeView = (BaseMicroNodeView)target;
            editorInfo = nodeView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.AddNode(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.RemoveNode(editorInfo);
            return true;
        }
    }

    /// <summary>
    /// 变量节点删除撤销
    /// </summary>
    internal class MicroVarNodeDeleteRecord : IMicroGraphRecordCommand
    {
        private MicroVariableNodeEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroVariableNodeView nodeView = (MicroVariableNodeView)target;
            editorInfo = nodeView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.RemoveVariableNodeView(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.AddVariableNodeView(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 变量节点添加撤销
    /// </summary>
    internal class MicroVarNodeAddRecord : IMicroGraphRecordCommand
    {
        private MicroVariableNodeEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroVariableNodeView nodeView = (MicroVariableNodeView)target;
            editorInfo = nodeView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.AddVariableNodeView(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.RemoveVariableNodeView(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 变量删除撤销
    /// </summary>
    internal class MicroVarDeleteRecord : IMicroGraphRecordCommand
    {
        private MicroVariableEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.VAR_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroVariableItemView varItemView = (MicroVariableItemView)target;
            editorInfo = varItemView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.RemoveVariable(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.AddVariable(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 变量添加撤销
    /// </summary>
    internal class MicroVarAddRecord : IMicroGraphRecordCommand
    {
        private MicroVariableEditorInfo editorInfo;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.VAR_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroVariableItemView varItemView = (MicroVariableItemView)target;
            editorInfo = varItemView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.AddVariable(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.RemoveVariable(editorInfo);
            return true;
        }
    }

    /// <summary>
    /// 分组添加撤销
    /// </summary>
    internal class MicroGroupAddRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        private MicroGroupEditorInfo editorInfo;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroGroupView varItemView = (MicroGroupView)target;
            editorInfo = varItemView.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.AddGroupView(editorInfo);
            return false;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {

            graphView.RemoveGroupView(editorInfo);
            return true;
        }
    }

    internal struct MicroGroupAttachRecordData
    {
        public int groupId;
        public int nodeId;
        public MicroGroupAttachRecordData(int groupId, int nodeId)
        {
            this.groupId = groupId;
            this.nodeId = nodeId;
        }
    }
    /// <summary>
    /// 分组节点附加撤销
    /// </summary>
    internal class MicroGroupUnattachRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        private MicroGroupAttachRecordData attachData;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            attachData = (MicroGroupAttachRecordData)target;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            Group group = graphView.GetElement<Group>(attachData.groupId);
            Node node = graphView.GetElement<Node>(attachData.nodeId);
            if (group != null && node != null)
            {
                group.RemoveElement(node);
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            Group group = graphView.GetElement<Group>(attachData.groupId);
            Node node = graphView.GetElement<Node>(attachData.nodeId);
            if (group != null && node != null)
            {
                group.AddElement(node);
            }
            return true;
        }
    }
    /// <summary>
    /// 分组节点附加撤销
    /// </summary>
    internal class MicroGroupAttachRecord : IMicroGraphRecordCommand
    {

        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        private MicroGroupAttachRecordData attachData;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            attachData = (MicroGroupAttachRecordData)target;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            Group group = graphView.GetElement<Group>(attachData.groupId);
            Node node = graphView.GetElement<Node>(attachData.nodeId);
            if (group != null && node != null)
            {
                group.AddElement(node);
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            Group group = graphView.GetElement<Group>(attachData.groupId);
            Node node = graphView.GetElement<Node>(attachData.nodeId);
            if (group != null && node != null)
            {
                group.RemoveElement(node);
            }
            return true;
        }
    }

    /// <summary>
    /// 分组删除撤销
    /// </summary>
    internal class MicroGroupDeleteRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        private MicroGroupEditorInfo editorInfo;
        private List<int> nodeIds = new List<int>();
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroGroupView varItemView = (MicroGroupView)target;
            editorInfo = varItemView.editorInfo;
            nodeIds.AddRange(editorInfo.Nodes);
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.RemoveGroupView(editorInfo);
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            //editorInfo.Nodes.AddRange(nodeIds);
            graphView.AddGroupView(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 便笺删除撤销
    /// </summary>
    internal class MicroStickyAddRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        private MicroStickyEditorInfo editorInfo;
        private List<int> nodeIds = new List<int>();
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroStickyNoteView stickyNote = (MicroStickyNoteView)target;
            editorInfo = stickyNote.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.AddStickyNodeView(editorInfo);
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.RemoveStickyNodeView(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// 便笺删除撤销
    /// </summary>
    internal class MicroStickyDeleteRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.NODE_VIEW_RECORD_PRIORITY;
        private MicroStickyEditorInfo editorInfo;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroStickyNoteView stickyNote = (MicroStickyNoteView)target;
            editorInfo = stickyNote.editorInfo;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            graphView.RemoveStickyNodeView(editorInfo);
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            graphView.AddStickyNodeView(editorInfo);
            return true;
        }
    }
    /// <summary>
    /// Group移动记录
    /// </summary>
    internal class MicroStickyMoveRecord : IMicroGraphRecordCommand
    {
        private Vector3 _lastPostion;
        private Vector3 _curPostion;
        private MicroStickyNoteView _stickyView;
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.GROUP_VIEW_RECORD_PRIORITY;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            _stickyView = (MicroStickyNoteView)target;
            _lastPostion = _stickyView.LastPos;
            _curPostion = _stickyView.GetPosition().position;
            _stickyView.LastPos = _curPostion;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {

            if (_stickyView != null)
            {
                _stickyView.SetPosition(new Rect(_curPostion, _stickyView.editorInfo.Size));
                _stickyView.LastPos = _curPostion;
            }
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            if (_stickyView != null)
            {
                _stickyView.SetPosition(new Rect(_lastPostion, _stickyView.editorInfo.Size));
                _stickyView.LastPos = _lastPostion;
            }
            return true;
        }
    }
    /// <summary>
    /// 连线添加撤销
    /// </summary>
    internal class MicroEdgeAddRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.EDGE_VIEW_RECORD_PRIORITY;
        private int inNodeId;
        private int outNodeId;
        private string inKey;
        private string outKey;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroEdgeView edgeView = (MicroEdgeView)target;
            MicroPort.InternalPort inPort = edgeView.input as MicroPort.InternalPort;
            MicroPort.InternalPort outPort = edgeView.output as MicroPort.InternalPort;
            this.inKey = inPort.microPort.key;
            this.outKey = outPort.microPort.key;

            if (edgeView.input.node is BaseMicroNodeView.InternalNodeView inNodeView)
                this.inNodeId = inNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.input.node is MicroVariableNodeView.InternalNodeView inVarView)
                this.inNodeId = inVarView.nodeView.editorInfo.NodeId;

            if (edgeView.output.node is BaseMicroNodeView.InternalNodeView outNodeView)
                this.outNodeId = outNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.output.node is MicroVariableNodeView.InternalNodeView outVarView)
                this.outNodeId = outVarView.nodeView.editorInfo.NodeId;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            var outNodeView = graphView.GetElement<Node>(outNodeId);
            var inNodeView = graphView.GetElement<Node>(inNodeId);
            if (outNodeView == null || inNodeView == null)
                return false;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(this.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inTempVarNodeView)
                inPort = inTempVarNodeView.nodeView.Input;

            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(this.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView outTempVarNodeView)
                outPort = outTempVarNodeView.nodeView.OutPut;
            outPort.Connect(inPort);
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            var outNodeView = graphView.GetElement<Node>(outNodeId);
            var inNodeView = graphView.GetElement<Node>(inNodeId);
            if (outNodeView == null || inNodeView == null)
                return false;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(this.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inTempVarNodeView)
                inPort = inTempVarNodeView.nodeView.Input;

            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(this.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView outTempVarNodeView)
                outPort = outTempVarNodeView.nodeView.OutPut;
            var edge = outPort.view.connections.FirstOrDefault(a => a.input == inPort.view && a.output == outPort.view);
            if (edge != null)
                graphView.View.AddToSelection(edge);
            return true;
        }
    }
    /// <summary>
    /// 连线删除撤销
    /// </summary>
    internal class MicroEdgeDeleteRecord : IMicroGraphRecordCommand
    {
        int IMicroGraphRecordCommand.Priority => MicroGraphOperate.EDGE_VIEW_RECORD_PRIORITY;
        private int inNodeId;
        private int outNodeId;
        private string inKey;
        private string outKey;
        public void Record(BaseMicroGraphView graphView, object target)
        {
            MicroEdgeView edgeView = (MicroEdgeView)target;
            MicroPort.InternalPort inPort = edgeView.input as MicroPort.InternalPort;
            MicroPort.InternalPort outPort = edgeView.output as MicroPort.InternalPort;
            this.inKey = inPort.microPort.key;
            this.outKey = outPort.microPort.key;

            if (edgeView.input.node is BaseMicroNodeView.InternalNodeView inNodeView)
                this.inNodeId = inNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.input.node is MicroVariableNodeView.InternalNodeView inVarView)
                this.inNodeId = inVarView.nodeView.editorInfo.NodeId;

            if (edgeView.output.node is BaseMicroNodeView.InternalNodeView outNodeView)
                this.outNodeId = outNodeView.nodeView.editorInfo.NodeId;
            else if (edgeView.output.node is MicroVariableNodeView.InternalNodeView outVarView)
                this.outNodeId = outVarView.nodeView.editorInfo.NodeId;
        }

        public bool Redo(BaseMicroGraphView graphView)
        {
            var outNodeView = graphView.GetElement<Node>(outNodeId);
            var inNodeView = graphView.GetElement<Node>(inNodeId);
            if (outNodeView == null || inNodeView == null)
                return false;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(this.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inTempVarNodeView)
                inPort = inTempVarNodeView.nodeView.Input;

            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(this.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView outTempVarNodeView)
                outPort = outTempVarNodeView.nodeView.OutPut;
            if (inPort == outPort || inPort == null || outPort == null)
                return false;
            var edge = outPort.view.connections.FirstOrDefault(a => a.input == inPort.view && a.output == outPort.view);
            if (edge != null)
                graphView.View.AddToSelection(edge);
            return true;
        }

        public bool Undo(BaseMicroGraphView graphView)
        {
            var outNodeView = graphView.GetElement<Node>(outNodeId);
            var inNodeView = graphView.GetElement<Node>(inNodeId);
            if (outNodeView == null || inNodeView == null)
                return false;
            MicroPort inPort = default, outPort = default;
            if (inNodeView is BaseMicroNodeView.InternalNodeView inTempNodeView)
                inPort = inTempNodeView.nodeView.GetMicroPort(this.inKey, true);
            else if (inNodeView is MicroVariableNodeView.InternalNodeView inTempVarNodeView)
                inPort = inTempVarNodeView.nodeView.Input;

            if (outNodeView is BaseMicroNodeView.InternalNodeView outTempNodeView)
                outPort = outTempNodeView.nodeView.GetMicroPort(this.outKey, false);
            else if (outNodeView is MicroVariableNodeView.InternalNodeView outTempVarNodeView)
                outPort = outTempVarNodeView.nodeView.OutPut;
            if (inPort == outPort || inPort == null || outPort == null)
                return false;
            outPort.Connect(inPort);
            return true;
        }
    }
}
