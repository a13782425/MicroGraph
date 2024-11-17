#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图编辑器信息
    /// </summary>
    [Serializable]
    internal sealed class MicroGraphEditorInfo
    {
        private readonly static DateTime MINI_TIME = new DateTime(2023, 1, 1);
        private const int NODE_START_ID = 10000;

        [SerializeField]
        public string Title;
        /// <summary>
        /// 描述
        /// </summary>
        [SerializeField]
        public string Describe;
        /// <summary>
        /// 创建时间
        /// </summary>
        [SerializeField]
        private int _createTime = 0;
        /// <summary>
        /// 创建时间DateTime.Now.ToString("yyyy.MM.dd");
        /// </summary>
        public DateTime CreateTime { get => MINI_TIME.AddMinutes(_createTime); set => _createTime = (int)(value - MINI_TIME).TotalMinutes; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SerializeField]
        private int _modifyTime = 0;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get => MINI_TIME.AddMinutes(_modifyTime); set => _modifyTime = (int)(value - MINI_TIME).TotalMinutes; }
        [SerializeField]
        private int _uniqueId = NODE_START_ID;
        [SerializeField]
        public bool ShowGrid = false;
        [SerializeField]
        public bool CanZoom = false;
        [SerializeField]
        public bool ShowMiniMap = false;
        [SerializeField]
        public float Scale = 1f;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        public List<MicroGroupEditorInfo> Groups = new List<MicroGroupEditorInfo>();
        [SerializeField]
        public List<MicroNodeEditorInfo> Nodes = new List<MicroNodeEditorInfo>();
        [SerializeField]
        public List<MicroVariableEditorInfo> Variables = new List<MicroVariableEditorInfo>();
        [SerializeField]
        public List<MicroVariableNodeEditorInfo> VariableNodes = new List<MicroVariableNodeEditorInfo>();
        [SerializeField]
        public List<MicroStickyEditorInfo> Stickys = new List<MicroStickyEditorInfo>();

        /// <summary>
        /// 获取变量节点缓存
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        internal MicroVariableNodeEditorInfo GetVariableNodeEditorInfo(int onlyId) => VariableNodes.FirstOrDefault(a => a.NodeId == onlyId);
        /// <summary>
        /// 获取节点缓存
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        internal MicroNodeEditorInfo GetNodeEditorInfo(int onlyId) => Nodes.FirstOrDefault(a => a.NodeId == onlyId);
        /// <summary>
        /// 获取唯一ID
        /// </summary>
        internal int GetNodeUniqueId() => ++_uniqueId;
        internal int SetNodeUniqueId(int value) => _uniqueId = Math.Max(NODE_START_ID, value);
    }
}

#endif