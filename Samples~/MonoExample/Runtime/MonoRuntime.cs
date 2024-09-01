using MicroGraph.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace MicroGraph.MonoExample.Runtime
{
    public class MonoRuntime : MonoBehaviour
    {
        private MicroGraphRuntime _runtime;
        public MonoGraph graph;

        private void Awake()
        {
            if (graph != null)
            {
                _runtime = new MicroGraphRuntime(graph);
                _runtime.RuntimeGraph.GetVariable("tran").SetValue(this.transform);
                _runtime.onStateChanged += m_runtime_onStateChanged;
                _runtime.Play(graph.AwakeNodeId);
            }
        }

        private void m_runtime_onStateChanged(int startNodeId, MicroGraphRuntimeState oldState, MicroGraphRuntimeState newState)
        {
            if (startNodeId == graph.AwakeNodeId)
            {
                if (newState == MicroGraphRuntimeState.Exit)
                {
                    _runtime.Play(graph.UpdateNodeId);
                }
            }
            else if (startNodeId == graph.UpdateNodeId)
            {
                if (newState == MicroGraphRuntimeState.Exit)
                {
                    _runtime.Play(graph.UpdateNodeId);
                }
            }
        }

        private void Update()
        {
            if (_runtime == null)
                return;
            _runtime.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            if (_runtime == null)
                return;
            _runtime.Play(graph.DestroyNodeId);
            _runtime.Exit();
        }

    }
}