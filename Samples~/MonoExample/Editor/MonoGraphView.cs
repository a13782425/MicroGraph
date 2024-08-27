using MicroGraph.Editor;

using MicroGraph.MonoExample.Runtime;
using MicroGraph.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MicroGraph.MonoExample.Editor
{
    [MicroGraphEditor(typeof(MonoGraph))]
    public class MonoGraphView : BaseMicroGraphView
    {
        private MonoGraph monoGraph;
        protected override void onInit()
        {
            monoGraph = Target as MonoGraph;
            if (Target.Variables.FirstOrDefault(a => a.Name == "tran") == null)
            {
                AddVariable<Transform>("tran", false, false, false, false);
            }
            BaseMicroNode node = Target.Nodes.FirstOrDefault(a => a.GetType() == typeof(AwakeMonoNode));
            if (node == null)
                node = AddNode<AwakeMonoNode>(Vector2.zero);
            monoGraph.AwakeNodeId = node.OnlyId;

            node = Target.Nodes.FirstOrDefault(a => a.GetType() == typeof(UpdateMonoNode));
            if (node == null)
                node = AddNode<UpdateMonoNode>(new Vector2(0, 128));
            monoGraph.UpdateNodeId = node.OnlyId;

            node = Target.Nodes.FirstOrDefault(a => a.GetType() == typeof(DestroyMonoNode));
            if (node == null)
                node = AddNode<DestroyMonoNode>(new Vector2(0, 256));
            monoGraph.DestroyNodeId = node.OnlyId;
        }
        protected override List<Type> getUsableNodeTypes()
        {
            return TypeCache.GetTypesDerivedFrom<BaseMonoNode>().ToList();
        }
        protected override List<Type> getUniqueNodeTypes()
        {
            return new List<Type>() { typeof(AwakeMonoNode), typeof(UpdateMonoNode), typeof(DestroyMonoNode) };
        }
    }
}