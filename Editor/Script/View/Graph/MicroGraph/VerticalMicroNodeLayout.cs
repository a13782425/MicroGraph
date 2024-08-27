using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 纵向微节点布局
    /// </summary>
    internal class VerticalMicroNodeLayout : MicroNodeLayout
    {
        internal override Orientation orientation => Orientation.Vertical;
        public VerticalMicroNodeLayout(Node node) : base(node)
        {
        }

        public override void NodeLayout()
        {
            removeExpanded();
            node.mainContainer.Insert(0, this.node.inputContainer);
            node.mainContainer.Add(this.node.outputContainer);
            node.topContainer.SetDisplay(false);
            this.node.inputContainer.RemoveFromClassList("horizontal_port_input");
            this.node.outputContainer.RemoveFromClassList("horizontal_port_output");
            this.node.inputContainer.AddToClassList("vertical_port_input");
            this.node.outputContainer.AddToClassList("vertical_port_output");
            node.mainContainer.style.overflow = Overflow.Visible;
        }
    }
}
