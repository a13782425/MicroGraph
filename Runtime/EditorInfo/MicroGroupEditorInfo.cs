#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图分组编辑器信息
    /// </summary>
    [Serializable]
    internal sealed class MicroGroupEditorInfo
    {
        [SerializeField]
        public int GroupId;
        [SerializeField]
        public string Title;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        public Vector2 Size = Vector2.one;
        [SerializeField]
        public Color GroupColor = Color.white;
        /// <summary>
        /// 当前分组拥有的节点
        /// </summary>
        [SerializeField]
        public List<int> Nodes = new List<int>();
    }
}

#endif