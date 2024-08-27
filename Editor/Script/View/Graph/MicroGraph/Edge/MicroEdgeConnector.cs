﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal sealed class MicroEdgeConnector : EdgeConnector
    {
        private MicroEdgeDragHelper dragHelper;
        private Edge edgeCandidate;
        public override EdgeDragHelper edgeDragHelper => dragHelper;
        private bool active;
        private Vector2 mouseDownPosition;
        internal const float k_ConnectionDistanceTreshold = 10f;
        public MicroEdgeConnector(IEdgeConnectorListener listener) : base()
        {
            active = false;
            dragHelper = new MicroEdgeDragHelper(listener);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }
        protected override void RegisterCallbacksOnTarget()
        {
            var graphElement = target as Port;
            if (graphElement == null)
            {
                return;
            }
            if (graphElement.direction == Direction.Input)
            {
                return;
            }
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            var graphElement = target as Port;
            if (graphElement == null)
            {
                return;
            }
            if (graphElement.direction == Direction.Input)
            {
                return;
            }
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (active)
            {
                e.StopPropagation();
                return;
            }

            if (!CanStartManipulation(e))
            {
                return;
            }

            var graphElement = target as Port;
            if (graphElement == null)
            {
                return;
            }

            mouseDownPosition = e.localMousePosition;

            edgeCandidate = new MicroEdgeView();
            edgeDragHelper.draggedPort = graphElement;
            edgeDragHelper.edgeCandidate = edgeCandidate;

            if (edgeDragHelper.HandleMouseDown(e))
            {
                active = true;
                target.CaptureMouse();

                e.StopPropagation();
            }
            else
            {
                edgeDragHelper.Reset();
                edgeCandidate = null;
            }
        }

        void OnCaptureOut(MouseCaptureOutEvent e)
        {
            active = false;
            if (edgeCandidate != null)
                Abort();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!active) return;

            edgeDragHelper.HandleMouseMove(e);
            edgeCandidate.candidatePosition = e.mousePosition;
            edgeCandidate.UpdateEdgeControl();
            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (!active || !CanStopManipulation(e))
                return;

            if (CanPerformConnection(e.localMousePosition))
                edgeDragHelper.HandleMouseUp(e);
            else
                Abort();

            active = false;
            edgeCandidate = null;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !active)
                return;

            Abort();

            active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        void Abort()
        {
            var graphView = target?.GetFirstAncestorOfType<GraphView>();
            graphView?.RemoveElement(edgeCandidate);

            edgeCandidate.input = null;
            edgeCandidate.output = null;
            edgeCandidate = null;

            edgeDragHelper.Reset();
        }

        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(mouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
        }
    }
}
