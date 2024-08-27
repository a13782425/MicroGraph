using MicroGraph.Runtime;
using System.Collections.Generic;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 事件ID
    /// 内部ID从-10000开始向下递增
    /// </summary>
    public class MicroGraphEventIds
    {
        /// <summary>
        /// 逻辑图资源变化
        /// args : GraphAssetsChangedEventArgs
        /// </summary>
        public const int GRAPH_ASSETS_CHANGED = -10000;

        /// <summary>
        /// 变量变化
        /// args : VarModifyEventArgs
        /// </summary>
        public const int VAR_MODIFY = -10100;
        /// <summary>
        /// 变量节点变化
        /// </summary>
        public const int VAR_NODE_MODIFY = -10101;
        /// <summary>
        /// 增加一个变量
        /// args : VarAddEventArgs
        /// </summary>
        public const int VAR_ADD = -10200;
        /// <summary>
        /// 删除一个变量
        /// args : VarDelEventArgs
        /// </summary>
        public const int VAR_DEL = -10300;
        /// <summary>
        /// 编辑器设置变化
        /// args : 变化字段
        /// </summary>
        public const int EDITOR_SETTING_CHANGED = -10500;
        /// <summary>
        /// 编辑器模式发生改变
        /// args ：PlayModeStateChange
        /// </summary>
        public const int EDITOR_PLAY_MODE_CHANGED = -10600;
        /// <summary>
        /// 预览视图变化
        /// </summary>
        public const int OVERVIEW_CHANGED = -10700;
        /// <summary>
        /// 变化
        /// </summary>
        public const int GRAPH_TEMPLATE_CHANGED = -10800;
    }

    /// <summary>
    /// 逻辑图资源变化
    /// </summary>
    public sealed class GraphAssetsChangedEventArgs
    {
        /// <summary>
        /// 移动了的逻辑图
        /// </summary>
        public List<string> moveGraphs = new List<string>();
        /// <summary>
        /// 删除了的逻辑图
        /// 里面存的不一定是逻辑图的assets
        /// </summary>
        public List<string> deletedGraphs = new List<string>();
        /// <summary>
        /// 新增的逻辑图
        /// </summary>
        public List<string> addGraphs = new List<string>();

    }
    /// <summary>
    /// 变量变化
    /// </summary>
    public sealed class VarModifyEventArgs
    {
        /// <summary>
        /// 旧名
        /// </summary>
        public string oldVarName;
        /// <summary>
        /// 变化的变量
        /// </summary>
        public BaseMicroVariable var;
    }

    /// <summary>
    /// 新增变量
    /// </summary>
    public sealed class VarAddEventArgs
    {
        /// <summary>
        /// 新增变量
        /// </summary>
        public BaseMicroVariable var;
    }
    /// <summary>
    /// 删除变量
    /// </summary>
    public sealed class VarDelEventArgs
    {
        /// <summary>
        /// 删除的变量
        /// </summary>
        public BaseMicroVariable var;
    }
}
