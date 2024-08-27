using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    [Serializable]
    internal class MicroVariableNodeEditorInfo
    {
        [SerializeField]
        public int NodeId;
        [SerializeField]
        public string Name;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [NonSerialized]
        [HideInInspector]
        public MicroVariableEditorInfo EditorInfo;
    }
}
