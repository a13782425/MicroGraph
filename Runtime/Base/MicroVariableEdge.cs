using System;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 变量的连线数据
    /// </summary>
    [Serializable]
    public partial class MicroVariableEdge : IMicroGraphClone
    {
#if UNITY_EDITOR
        /// <summary>
        /// 连线的节点ID
        /// </summary>
        public int nodeId = 0;
#endif

        /// <summary>
        /// 是否是输入
        /// </summary>
        public bool isInput = true;
        /// <summary>
        /// 当前线对应的图内变量名
        /// </summary>
        public string varName = "";
        /// <summary>
        /// 当前线对应的节点内变量名
        /// </summary>
        public string fieldName = "";
    }
}
