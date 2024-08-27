using MicroGraph.Runtime;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 总览图的分组
    /// </summary>
    internal class OverviewGroupView : Scope
    {
        private const string STYLE_PATH = "Uss/OverviewGraph/OverviewGroupView";
        private const int NODE_WIDTH = 180;
        public OverviewGraphView owner { get; }
        private Label title_label { get; }

        private GraphCategoryModel _category;

        private OverviewGroupInfo groupInfo;
        public override string title { get => title_label.text; set => title_label.text = value; }

        private IntegerField _columnField;
        public OverviewGroupView(OverviewGraphView view)
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Droppable | Capabilities.Movable;
            owner = view;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("overviewGroup");
            title_label = new Label("默认逻辑图");
            title_label.AddToClassList("overviewGroup_title");
            this.headerContainer.Add(title_label);
            _columnField = new IntegerField("列数");
            _columnField.AddToClassList("overviewGroup_column_input");
            _columnField.tooltip = "如果是小于等于0，则不会生效";
            this.headerContainer.Add(_columnField);
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        internal void Initialize(GraphCategoryModel categoryModel)
        {
            this._category = categoryModel;
            this.title = categoryModel.GraphName;
            this.headerContainer.style.backgroundColor = new StyleColor(new Color(categoryModel.GraphColor.r, categoryModel.GraphColor.g, categoryModel.GraphColor.b, 0.5f));
            this.headerContainer.style.borderRightColor = new StyleColor(categoryModel.GraphColor);
            this.headerContainer.style.borderLeftColor = new StyleColor(categoryModel.GraphColor);
            this.headerContainer.style.borderTopColor = new StyleColor(categoryModel.GraphColor);
            this.headerContainer.style.borderBottomColor = new StyleColor(categoryModel.GraphColor);
            this.style.borderRightColor = new StyleColor(categoryModel.GraphColor);
            this.style.borderLeftColor = new StyleColor(categoryModel.GraphColor);
            this.style.borderTopColor = new StyleColor(categoryModel.GraphColor);
            this.style.borderBottomColor = new StyleColor(categoryModel.GraphColor);
            this._columnField.style.borderLeftColor = new StyleColor(categoryModel.GraphColor);
            Color centralColor = new Color(categoryModel.GraphColor.r, categoryModel.GraphColor.g, categoryModel.GraphColor.b, 0.1f);
            this.contentContainer.style.backgroundColor = centralColor;
            string typeName = categoryModel.GraphType.FullName;
            var list = MicroGraphProvider.GraphSummaryList.Where(a => a.GraphClassName == typeName).ToList();
            foreach (GraphSummaryModel item in list)
            {
                var node = new OverviewNodeView();
                node.Initialize(this.owner, item);
                owner.AddElement(node);
                this.AddElement(node);
            }
            groupInfo = MicroGraphUtils.EditorConfig.OverviewConfig.GroupInfos.FirstOrDefault(a => a.groupKey == categoryModel.GraphType.FullName);
            if (groupInfo == null)
            {
                groupInfo = new OverviewGroupInfo();
                groupInfo.groupKey = categoryModel.GraphType.FullName;
                groupInfo.pos = new Vector2(-12, -48 + 200 * _category.Index);
                MicroGraphUtils.EditorConfig.OverviewConfig.GroupInfos.Add(groupInfo);
                MicroGraphUtils.SaveConfig();
            }
            _columnField.value = groupInfo.columnCount;
            _columnField.RegisterValueChangedCallback(a =>
            {
                groupInfo.columnCount = a.newValue;
                MicroGraphUtils.SaveConfig();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
            });
            this.SetPosition(new Rect(groupInfo.pos, Vector2.one));
            this.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
            //();
        }

        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            ResetElementPosition();
        }

        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"创建{title}", m_createMicroGraph, DropdownMenuAction.AlwaysEnabled);
            evt.StopImmediatePropagation();
        }

        private void m_createMicroGraph(DropdownMenuAction action)
        {
            BaseMicroGraph graph = MicroGraphUtils.CreateLogicGraph(_category);
            if (graph == null)
                return;
            MicroGraphWindow.ShowMicroGraph(owner.owner, graph.OnlyId);
        }

        /// <summary>
        /// 刷新总览视图
        /// </summary>
        /// <param name="eventArgs"></param>
        internal void Refresh()
        {
            this._columnField.SetValueWithoutNotify(groupInfo.columnCount);

            var nodeViewList = this.containedElements.OfType<OverviewNodeView>().ToList();
            foreach (var item in nodeViewList)
            {
                item.IsRefresh = false;
            }
            string typeName = _category.GraphType.FullName;
            var graphSummaryList = MicroGraphProvider.GraphSummaryList.Where(a => a.GraphClassName == typeName).ToList();
            foreach (var item in graphSummaryList)
            {
                var nodeView = nodeViewList.FirstOrDefault(a => a.SummaryModel == item);
                if (nodeView != null)
                {
                    nodeView.IsRefresh = true;
                    nodeView.Refresh();
                }
                else
                {
                    nodeView = new OverviewNodeView();
                    nodeView.IsRefresh = true;
                    nodeView.Initialize(this.owner, item);
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
        ///// <summary>
        ///// 仅仅刷新
        ///// </summary>
        //internal void Refresh()
        //{
        //    foreach (var item in this.containedElements)
        //    {
        //        if (item is OverviewNodeView nodeView)
        //        {
        //            nodeView.Refresh();
        //        }
        //    }
        //    ResetElementPosition();
        //}
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            groupInfo.pos = newPos.position;
        }

        internal void ResetElementPosition()
        {
            Vector2 startPosition = groupInfo.pos + new Vector2(24f, 47f);

            var nodeViews = this.containedElements.OfType<OverviewNodeView>().ToList();
            nodeViews.Sort((a, b) => b.SummaryModel.ModifyTime.CompareTo(a.SummaryModel.ModifyTime));

            bool isSingleRow = groupInfo.columnCount <= 0;
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
                int columnCount = groupInfo.columnCount;
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
