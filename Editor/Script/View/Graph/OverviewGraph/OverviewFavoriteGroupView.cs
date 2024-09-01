using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 总览图的分组
    /// </summary>
    internal class OverviewFavoriteGroupView : Scope
    {
        private const string STYLE_PATH = "Uss/OverviewGraph/OverviewFavoriteGroupView";
        private const int NODE_WIDTH = 180;
        public OverviewGraphView owner { get; }
        private EditorLabelElement title_label { get; }

        internal bool IsRefresh { get; set; }

        private OverviewFavoriteGroupInfo _favoriteGroupInfo;
        public OverviewFavoriteGroupInfo favoriteGroupInfo => _favoriteGroupInfo;
        public override string title { get => title_label.text; set => title_label.text = value; }

        private IntegerField _columnField;
        private ColorField _colorField;
        public OverviewFavoriteGroupView(OverviewGraphView view)
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Droppable | Capabilities.Movable;
            owner = view;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("overviewGroup");
            Image image = new Image();
            image.AddToClassList("favorite_icon");
            this.headerContainer.Add(image);
            title_label = new EditorLabelElement("默认逻辑图");
            title_label.onRename += Title_label_onRename;
            title_label.AddToClassList("overviewGroup_title");
            this.headerContainer.Add(title_label);
            _columnField = new IntegerField("列数");
            _columnField.AddToClassList("overviewGroup_column_input");
            _columnField.tooltip = "如果是小于等于0，则不会生效";
            this.headerContainer.Add(_columnField);
            var colorContainer = new VisualElement();
            colorContainer.AddToClassList("overviewGroup_palette_container");
            _colorField = new ColorField();
            _colorField.AddToClassList("overviewGroup_palette");
            _colorField.RegisterCallback<ChangeEvent<Color>>(m_paletteChange);
            this.headerContainer.Add(colorContainer);
            colorContainer.Add(_colorField);
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }
        internal void Initialize(OverviewFavoriteGroupInfo favoriteGroupInfo)
        {
            this._favoriteGroupInfo = favoriteGroupInfo;
            this.title = favoriteGroupInfo.FavoriteName;
            refreshColor(favoriteGroupInfo.Color);
            Refresh();
            _columnField.value = favoriteGroupInfo.ColumnCount;
            _columnField.RegisterValueChangedCallback(a =>
            {
                favoriteGroupInfo.ColumnCount = a.newValue;
                MicroGraphUtils.SaveConfig();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
            });
            this.SetPosition(new Rect(favoriteGroupInfo.Pos, Vector2.one));
            this.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
        }
        private void Title_label_onRename(string oldName, string newName)
        {
            if (!MicroGraphUtils.TitleValidity(newName, MicroGraphUtils.EditorConfig.GroupTitleLength))
            {
                title = oldName;
                owner.owner.ShowNotification(new GUIContent("标题不合法"), 2f);
                return;
            }
            _favoriteGroupInfo.FavoriteName = newName;
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }
        private void m_paletteChange(ChangeEvent<Color> evt)
        {
            _favoriteGroupInfo.Color = evt.newValue;
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }

        private void refreshColor(Color color)
        {
            this._colorField.SetValueWithoutNotify(color);
            this.headerContainer.style.backgroundColor = new StyleColor(new Color(color.r, color.g, color.b, 0.5f));
            this.headerContainer.style.borderRightColor = new StyleColor(color);
            this.headerContainer.style.borderLeftColor = new StyleColor(color);
            this.headerContainer.style.borderTopColor = new StyleColor(color);
            this.headerContainer.style.borderBottomColor = new StyleColor(color);
            this.style.borderRightColor = new StyleColor(color);
            this.style.borderLeftColor = new StyleColor(color);
            this.style.borderTopColor = new StyleColor(color);
            this.style.borderBottomColor = new StyleColor(color);
            this._columnField.style.borderLeftColor = new StyleColor(color);
            Color centralColor = new Color(color.r, color.g, color.b, 0.1f);
            this.contentContainer.style.backgroundColor = centralColor;
        }

        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            ResetElementPosition();
        }

        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"删除{title}", m_deleteFavoriteGroup, DropdownMenuAction.AlwaysEnabled);
            evt.StopImmediatePropagation();
        }

        private void m_deleteFavoriteGroup(DropdownMenuAction action)
        {
            if (_favoriteGroupInfo.Graphs.Count > 0)
            {
                if (EditorUtility.DisplayDialog("提示", "当前收藏夹不为空,是否删除", "是", "否"))
                    this.owner.DeleteElements(this.containedElements);
                else
                    return;
            }

            MicroGraphUtils.EditorConfig.OverviewConfig.FavoriteGroupInfos.Remove(_favoriteGroupInfo);
            this.owner.DeleteElements(new[] { this });
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }

        /// <summary>
        /// 刷新总览视图
        /// </summary>
        /// <param name="eventArgs"></param>
        internal void Refresh()
        {
            this.title = favoriteGroupInfo.FavoriteName;
            this._columnField.SetValueWithoutNotify(_favoriteGroupInfo.ColumnCount);
            refreshColor(_favoriteGroupInfo.Color);
            var nodeViewList = this.containedElements.OfType<OverviewNodeView>().ToList();
            foreach (var item in nodeViewList)
            {
                item.IsRefresh = false;
            }
            for (int i = _favoriteGroupInfo.Graphs.Count - 1; i >= 0; i--)
            {
                string item = _favoriteGroupInfo.Graphs[i];
                var graphSummaryModel = MicroGraphProvider.GetGraphSummary(item);
                if (graphSummaryModel == null)
                {
                    _favoriteGroupInfo.Graphs.RemoveAt(i);
                    continue;
                }
                var nodeView = nodeViewList.FirstOrDefault(a => a.SummaryModel == graphSummaryModel);
                if (nodeView != null)
                {
                    nodeView.IsRefresh = true;
                    nodeView.Refresh();
                }
                else
                {
                    nodeView = new OverviewNodeView();
                    nodeView.IsRefresh = true;
                    nodeView.Initialize(this.owner, graphSummaryModel, _favoriteGroupInfo);
                    owner.AddElement(nodeView);
                    this.AddElement(nodeView);
                }
            }
            for (int i = nodeViewList.Count - 1; i >= 0; i--)
            {
                var item = nodeViewList[i];
                if (!item.IsRefresh)
                {
                    this.RemoveElements(new[] { item });
                    item.RemoveFromHierarchy();
                }
            }
            ResetElementPosition();
        }
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _favoriteGroupInfo.Pos = newPos.position;
        }

        internal void ResetElementPosition()
        {
            Vector2 startPosition = _favoriteGroupInfo.Pos + new Vector2(24f, 47f);

            var nodeViews = this.containedElements.OfType<OverviewNodeView>().ToList();
            nodeViews.Sort((a, b) => b.SummaryModel.ModifyTime.CompareTo(a.SummaryModel.ModifyTime));

            bool isSingleRow = _favoriteGroupInfo.ColumnCount <= 0;
            float horizontalOffset = 0; // 用于横向累加元素位置

            if (isSingleRow)
            {
                // 单行布局
                foreach (var nodeView in nodeViews)
                {
                    var newPosition = new Vector2(startPosition.x + horizontalOffset, startPosition.y);
                    nodeView.SetPosition(new Rect(newPosition, nodeView.layout.size));
                    horizontalOffset += nodeView.layout.width;
                }
            }
            else
            {
                // 多列布局
                int columnCount = _favoriteGroupInfo.ColumnCount;
                float[] columnWidths = new float[columnCount]; // 记录每列宽度
                float[] columnHeights = new float[columnCount]; // 记录每列高度
                for (int i = 0; i < nodeViews.Count; i++)
                {
                    var nodeView = nodeViews[i];
                    // 确定这个节点应放在哪一列
                    int columnIndex = i % columnCount;

                    // 为节点计算新位置
                    var newX = startPosition.x + Enumerable.Range(0, columnIndex).Sum(c => columnWidths[c]);
                    var newY = startPosition.y + columnHeights[columnIndex];
                    var newPosition = new Vector2(newX, newY);

                    // 更新列宽数组和列高数组
                    columnWidths[columnIndex] = Mathf.Max(columnWidths[columnIndex], nodeView.layout.width);
                    columnHeights[columnIndex] += nodeView.layout.height;

                    // 为节点设置新位置
                    nodeView.SetPosition(new Rect(newPosition, nodeView.layout.size));

                    // 我们只在循环结束后标记重绘，以便优化性能
                    if (i == nodeViews.Count - 1 || columnIndex == columnCount - 1)
                    {
                        nodeView.MarkDirtyRepaint();
                    }
                }
            }
        }
    }
}
