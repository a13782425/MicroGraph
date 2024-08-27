using MicroGraph.Runtime;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphEditor(typeof(Vector3))]
    internal class Vector3VariableElement : IVariableElement
    {
        public VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt)
        {
            Vector3Field inputField = new Vector3Field();
            inputField.label = "值:";
            inputField.labelElement.AddTailwindCSS(TailwindCSS.W_6)
               .AddTailwindCSS(TailwindCSS.MinW_0);
            inputField.value = (Vector3)variable.GetValue();
            inputField.RegisterValueChangedCallback(a => variable.SetValue(a.newValue));
            return inputField;
        }
    }
}
