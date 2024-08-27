using MicroGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.MonoExample.Editor
{
    [MicroGraphEditor(typeof(Transform))]
    public class TransformDrawer : BaseNodeFieldElement<Transform>
    {
        protected override VisualElement getInputElement()
        {
            return null;
        }
    }
}
