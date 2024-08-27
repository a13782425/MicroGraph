using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class UndoKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.Z;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            graphView.Undo.Undo();
            return true;
        }
    }
}
