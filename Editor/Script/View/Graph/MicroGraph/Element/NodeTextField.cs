using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(string))]
    public class NodeStringField : BaseNodeFieldElement<string>
    {
        private TextField _textField;

        public override string Value { get => _textField.value; set => _textField.value = value; }

        protected override VisualElement getInputElement()
        {
            _textField = new TextField() { multiline = true };
            _textField.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _textField;
        }

    }
}
