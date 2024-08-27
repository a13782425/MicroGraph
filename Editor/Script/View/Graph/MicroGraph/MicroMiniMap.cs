using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal sealed class MicroMiniMap : MiniMap
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroMiniMap";
        private BaseMicroGraphView _owner;
        private Label _title;

        public MicroMiniMap(BaseMicroGraphView baseMicroGraphView)
        {
            _owner = baseMicroGraphView;
            this.RegisterCallback<AttachToPanelEvent>(m_attachPanel);
            this.RegisterCallback<GeometryChangedEvent>(m_geometryChangedCallback);
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_minimap");
            this.capabilities |= Capabilities.Resizable;
            base.hierarchy.Add(new ResizableElement());
            this.maxWidth = 64;
            this.maxHeight = 64;
            this.style.minWidth = 64;
            this.style.minHeight = 64;
            this.style.width = 64;
            this.style.height = 64;
            anchored = true;
            _title = this.Q<Label>();
            _title.text = "小地图";
            this.zoomFactorTextChanged += onTextChanged;
        }

        private void onTextChanged(string obj)
        {
            this._title.text = "小地图 " + obj;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!windowed)
            {
                evt.menu.AppendAction(anchored ? "浮动" : "固定", m_toggleAnchorState, DropdownMenuAction.AlwaysEnabled);
            }
            evt.menu.AppendAction("关闭小地图", m_closeMiniMap, DropdownMenuAction.AlwaysEnabled);
            evt.StopPropagation();
        }

        private void m_closeMiniMap(DropdownMenuAction action)
        {
            _owner.ShowMiniMap = false;
        }

        private void m_toggleAnchorState(DropdownMenuAction action)
        {
            this.anchored = !this.anchored;
        }

        private void m_geometryChangedCallback(GeometryChangedEvent evt)
        {
            this.maxWidth = this.layout.width;
            this.maxHeight = this.layout.height;
        }

        private void m_attachPanel(AttachToPanelEvent evt)
        {
            if (float.IsNaN(graphView.layout.width))
            {
                graphView.RegisterCallback<GeometryChangedEvent>(m_panelGeometryChanged);
                return;
            }
            m_setFirstPosition();
        }
        private void m_panelGeometryChanged(GeometryChangedEvent evt)
        {
            graphView.UnregisterCallback<GeometryChangedEvent>(m_panelGeometryChanged);
            m_setFirstPosition();
        }
        private void m_setFirstPosition()
        {
            float width = graphView.layout.width;
            width -= 68;
            this.SetPosition(new Rect(new Vector2(width, 0), new Vector2(64, 64)));
            // this.OnResized();
        }

    }
}
