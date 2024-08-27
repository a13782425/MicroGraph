using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(int))]
    public class NodeIntegerField : BaseNodeFieldElement<int>
    {
        private IntegerField _integerField;

        public override int Value { get => _integerField.value; set => _integerField.value = value; }

        protected override VisualElement getInputElement()
        {
            _integerField = new IntegerField();
            _integerField.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _integerField;
        }
    }
}
