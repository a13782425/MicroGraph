using UnityEditor.Experimental.GraphView;

namespace MicroGraph.Editor
{
    internal class MicroVariableRowView : BlackboardRow
    {
        public MicroVariableItemView ItemView { get; private set; }
        public MicroVariablePropView PropView { get; private set; }
        public MicroVariableRowView(MicroVariableItemView item, MicroVariablePropView propertyView) : base(item, propertyView)
        {
            ItemView = item;
            PropView = propertyView;
        }
    }
}
