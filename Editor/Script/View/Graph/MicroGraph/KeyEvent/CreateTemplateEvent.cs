using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal class CreateTemplateEvent : BaseMicroGraphKeyEvent
    {
        public override KeyCode Code =>  KeyCode.T;

        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            graphView.AddGraphTemplate();
            return true;
        }
    }
}
