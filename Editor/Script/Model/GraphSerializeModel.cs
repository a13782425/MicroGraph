using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图节点序列化
    /// </summary>
    [Serializable]
    public class MicroNodeSerializeModel
    {
        public int nodeId;
        public string className;
        [SerializeField]
        public Vector2 pos = Vector2.zero;
    }
    /// <summary>
    /// 微图变量序列化
    /// </summary>
    [Serializable]
    public class MicroVarSerializeModel
    {
        public string varName;
        public string varClassName;
        [SerializeField]
        public bool canRename = true;
        [SerializeField]
        public bool canDelete = true;
        /// <summary>
        /// 可以有默认值
        /// </summary>
        [SerializeField]
        public bool canDefaultValue = true;
        /// <summary>
        /// 可以被赋值
        /// </summary>
        [SerializeField]
        public bool canAssign = true;
    }
    /// <summary>
    /// 微图变量节点序列化
    /// </summary>
    [Serializable]
    public class MicroVarNodeSerializeModel
    {
        public int nodeId;
        public string varName;
        [SerializeField]
        public Vector2 pos = Vector2.zero;
    }
    /// <summary>
    /// 微图连线序列化
    /// </summary>
    [Serializable]
    public class MicroEdgeSerializeModel
    {
        public int inNodeId;
        public int outNodeId;
        public string inKey;
        public string outKey;
    }
    /// <summary>
    /// 微图分组序列化
    /// </summary>
    [Serializable]
    public class MicroGroupSerializeModel
    {
        [SerializeField]
        public Vector2 pos = default;
        [SerializeField]
        public Color color = default;
        [SerializeField]
        public string title = default;
        [SerializeField]
        public List<int> nodeIds = new List<int>();
    }

    /// <summary>
    /// 微图便笺序列化
    /// </summary>
    [Serializable]
    public class MicroStickySerializeModel
    {
        [SerializeField]
        public string content;
        [SerializeField]
        public int theme;
        [SerializeField]
        public int fontStyle;
        [SerializeField]
        public int fontSize;
        [SerializeField]
        public Vector2 pos = Vector2.zero;
        [SerializeField]
        public Vector2 size = Vector2.zero;
    }
}
