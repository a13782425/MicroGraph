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
        /// 模板变化
        /// </summary>
        public const int GRAPH_TEMPLATE_CHANGED = -10800;
        /// <summary>
        /// 微图包变化
        /// args ：MicroPackageInfo
        /// </summary>
        public const int GRAPH_PACKAGE_CHANGED = -10900;
        /// <summary>
        /// 微图包移除
        /// args ：MicroPackageInfo
        /// </summary>
        public const int GRAPH_PACKAGE_DELETE = -11000;

#if MICRO_GRAPH_DEBUG
        /// <summary>
        /// 全局发生变化
        /// 调试器状态变化
        /// </summary>
        public const int DEBUGGER_STATE_CHANGED = -20000;
        /// <summary>
        /// 全局发生变化
        /// 调试器接收到数据
        /// args ：NetMessagePackage
        /// </summary>
        public const int DEBUGGER_RECEIVE_NET_DATA = -20100;
        /// <summary>
        /// 全局发生变化
        /// 调试器接受到微图数据发生变化
        /// args ：DebuggerGraphData
        /// </summary>
        public const int DEBUGGER_GLOBAL_GRAPH_DATA_CHANGED = -20210;
        /// <summary>
        /// 全局发生变化
        /// 调试器接受到微图名字数据发生变化
        /// args ：DebuggerGraphData
        /// </summary>
        public const int DEBUGGER_GLOBAL_GRAPHRENAME_DATA_CHANGED = -20220;
        /// <summary>
        /// 全局变化
        /// 删除一个微图
        /// args ：DebuggerGraphDeleteData
        /// </summary>
        public const int DEBUGGER_GLOBAL_GRAPHDELETE_DATA_CHANGED = -20221;
        /// <summary>
        /// 全局发生变化
        /// 调试器接受到微图节点数据发生变化(不会进行唯一id区分)
        /// args ：DebuggerNodeData
        /// </summary>
        public const int DEBUGGER_GLOBAL_NODE_DATA_CHANGED = -20230;
        /// <summary>
        /// 全局发生变化
        /// 调试器接受到微图变量数据发生变化(不会进行唯一id区分)
        /// args ：DebuggerVarData
        /// </summary>
        public const int DEBUGGER_GLOBAL_VAR_DATA_CHANGED = -20240;
        /// <summary>
        /// 局部发生变化
        /// 调试器微图状态变化
        /// </summary>
        public const int DEBUGGER_LOCAL_GRAPH_STATE_CHANGED = -20250;
        /// <summary>
        /// 局部发生变化
        /// 调试器接受到微图变量数据发生变化(区分了唯一id)
        /// args ：string 运行时名，可为空
        /// </summary>
        public const int DEBUGGER_LOCAL_NODE_DATA_CHANGED = -20260;
        /// <summary>
        /// 局部发生变化
        /// 调试器接受到微图变量数据发生变化(区分了唯一id)
        /// args ：string 运行时名，可为空
        /// </summary>
        public const int DEBUGGER_LOCAL_VAR_DATA_CHANGED = -20270;
#endif
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
