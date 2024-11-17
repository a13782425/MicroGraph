using MicroGraph.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 包节点视图
    /// </summary>
    [MicroGraphEditor(typeof(MicroPackageNode))]
    internal partial class MicroPackageNodeView : BaseMicroNodeView
    {
        private readonly static Color NODE_COLOR = MicroGraphUtils.GetColor(typeof(MicroNodeSerializeModel));
        private readonly static Color VAR_NODE_COLOR = MicroGraphUtils.GetColor(typeof(MicroVarNodeSerializeModel).Name);
        private static Vector3[] s_cachedRect = new Vector3[4];
        private readonly static Vector2 NODE_SIZE = new Vector2(160, 160);
        private readonly static Vector2 VAR_SIZE = new Vector2(160, 80);
        private IMGUIContainer _container;
        private Image _icon;
        /// <summary>
        /// 收藏所有元素的矩形
        /// 没有缩放
        /// </summary>
        private Rect _templateRect;
        private Rect _contentRect = new Rect(4, 4, 128, 128);

        private List<int> _allItems = new List<int>();
        private MicroPackageNode _node;
        protected override void onInit()
        {
            base.onInit();
            _node = (MicroPackageNode)Target;
            var groupEditor = owner.editorInfo.Groups.FirstOrDefault(a => a.GroupId == _node.PackageId);
            if (groupEditor != null)
            {
                _allItems.AddRange(groupEditor.Nodes);
            }
            nodeIcon.sprite = Resources.Load<Sprite>("__MicroGraph/Texture/Common/package");
            owner.listener.AddListener(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, m_packageChanged);

        }

        protected override void buildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            int selectCount = this.owner.View.selection.OfType<Node>().Count();
            if (selectCount > 1)
            {
                evt.StopPropagation();
                return;
            }
            evt.menu.AppendAction("前往当前包", m_frameGroup, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendSeparator();


            string msg = editorInfo.TitleColor < 0 ? "默认颜色" : ((NodeTitleColorType)editorInfo.TitleColor).ToString();
            evt.menu.AppendAction($"当前颜色: {msg}", null, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("修改标题颜色/默认颜色/当前节点", (a) => TitleColor = (NodeTitleColorType)a.userData, DropdownMenuAction.AlwaysEnabled, (NodeTitleColorType)(-1));
            evt.menu.AppendAction("修改标题颜色/默认颜色/同类型节点", a => m_changeAllTitleColor((NodeTitleColorType)a.userData), DropdownMenuAction.AlwaysEnabled, (NodeTitleColorType)(-1));
            foreach (var item in nodeColorTypes)
            {
                string menu = "修改标题颜色/";
                menu += item.ToString();
                evt.menu.AppendAction(menu + "/当前节点", (a) => TitleColor = (NodeTitleColorType)a.userData, DropdownMenuAction.AlwaysEnabled, item);
                evt.menu.AppendAction(menu + "/同类型节点", a => m_changeAllTitleColor((NodeTitleColorType)a.userData), DropdownMenuAction.AlwaysEnabled, item);
            }


            evt.menu.AppendAction("删除", onDeleteNodeView, this.owner.CategoryModel.IsUniqueNode(this.Target.GetType()) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.StopPropagation();
        }
        /// <summary>
        /// 修改当前类型节点标题颜色
        /// </summary>
        /// <param name="action"></param>
        private void m_changeAllTitleColor(NodeTitleColorType colorType)
        {
            foreach (var item in owner.View.nodes.OfType<BaseMicroNodeView>())
            {
                if (item.Target.GetType() == this.Target.GetType())
                    item.TitleColor = colorType;
            }
        }
        private void m_frameGroup(DropdownMenuAction action)
        {
            owner.View.ClearSelection();
            MicroGroupView groupView = owner.GetElement<MicroGroupView>(this._node.PackageId);
            owner.View.AddToSelection(groupView);
            owner.View.FrameSelection();
        }

        /// <summary>
        /// 检测包ID
        /// </summary>
        private void m_checkPackageId()
        {
            if (_node.PackageId == 0)
            {
                //间隔500毫秒后并没有设置包ID，则认为是无效的节点，删除
                owner.RemoveNode(_node.OnlyId);
            }
            else
            {
                m_refresh();
                if (Title == category.NodeName)
                {
                    var groupEditor = owner.editorInfo.Groups.FirstOrDefault(a => a.GroupId == _node.PackageId);
                    if (groupEditor != null)
                    {
                        Title = "包: " + groupEditor.Title;
                    }
                }
            }
        }

        private bool m_packageChanged(object args)
        {
            if (args is MicroPackageInfo packageNode)
            {
                if (packageNode.PackageId == _node.PackageId)
                {
                    view.schedule.Execute(m_refresh).ExecuteLater(100);
                }
            }
            return true;
        }

        protected override void onExit()
        {
            owner.listener.RemoveListener(MicroGraphEventIds.GRAPH_PACKAGE_CHANGED, m_packageChanged);
            base.onExit();
        }
        protected internal override void DrawUI()
        {
            DrawUI("showLog");
            _container = new IMGUIContainer(DrawTemplateContent);
            _container.style.width = 136;
            _container.style.height = 136;
            _container.style.alignSelf = Align.Center;
            _icon = new Image();
            _icon.sprite = Resources.Load<Sprite>("__MicroGraph/Texture/Common/package");
            _icon.tintColor = new Color(1, 1, 1, 0.3f);
            _icon.style.width = 32;
            _icon.style.height = 32;
            _icon.style.position = Position.Absolute;
            _icon.style.alignSelf = Align.FlexEnd;
            _icon.style.top = new StyleLength(StyleKeyword.Auto);
            _icon.style.left = new StyleLength(StyleKeyword.Auto);
            _icon.style.right = 2;
            _icon.style.bottom = 2;
            this.Add(_icon);
            this.Add(_container);
            this.view.schedule.Execute(m_checkPackageId).ExecuteLater(100);
        }

        private void m_refresh()
        {
            _allItems.Clear();

            var groupEditor = owner.editorInfo.Groups.FirstOrDefault(a => a.GroupId == _node.PackageId);
            if (groupEditor != null)
            {
                _allItems.AddRange(groupEditor.Nodes);
            }
            var packageInfo = owner.Target.Packages.FirstOrDefault(a => a.PackageId == _node.PackageId);
            if (packageInfo != null)
            {
                if (packageInfo.StartNodes.Count > 0)
                {
                    Input.view.SetDisplay(true);
                }
                else
                {
                    Input.view.SetDisplay(false);
                    owner.View.DeleteElements(Input.view.connections);
                }
                if (packageInfo.EndNodes.Count > 0)
                {
                    OutPut.view.SetDisplay(true);
                }
                else
                {
                    OutPut.view.SetDisplay(false);
                    owner.View.DeleteElements(OutPut.view.connections);
                }
            }
        }
        private void DrawTemplateContent()
        {
            Color color = Handles.color;
            m_calculateTemplateRect();
            m_drawElements();
            Handles.color = color;

        }
        private void m_drawElements()
        {
            float xFactor = _contentRect.width / _templateRect.width;
            float yFactor = _contentRect.height / _templateRect.height;
            float factor = 0;
            float xOffset = 0;
            float yOffset = 0;
            if (xFactor > yFactor)
            {
                factor = yFactor;
                xOffset = _contentRect.width - _contentRect.width / (xFactor / yFactor);
                xOffset *= 0.5f;
            }
            else
            {
                factor = xFactor;
                yOffset = _contentRect.height - _contentRect.height / (yFactor / xFactor);
                yOffset *= 0.5f;
            }
            Rect rect;
            foreach (var item in _allItems)
            {
                GraphElement graphelement = owner.GetElement<GraphElement>(item);
                switch (graphelement)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        rect = m_calculateElementRect(nodeView.nodeView.editorInfo.Pos, NODE_SIZE, factor, xOffset, yOffset);
                        s_cachedRect[0].Set(rect.xMin, rect.yMin, 0f);
                        s_cachedRect[1].Set(rect.xMax, rect.yMin, 0f);
                        s_cachedRect[2].Set(rect.xMax, rect.yMax, 0f);
                        s_cachedRect[3].Set(rect.xMin, rect.yMax, 0f);
                        Handles.DrawSolidRectangleWithOutline(s_cachedRect, NODE_COLOR.Fade(0.3f), NODE_COLOR);
                        break;
                    case MicroVariableNodeView.InternalNodeView varNodeView:
                        rect = m_calculateElementRect(varNodeView.nodeView.editorInfo.Pos, VAR_SIZE, factor, xOffset, yOffset);
                        s_cachedRect[0].Set(rect.xMin, rect.yMin, 0f);
                        s_cachedRect[1].Set(rect.xMax, rect.yMin, 0f);
                        s_cachedRect[2].Set(rect.xMax, rect.yMax, 0f);
                        s_cachedRect[3].Set(rect.xMin, rect.yMax, 0f);
                        Handles.DrawSolidRectangleWithOutline(s_cachedRect, VAR_NODE_COLOR.Fade(0.3f), VAR_NODE_COLOR);
                        break;
                    default:
                        break;
                }
            }
        }

        private Rect m_calculateElementRect(Vector2 pos, Vector2 size, float factor, float xOffset, float yOffset)
        {
            Vector2 min = pos;
            min.y -= size.y;
            Vector2 max = pos;
            max.x += size.x;
            Rect rect = new Rect();
            rect.min = min - _templateRect.min;
            rect.max = rect.min + size;
            rect.min = rect.min * factor;
            rect.max = rect.max * factor;

            rect.xMin += _contentRect.x + xOffset;
            rect.yMin += _contentRect.y + yOffset;
            rect.xMax += _contentRect.x + xOffset;
            rect.yMax += _contentRect.y + yOffset;
            return rect;
        }

        private void m_calculateTemplateRect()
        {
            _templateRect = new Rect();
            _templateRect.xMax = float.MinValue;
            _templateRect.yMax = float.MinValue;
            _templateRect.xMin = float.MaxValue;
            _templateRect.yMin = float.MaxValue;

            foreach (var item in _allItems)
            {
                GraphElement graphelement = owner.GetElement<GraphElement>(item);
                switch (graphelement)
                {
                    case BaseMicroNodeView.InternalNodeView nodeView:
                        m_calculate(nodeView.nodeView.editorInfo.Pos, NODE_SIZE);
                        break;
                    case MicroVariableNodeView.InternalNodeView varNodeView:
                        m_calculate(varNodeView.nodeView.editorInfo.Pos, VAR_SIZE);
                        break;
                    default:
                        break;
                }
            }
            void m_calculate(Vector2 pos, Vector2 size)
            {
                Vector2 min = pos;
                min.y -= size.y;
                Vector2 max = pos;
                max.x += size.x;
                if (_templateRect.min.x > min.x)
                    _templateRect.xMin = min.x;
                if (_templateRect.min.y > min.y)
                    _templateRect.yMin = min.y;

                if (_templateRect.max.x < max.x)
                    _templateRect.xMax = max.x;
                if (_templateRect.max.y < max.y)
                    _templateRect.yMax = max.y;
            }
        }
    }
}
