using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图配置信息
    /// </summary>
    [Serializable]
    internal class MicroGraphConfigModel
    {
        /// <summary>
        /// [大版本][小版本][Bug修复]
        /// 大版本 大于0 为正式版
        /// </summary>
        public const string VERSIONS = "0.0.1 A";

        /// <summary>
        /// 默认开放网格
        /// </summary>
        public bool defaultOpenGrid = false;
        /// <summary>
        /// 默认开放缩放
        /// </summary>
        public bool defaultOpenZoom = false;

        /// <summary>
        /// 编辑器字体
        /// </summary>
        public string editorFont = "";

        /// <summary>
        /// 记录存储目录
        /// </summary>
        public bool recordSavePath = true;
        /// <summary>
        /// 默认保存路径
        /// </summary>
        public string savePath = "";

        /// <summary>
        /// 回退步数
        /// </summary>
        public int undoStep = 128;

        /// <summary>
        /// 图表标题长度
        /// </summary>
        public int graphTitleLength = 64;
        /// <summary>
        /// 节点标题长度
        /// </summary>
        public int nodeTitleLength = 64;
        /// <summary>
        /// 分组标题长度
        /// </summary>
        public int groupTitleLength = 64;

        /// <summary>
        /// 预览图设置
        /// </summary>
        [SerializeField]
        public OverviewGraphConfig OverviewConfig = new OverviewGraphConfig();

        /// <summary>
        /// 模板集合
        /// </summary>
        [SerializeField]
        public List<MicroGraphTemplateModel> graphTemplates = new List<MicroGraphTemplateModel>();
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
        public string title;
        /// <summary>
        /// 微图类全名字
        /// </summary>
        public string graphClassName;
        [SerializeField]
        public List<MicroVarSerializeModel> vars = new List<MicroVarSerializeModel>();
        [SerializeField]
        public List<MicroNodeSerializeModel> nodes = new List<MicroNodeSerializeModel>();
        [SerializeField]
        public List<MicroVarNodeSerializeModel> varNodes = new List<MicroVarNodeSerializeModel>();
        [SerializeField]
        public List<MicroEdgeSerializeModel> edges = new List<MicroEdgeSerializeModel>();
        [SerializeField]
        public List<MicroGroupSerializeModel> groups = new List<MicroGroupSerializeModel>();
        [SerializeField]
        public List<MicroStickySerializeModel> stickys = new List<MicroStickySerializeModel>();
    }
}
