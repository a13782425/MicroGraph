using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Vector2))]
    public class NodeVector2Field : BaseNodeFieldElement<Vector2>
    {
        private Vector2Field _element;

        public override Vector2 Value { get => _element.value; set => _element.value = value; }

        protected override VisualElement getInputElement()
        {
            _element = new Vector2Field();
            _element.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _element;
        }

    }
}
