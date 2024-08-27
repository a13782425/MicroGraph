#if UNITY_EDITOR
using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    [Serializable]
    internal sealed class MicroNodeEditorInfo
    {
        [SerializeField]
        public int NodeId;
        [SerializeField]
        public string Title;
        [SerializeField]
        public bool IsLock;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [NonSerialized]
        [HideInInspector]
        public BaseMicroNode Target;
    }
}

#endif