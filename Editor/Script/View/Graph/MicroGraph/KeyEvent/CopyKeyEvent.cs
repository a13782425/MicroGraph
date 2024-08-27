using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class CopyKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.C;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            if (!graphView.CanCopy)
                return false;

            MicroCopyPasteOperateData copyData = new MicroCopyPasteOperateData();

            List<MicroGroupView> groups = graphView.View.selection.Where(x => x is MicroGroupView).OfType<MicroGroupView>().ToList();

            var others = graphView.View.selection
                .Union(groups.SelectMany(a => a.containedElements)).ToList();
            m_checkSelections(graphView, others, copyData);
            MicroGraphOperate.CopyDatas[graphView.GetType()] = copyData;
            return true;
        }


        private void m_checkSelections(BaseMicroGraphView graphView, List<ISelectable> selection, MicroCopyPasteOperateData copyData)
        {
            int count = 0;
            Vector2 sum = Vector2.zero;
            IMicroGraphCopyPaste paste = null;
            foreach (var item in selection)
            {
                switch (item)
                {
                    case MicroGroupView group:
                        paste = MicroGraphProvider.GetCopyPasteImpl(group.GetType());
                        paste.Copy(graphView, group);
                        copyData.groups.Add(paste);
                        break;
                    case MicroEdgeView edge:
                        paste = MicroGraphProvider.GetCopyPasteImpl(edge.GetType());
                        paste.Copy(graphView, edge);
                        copyData.edges.Add(paste);
                        break;
                    case MicroVariableItemView varItem:
                        paste = MicroGraphProvider.GetCopyPasteImpl(varItem.GetType());
                        paste.Copy(graphView, varItem);
                        copyData.variables.Add(paste);
                        break;
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        paste = MicroGraphProvider.GetCopyPasteImpl(nodeView.nodeView.GetType());
                        paste.Copy(graphView, nodeView.nodeView);
                        copyData.elements.Add(paste);
                        count++;
                        sum += nodeView.GetPosition().position;
                        break;
                    case MicroVariableNodeView.InternalNodeView varNodeView:
                        paste = MicroGraphProvider.GetCopyPasteImpl(varNodeView.nodeView.GetType());
                        paste.Copy(graphView, varNodeView.nodeView);
                        copyData.elements.Add(paste);
                        count++;
                        sum += varNodeView.GetPosition().position;
                        break;
                    case MicroStickyNoteView stickyNoteView:
                        paste = MicroGraphProvider.GetCopyPasteImpl(stickyNoteView.GetType());
                        paste.Copy(graphView, stickyNoteView);
                        copyData.elements.Add(paste);
                        count++;
                        sum += stickyNoteView.GetPosition().position;
                        break;
                    default:
                        break;
                }
            }
            copyData.centerPos = sum / count;
        }

    }
}
