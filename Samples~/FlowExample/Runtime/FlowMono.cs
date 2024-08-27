using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MicroGraph.FlowExample.Runtime
{
    public class FlowMono : MonoBehaviour
    {
        public FlowGraph Graph;

        private FlowRuntime runtime;

        void Start()
        {
            if (Graph == null)
                return;
            runtime = new FlowRuntime(Graph);
            runtime.PlayGraph();
        }
        private void Update()
        {
            runtime?.UpdateGraph(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}