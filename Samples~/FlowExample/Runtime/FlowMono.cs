using MicroGraph.Runtime;
using System.Linq;
using UnityEngine;
namespace MicroGraph.FlowExample.Runtime
{
    public class FlowMono : MonoBehaviour
    {
        public FlowGraph Graph;

        private MicroGraphRuntime runtime;

        void Start()
        {
            if (Graph == null)
                return;
            runtime = new MicroGraphRuntime(Graph);
            runtime.Play(Graph.Nodes.FirstOrDefault(a => a.GetType() == typeof(FlowStartNode)).OnlyId);
        }
        private void Update()
        {
            runtime?.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}