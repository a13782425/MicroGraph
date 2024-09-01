using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图全局配置信息
    /// </summary>
    [Serializable]
    internal class MicroGraphGlobalConfigModel
    {
        /// <summary>
        /// [大版本][小版本][Bug修复]
        /// 大版本 大于0 为正式版
        /// </summary>
        public const string VERSIONS = "0.1.0 A";

        /// <summary>
        /// 默认开放网格
        /// </summary>
        public bool DefaultOpenGrid = false;
        /// <summary>
        /// 默认开放缩放
        /// </summary>
        public bool DefaultOpenZoom = false;

        /// <summary>
        /// 编辑器字体
        /// </summary>
        public string EditorFont = "";

        /// <summary>
        /// 记录存储目录
        /// </summary>
        public bool RecordSavePath = true;
        /// <summary>
        /// 默认保存路径
        /// </summary>
        public string SavePath = "";

        /// <summary>
        /// 回退步数
        /// </summary>
        public int UndoStep = 128;

        /// <summary>
        /// 图表标题长度
        /// </summary>
        public int GraphTitleLength = 64;
        /// <summary>
        /// 节点标题长度
        /// </summary>
        public int NodeTitleLength = 64;
        /// <summary>
        /// 分组标题长度
        /// </summary>
        public int GroupTitleLength = 64;

        /// <summary>
        /// 预览图设置
        /// </summary>
        [SerializeField]
        public OverviewGraphConfig OverviewConfig = new OverviewGraphConfig();
        /// <summary>
        /// 微图配置
        /// </summary>
        [SerializeField]
        public List<MicroGraphConfig> GraphConfigs = new List<MicroGraphConfig>();
    }
    /// <summary>
    /// 单一微图编辑器模型
    /// </summary>
    [Serializable]
    internal class MicroGraphConfig
    {
        /// <summary>
        /// 微图类全名字
        /// </summary>
        public string GraphClassName;
        /// <summary>
        /// 模板集合
        /// </summary>
        [SerializeField]
        public List<MicroGraphTemplateModel> Templates = new List<MicroGraphTemplateModel>();
    }

    /// <summary>
    /// 微图模板
    /// </summary>
    [Serializable]
    internal class MicroGraphTemplateModel
    {
        /// <summary>
        /// 收藏名字
        /// </summary>
        public string Title;
        /// <summary>
        /// 微图类全名字
        /// </summary>
        public string GraphClassName;
        [SerializeField]
        public List<MicroVarSerializeModel> Vars = new List<MicroVarSerializeModel>();
        [SerializeField]
        public List<MicroNodeSerializeModel> Nodes = new List<MicroNodeSerializeModel>();
        [SerializeField]
        public List<MicroVarNodeSerializeModel> VarNodes = new List<MicroVarNodeSerializeModel>();
        [SerializeField]
        public List<MicroEdgeSerializeModel> Edges = new List<MicroEdgeSerializeModel>();
        [SerializeField]
        public List<MicroGroupSerializeModel> Groups = new List<MicroGroupSerializeModel>();
        [SerializeField]
        public List<MicroStickySerializeModel> Stickys = new List<MicroStickySerializeModel>();
    }
}
