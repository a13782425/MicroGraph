using MicroGraph.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MicroGraph.FlowExample.Runtime
{
    [MicroGraph("Á÷³ÌÍ¼")]
    public class FlowGraph : BaseMicroGraph
    {
        public override List<BaseMicroNode> GetStartNode()
        {
            return Nodes.Where(a => a.GetType() == typeof(FlowStartNode)).ToList();
        }
    }
}