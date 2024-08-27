using MicroGraph.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(int))]
    internal class IntVariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            IntegerField inputField = new IntegerField();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
               .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.value = (int)variable.GetValue();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
