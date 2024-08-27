using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class CopyAndPasteKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.D;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            new CopyKeyEvent().Execute(evt, graphView);
            new PasteKeyEvent().Execute(evt, graphView);
            return true;
        }
    }
}
