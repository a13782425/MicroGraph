using MicroGraph.Editor;
using MicroGraph.FlowExample.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MicroGraph.FlowExample.Editor
{
    [MicroGraphEditor(typeof(FlowGraph))]
    public class FlowGraphView : BaseMicroGraphView
    {
        protected override List<Type> getUniqueNodeTypes()
        {
            return new List<Type> { typeof(FlowStartNode) };
        }

        protected override List<Type> getUsableNodeTypes()
        {
            return TypeCache.GetTypesDerivedFrom<BaseFlowNode>().ToList();
        }
    }

}