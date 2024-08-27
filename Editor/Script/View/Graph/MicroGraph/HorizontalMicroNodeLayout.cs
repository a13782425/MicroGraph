using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{

    /// <summary>
    /// 横向微节点布局
    /// </summary>
    internal sealed class HorizontalMicroNodeLayout : MicroNodeLayout
    {
        internal override Orientation orientation => Orientation.Horizontal;
        public HorizontalMicroNodeLayout(Node node) : base(node)
        {
        }

        public override void NodeLayout()
        {
            removeExpanded(); 
            node.topContainer.SetDisplay(true);
            node.topContainer.Insert(0, this.node.inputContainer);
            node.topContainer.Add(this.node.outputContainer);
            this.node.inputContainer.RemoveFromClassList("vertical_port_input");
            this.node.outputContainer.RemoveFromClassList("vertical_port_output");
            this.node.inputContainer.AddToClassList("horizontal_port_input");
            this.node.outputContainer.AddToClassList("horizontal_port_output");
            node.mainContainer.style.overflow = Overflow.Visible;
        }
    }
}
