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
        private EnumField _element;

        private Type _enumType;
        public override Enum Value
        {
            get => _element.value; 
            set
            {
                _element.value = value;
            }
        }

        public override void DrawElement(BaseMicroNodeView nodeView, FieldInfo field, PortDirEnum portDir)
        {
            base.DrawElement(nodeView, field, portDir);
            Enum @enum= (Enum)field.GetValue(nodeView.Target);
            _element.Init(@enum);
        }

        protected override VisualElement getInputElement()
        {
            _element = new EnumField();
            _element.labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
            return _element;
        }
    }
}
