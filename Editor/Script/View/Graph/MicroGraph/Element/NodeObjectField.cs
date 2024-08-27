using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(System.Object))]
    public class NodeObjectField : BaseNodeFieldElement<System.Object>
    {
        private Vector3Field _element;

        protected override VisualElement getInputElement()
        {
            //Debug.LogWarning("Object类型仅支持In或Out");
            return null;
        }
    }
}
