#if MICRO_GRAPH_DEBUG
using MicroGraph.Runtime;
using System.Collections.Generic;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 调试微图信息
    /// </summary>
    internal class DebuggerGraphContainerData
    {
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId;

        private Dictionary<string, DebuggerGraphEditorData> _debugGraphEditorData = new Dictionary<string, DebuggerGraphEditorData>();

        internal Dictionary<string, DebuggerGraphEditorData> DebugGraphEditorData => _debugGraphEditorData;
    }
    /// <summary>
    /// 调试微图信息
    /// </summary>
    internal class DebuggerGraphEditorData
    {
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId;
        /// <summary>
        /// 运行时名称
        /// </summary>
        public string runtimeName;
        /// <summary>
        /// 节点状态
        /// </summary>
        public Dictionary<int, NodeState> nodeDatas = new Dictionary<int, NodeState>();
        /// <summary>
        /// 变量信息
        /// </summary>
        public Dictionary<string, string> varDatas = new Dictionary<string, string>();
    }
}

#endif