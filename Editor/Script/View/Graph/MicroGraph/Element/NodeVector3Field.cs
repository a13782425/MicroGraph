using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Vector3))]
    public class NodeVector3Field : BaseNodeFieldElement<Vector3>
    {
        private Vector3Field _element;

        public override Vector3 Value { get => _element.value; set => _element.value = value; }

        protected override VisualElement getInputElement()
        {
            _element = new Vector3Field();
            _element.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _element;
        }

    }
}
