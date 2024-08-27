using System;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public sealed class ToolbarView : VisualElement
    {
        public event Action onGUI;
        public ToolbarView()
        {
            Add(new IMGUIContainer(DrawImGUIToolbar));
            this.RegisterCallback<MouseDownEvent>((a) => a.StopPropagation());
            this.RegisterCallback<MouseUpEvent>((a) => a.StopPropagation());
        }
        private void DrawImGUIToolbar()
        {
            onGUI?.Invoke();
        }
    }
}
