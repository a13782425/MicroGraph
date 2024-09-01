using MicroGraph.Runtime;
using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Enum))]
    internal class NodeEnumField : BaseNodeFieldElement<Enum>
    {
        private SearchPopupField _element;

        private Type _enumType;
        public override Enum Value
        {
            get
            {
                if (Enum.TryParse(_enumType, _element.value, out object result))
                {
                    return (Enum)result;
                }
                return default;
            }
            set
            {
                _element.value = value.ToString();
            }
        }

        public override void DrawElement(BaseMicroNodeView nodeView, FieldInfo field, PortDirEnum portDir)
        {
            _enumType = field.FieldType;
            base.DrawElement(nodeView, field, portDir);
        }

        protected override VisualElement getInputElement()
        {
            _element = new SearchPopupField();
            _element.getContent += m_getContent;
            _element.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            _element.RegisterValueChangedCallback(m_valueChanged);
            return _element;
        }

        private void m_valueChanged(ChangeEvent<string> evt)
        {
            if (Enum.TryParse(_enumType, _element.value, out object result))
                Field.SetValue(this.nodeView.Target, result);
        }

        private SearchPopupContent m_getContent()
        {
            SearchPopupContent popupContent = new SearchPopupContent();
            if (_enumType == null)
                return popupContent;
            foreach (var item in Enum.GetValues(_enumType))
            {
                popupContent.AppendValue(new SearchPopupContent.ValueItem() { value = item.ToString() });

            }
            return popupContent;

        }
    }
}
