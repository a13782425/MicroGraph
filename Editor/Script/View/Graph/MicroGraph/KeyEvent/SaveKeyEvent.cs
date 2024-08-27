using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class SaveKeyEvent : BaseMicroGraphKeyEvent
    {

        public override KeyCode Code => KeyCode.S;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            graphView.Save();
            return true;
        }
    }
}
