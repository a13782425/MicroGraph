using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class RedoKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.Y;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            graphView.Undo.Redo();
            return true;
        }
    }
}
