using MicroGraph.Runtime;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{

    [MicroGraphEditor(typeof(string))]
    internal class StringVariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            TextField inputField = new TextField();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
                .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.multiline = true;
            inputField.value = variable.GetValue()?.ToString();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
