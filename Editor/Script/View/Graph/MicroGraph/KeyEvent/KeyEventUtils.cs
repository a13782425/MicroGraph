using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal static class KeyEventUtils
    {
        internal static Vector2 MousePosToNodePos(Vector2 mousePos, BaseMicroGraphView graphView)
        {
            Vector2 screenPos = graphView.owner.GetScreenPosition(mousePos);
            //经过计算得出节点的位置
            var windowMousePosition = graphView.owner.rootVisualElement.ChangeCoordinatesTo(graphView.owner.rootVisualElement.parent, screenPos - graphView.owner.position.position);
            return graphView.View.contentViewContainer.WorldToLocal(windowMousePosition);
        }
    }
}
