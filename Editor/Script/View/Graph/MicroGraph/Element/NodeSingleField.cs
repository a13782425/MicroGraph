using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(float))]
    public class NodeSingleField : BaseNodeFieldElement<float>
    {
        private FloatField _floatField;

        public override float Value { get => _floatField.value; set => _floatField.value = value; }

        protected override VisualElement getInputElement()
        {
            _floatField = new FloatField();
            _floatField.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _floatField;
        }

    }
}
