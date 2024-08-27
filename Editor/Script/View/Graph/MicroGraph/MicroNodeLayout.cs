using UnityEditor.Experimental.GraphView;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微节点布局
    /// </summary>
    internal abstract class MicroNodeLayout
    {
        internal abstract Orientation orientation { get; }
        protected Node node { get; private set; }
        public MicroNodeLayout(Node node) => this.node = node;
        /// <summary>
        /// 节点布局
        /// </summary>
        /// <param name="node"></param>
        public abstract void NodeLayout();

        protected void removeExpanded()
        {
            node.extensionContainer.RemoveFromHierarchy();
        }
    }
}
