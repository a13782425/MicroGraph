using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Color))]
    public class NodeColorField : BaseNodeFieldElement<Color>
    {
        private ColorField _colorField;
        public override Color Value { get => _colorField.value; set => _colorField.value = value; }

        protected override VisualElement getInputElement()
        {
            _colorField = new ColorField();
            _colorField.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _colorField;
        }
    }
}
