using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

namespace MicroGraph.Editor
{
    internal sealed class MicroEdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;

        private List<Edge> m_EdgesToCreate;

        private List<GraphElement> m_EdgesToDelete;
        public MicroEdgeConnectorListener()
        {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        /// <summary>
        /// 当一条线连接到端口时候调用
        /// </summary>
        /// <param name="graphView"></param>
        /// <param name="edge"></param>
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();
            if (edge.input.capacity == Capacity.Single)
            {
                foreach (Edge connection in edge.input.connections)
                {
                    if (connection != edge)
                    {
                        m_EdgesToDelete.Add(connection);
                    }
                }
            }

            if (edge.output.capacity == Capacity.Single)
            {
                foreach (Edge connection2 in edge.output.connections)
                {
                    if (connection2 != edge)
                    {
                        m_EdgesToDelete.Add(connection2);
                    }
                }
            }

            if (m_EdgesToDelete.Count > 0)
            {
                graphView.DeleteElements(m_EdgesToDelete);
            }

            List<Edge> edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (Edge item in edgesToCreate)
            {
                graphView.AddElement(item);
                var input = edge.input as MicroPort.InternalPort;
                var output = edge.output as MicroPort.InternalPort;
                input.microPort.Connect(item);
                output.microPort.Connect(item);
                var view = (BaseMicroGraphView.InternalGraphView)graphView;
                if (view == null)
                    continue;
                IMicroGraphRecordCommand record = new MicroEdgeAddRecord();
                record.Record(view.graphView, (MicroEdgeView)item);
                view.graphView.Undo.AddCommand(record);
            }
        }

        /// <summary>
        /// 当一条线在空白位置松手时候调用
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="position"></param>
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    }
}
