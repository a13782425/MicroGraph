using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(bool))]
    public class NodeBooleanField : BaseNodeFieldElement<bool>
    {
        private Toggle _toggle;

        public override bool Value { get => _toggle.value; set => _toggle.value = value; }

        protected override VisualElement getInputElement()
        {
            _toggle = new Toggle();
            _toggle.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _toggle;
        }

    }
}
