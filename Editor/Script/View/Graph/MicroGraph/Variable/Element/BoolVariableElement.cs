using MicroGraph.Runtime;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(bool))]
    internal class BoolVariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            Toggle inputField = new Toggle();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
               .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.value = (bool)variable.GetValue();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
