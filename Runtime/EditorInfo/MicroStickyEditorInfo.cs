#if UNITY_EDITOR
using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    [Serializable]
    internal sealed class MicroStickyEditorInfo
    {
        [SerializeField]
        public int NodeId;
        [SerializeField]
        public string Content;
        [SerializeField]
        public int Theme;
        [SerializeField]
        public int FontStyle;
        [SerializeField]
        public int FontSize;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        public Vector2 Size = Vector2.zero;
    }
}

#endif