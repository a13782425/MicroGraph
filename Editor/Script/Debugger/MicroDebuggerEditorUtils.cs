#if MICRO_GRAPH_DEBUG
using MicroGraph.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    internal static class MicroDebuggerEditorUtils
    {
        /// <summary>
        /// 调试延时(毫秒)
        /// </summary>
        internal const int DEBUGGER_TIMEOUT = 10000;
        /// <summary>
        /// 调试视图元素的层级
        /// </summary>
        internal const int DEBUGGER_LAYER = 3;

        public readonly static Dictionary<NodeState, MicroDebuggerNodeState> DebuggerNodeStates = new Dictionary<NodeState, MicroDebuggerNodeState>()
        {
            { NodeState.None, new MicroDebuggerNodeState() { color = new Color(0.2f,0.2f,0.2f,1), state = NodeState.None, name = "空"} },
            { NodeState.Skip, new MicroDebuggerNodeState() { color = new Color(0.5f,0.5f,0.5f,1), state = NodeState.Skip, name = "跳过" } },
            { NodeState.Inited, new MicroDebuggerNodeState() { color = new Color(1f,1f,1f,1), state = NodeState.Inited, name = "初始化" } },
            { NodeState.Running, new MicroDebuggerNodeState() { color = new Color(0f,0f,1f,1), state = NodeState.Running, name = "运行中" } },
            { NodeState.Success, new MicroDebuggerNodeState() { color = new Color(0f,1f,0f,1), state = NodeState.Success, name = "完成" } },
            { NodeState.Failed, new MicroDebuggerNodeState() { color = new Color(1f,0f,0f,1), state = NodeState.Failed, name = "失败" } },
            { NodeState.Exit, new MicroDebuggerNodeState() { color = new Color(0f,0f,0f,1), state = NodeState.Exit, name = "退出" } },

        };
        public class MicroDebuggerNodeState
        {
            /// <summary>
            /// 状态颜色
            /// </summary>
            public Color color;
            /// <summary>
            /// 对应状态
            /// </summary>
            public NodeState state;
            /// <summary>
            /// 状态名
            /// </summary>
            public string name;
        }
    }

}

#endif