using MicroGraph.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(float))]
    internal class FloatVariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            FloatField inputField = new FloatField();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
               .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.value = (float)variable.GetValue();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
