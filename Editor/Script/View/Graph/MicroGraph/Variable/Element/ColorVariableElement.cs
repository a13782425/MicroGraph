using MicroGraph.Runtime;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Color))]
    internal class ColorVariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            ColorField inputField = new ColorField();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
               .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.value = (Color)variable.GetValue();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
