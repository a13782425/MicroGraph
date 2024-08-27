using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class PasteKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.V;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            MicroCopyPasteOperateData copyData = null;
            if (!MicroGraphOperate.CopyDatas.TryGetValue(graphView.GetType(), out copyData))
                return false;
            copyData.view = graphView;
            copyData.mousePos = KeyEventUtils.MousePosToNodePos(evt.originalMousePosition, graphView);
            copyData.variables.ForEach(item => item.Paste(copyData));
            copyData.elements.ForEach(item => item.Paste(copyData));
            copyData.edges.ForEach(item => item.Paste(copyData));
            copyData.groups.ForEach(item => item.Paste(copyData));
            graphView.listener.OnEvent(MicroGraphEventIds.VAR_NODE_MODIFY);
            return true;
        }
    }
}
