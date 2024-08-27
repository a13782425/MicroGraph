using MicroGraph.Runtime;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图操作数据
    /// 编辑器内部使用
    /// </summary>
    internal sealed class GraphOperateModel
    {
        private GraphSummaryModel _summaryModel = null;
        /// <summary>
        /// 当前展示微图的简介
        /// </summary>
        public GraphSummaryModel summaryModel => _summaryModel;
        private GraphCategoryModel _categoryModel = null;
        /// <summary>
        /// 微图分类信息数据
        /// </summary>
        public GraphCategoryModel categoryModel => _categoryModel;
        private BaseMicroGraph _microGraph = null;
        /// <summary>
        /// 微图数据
        /// </summary>
        public BaseMicroGraph microGraph => _microGraph;

        private MicroGraphEditorInfo _editorInfo = null;
        /// <summary>
        /// 微图编辑器数据
        /// </summary>
        public MicroGraphEditorInfo editorInfo => _editorInfo;
        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh(string onlyId)
        {
            MicroGraphUtils.UnloadObject(_microGraph);
            if (string.IsNullOrWhiteSpace(onlyId))
            {
                _summaryModel = null;
                _categoryModel = null;
                _microGraph = null;
                _editorInfo = null;
            }
            else
            {
                _summaryModel = MicroGraphProvider.GetGraphSummary(onlyId);
                _categoryModel = MicroGraphProvider.GetGraphCategory(_summaryModel.GraphClassName);
                BaseMicroGraph graph = MicroGraphUtils.GetMicroGraph(_summaryModel.AssetPath);
                _microGraph = graph;
                _editorInfo = graph.editorInfo;
            }
        }
    }
}
