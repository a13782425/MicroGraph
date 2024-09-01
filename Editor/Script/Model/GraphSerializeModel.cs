using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图节点序列化
    /// </summary>
    [Serializable]
    public class MicroNodeSerializeModel
    {
        public int NodeId;
        public string ClassName;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
    }
    /// <summary>
    /// 微图变量序列化
    /// </summary>
    [Serializable]
    public class MicroVarSerializeModel
    {
        public string VarName;
        public string VarClassName;
        [SerializeField]
        public bool CanRename = true;
        [SerializeField]
        public bool CanDelete = true;
        /// <summary>
        /// 可以有默认值
        /// </summary>
        [SerializeField]
        public bool CanDefaultValue = true;
        /// <summary>
        /// 可以被赋值
        /// </summary>
        [SerializeField]
        public bool CanAssign = true;
    }
    /// <summary>
    /// 微图变量节点序列化
    /// </summary>
    [Serializable]
    public class MicroVarNodeSerializeModel
    {
        public int NodeId;
        public string VarName;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
    }
    /// <summary>
    /// 微图连线序列化
    /// </summary>
    [Serializable]
    public class MicroEdgeSerializeModel
    {
        public int InNodeId;
        public int OutNodeId;
        public string InKey;
        public string OutKey;
    }
    /// <summary>
    /// 微图分组序列化
    /// </summary>
    [Serializable]
    public class MicroGroupSerializeModel
    {
        [SerializeField]
        public Vector2 Pos = default;
        [SerializeField]
        public Color Color = default;
        [SerializeField]
        public string Title = default;
        [SerializeField]
        public List<int> NodeIds = new List<int>();
    }

    /// <summary>
    /// 微图便笺序列化
    /// </summary>
    [Serializable]
    public class MicroStickySerializeModel
    {
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
