using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 总览图
    /// </summary>
    internal sealed class OverviewGraphView : GraphView
    {
        private const string STYLE_PATH = "Uss/OverviewGraph/OverviewGraphView";
        private List<OverviewGroupView> _groups = new List<OverviewGroupView>();
        private List<OverviewFavoriteGroupView> _favoriteGroups = new List<OverviewFavoriteGroupView>();
        private OverviewCreateGraphWindow _createGraphWindow;
        private OverviewSearchView _searchView;
        internal MicroGraphWindow owner { get; private set; }
        private bool _showGrid = false;
        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid == value)
                    return;
                _showGrid = value;
                if (value)
                    this.m_showGrid();
                else
                    this.m_hideGrid();
            }
        }

        private ContentZoomer _contentZoomer = new ContentZoomer();
        private bool _canZoom = false;
        /// <summary>
        /// 是否可以缩放
        /// </summary>
        public bool CanZoom
        {
            get => _canZoom;
            set
            {
                if (_canZoom == value)
                    return;
                _canZoom = value;
                if (value)
                    this.AddManipulator(_contentZoomer);
                else
                {
                    this.RemoveManipulator(_contentZoomer);
                }
            }
        }
        /// <summary>
        /// 缩放最小值
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomMinScale { get => _contentZoomer.minScale; set => _contentZoomer.minScale = value; }
        /// <summary>
        /// 缩放最大值
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomMaxScale { get => _contentZoomer.maxScale; set => _contentZoomer.maxScale = value; }
        /// <summary>
        /// 单次缩放步长
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomScaleStep { get => _contentZoomer.scaleStep; set => _contentZoomer.scaleStep = value; }
        public OverviewGraphView()
        {

        }
        public void Initialize(MicroGraphWindow window)
        {
            owner = window;
            OnInit();
        }
        public void Show()
        {
            owner.listener.AddListener(MicroGraphEventIds.GRAPH_ASSETS_CHANGED, m_onLogicAssetsChanged);
            owner.listener.AddListener(MicroGraphEventIds.OVERVIEW_CHANGED, m_onOverviewChanged);
            this.style.display = DisplayStyle.Flex;
            owner.topToolbarView.onGUI -= m_onTopToolbarGUI;
            owner.topToolbarView.onGUI += m_onTopToolbarGUI;
            owner.bottomToolbarView.onGUI -= m_onBottomToolbarGUI;
            owner.bottomToolbarView.onGUI += m_onBottomToolbarGUI;
            m_onOverviewChanged(null);
        }

        public void Hide()
        {
            owner.listener.RemoveListener(MicroGraphEventIds.GRAPH_ASSETS_CHANGED, m_onLogicAssetsChanged);
            owner.listener.RemoveListener(MicroGraphEventIds.OVERVIEW_CHANGED, m_onOverviewChanged);
            this.style.display = DisplayStyle.None;
            owner.topToolbarView.onGUI -= m_onTopToolbarGUI;
            owner.bottomToolbarView.onGUI -= m_onBottomToolbarGUI;
        }
        internal void Exit()
        {
            owner.listener.RemoveListener(MicroGraphEventIds.GRAPH_ASSETS_CHANGED, m_onLogicAssetsChanged);
            owner.listener.RemoveListener(MicroGraphEventIds.OVERVIEW_CHANGED, m_onOverviewChanged);
        }
        public void OnUpdate()
        {

        }
        private void OnInit()
        {
            Input.imeCompositionMode = IMECompositionMode.On;
            this.AddStyleSheet(STYLE_PATH);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            m_addGroup();
            this.ZoomMaxScale = 2;
            ShowGrid = MicroGraphUtils.EditorConfig.OverviewConfig.ShowGrid;
            CanZoom = MicroGraphUtils.EditorConfig.OverviewConfig.CanZoom;
            this.viewTransformChanged += m_viewTransformChanged;
            this.graphViewChanged += m_graphViewChanged;
            this.RegisterCallback<KeyDownEvent>(onKeyDownEvent);
            //this.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
            float scale = MicroGraphUtils.EditorConfig.OverviewConfig.Scale;
            Vector2 pos = MicroGraphUtils.EditorConfig.OverviewConfig.Pos;
            this.UpdateViewTransform(pos, new Vector2(scale, scale));
            this.StretchToParentSize();
        }


        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            if (this.style.display != DisplayStyle.Flex)
                return;
            foreach (var item in _groups)
            {
                item.ResetElementPosition();
            }
        }

        private void onKeyDownEvent(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    this.ClearSelection();
                    evt.StopPropagation();
                    break;
                case KeyCode.Delete:
                    List<OverviewNodeView> tempList = this.selection.OfType<OverviewNodeView>().ToList();
                    bool needRefresh = false;
                    foreach (var item in tempList)
                    {
                        if (item.FavoriteGroup == null)
                        {
                            if (EditorUtility.DisplayDialog("警告", $"正在删除 {MicroGraphProvider.GetGraphCategory(item.SummaryModel).GraphName}: {item.SummaryModel.MicroName} ", "确定", "取消"))
                            {
                                MicroGraphUtils.RemoveGraph(item.SummaryModel.AssetPath);
                                needRefresh = true;
                            }
                        }
                        else
                        {
                            if (item.FavoriteGroup.Graphs.Contains(item.SummaryModel.OnlyId))
                            {
                                item.FavoriteGroup.Graphs.Remove(item.SummaryModel.OnlyId);
                                needRefresh = true;
                            }
                        }
                    }
                    if (needRefresh)
                    {
                        MicroGraphUtils.SaveConfig();
                        MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
                    }
                    evt.StopPropagation();
                    break;
                case KeyCode.S:
                    if (evt.ctrlKey)
                    {
                        MicroGraphUtils.SaveConfig();
                        owner.ShowNotification(new GUIContent("保存配置文件成功"), 2);
                        evt.StopPropagation();
                    }
                    break;
                case KeyCode.F:
                    if (evt.ctrlKey)
                    {
                        if (_searchView == null)
                        {
                            _searchView = new OverviewSearchView(this);
                            Add(_searchView);
                        }
                        _searchView.Search();
                        evt.StopPropagation();
                    }
                    break;
            }
        }

        private void m_viewTransformChanged(GraphView graphView)
        {
            MicroGraphUtils.EditorConfig.OverviewConfig.Scale = graphView.scale;
            MicroGraphUtils.EditorConfig.OverviewConfig.Pos = graphView.viewTransform.position;
        }
        private GraphViewChange m_graphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null && graphViewChange.movedElements.Count > 0)
            {
                MicroGraphUtils.SaveConfig();
            }
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                foreach (var item in graphViewChange.elementsToRemove)
                {
                    if (item is OverviewFavoriteGroupView groupView)
                    {
                        _favoriteGroups.Remove(groupView);
                    }
                }
            }
            return graphViewChange;
        }
        private bool m_onOverviewChanged(object args)
        {
            foreach (var item in _groups)
            {
                item.Refresh();
            }
            foreach (var item in _favoriteGroups)
            {
                item.IsRefresh = false;
            }
            foreach (var item in MicroGraphUtils.EditorConfig.OverviewConfig.FavoriteGroupInfos)
            {
                var group = _favoriteGroups.FirstOrDefault(a => a.favoriteGroupInfo == item);
                if (group != null)
                {
                    group.IsRefresh = true;
                    group.Refresh();
                }
                else
                {
                    group = new OverviewFavoriteGroupView(this);
                    group.IsRefresh = true;
                    group.Initialize(item);
                    this.AddElement(group);
                    _favoriteGroups.Add(group);
                }
            }
            for (int i = _favoriteGroups.Count - 1; i >= 0; i--)
            {
                var item = _favoriteGroups[i];
                if (!item.IsRefresh)
                    this.DeleteElements(item.containedElements.Union(new[] { item }));
            }
            return true;
        }

        private bool m_onLogicAssetsChanged(object args)
        {
            GraphAssetsChangedEventArgs eventArg = args as GraphAssetsChangedEventArgs;
            foreach (var item in _groups)
            {
                item.Refresh();
            }
            foreach (var item in _favoriteGroups)
            {
                item.Refresh();
            }
            return true;
        }

        /// <summary>
        /// 添加分组
        /// </summary>
        private void m_addGroup()
        {
            foreach (var item in MicroGraphProvider.GraphCategoryList)
            {
                var group = new OverviewGroupView(this);
                group.Initialize(item);
                this.AddElement(group);
                _groups.Add(group);
            }
            foreach (var item in MicroGraphUtils.EditorConfig.OverviewConfig.FavoriteGroupInfos)
            {
                var group = new OverviewFavoriteGroupView(this);
                group.Initialize(item);
                this.AddElement(group);
                _favoriteGroups.Add(group);
            }
        }
        private void m_onTopToolbarGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            bool value = GUILayout.Toggle(ShowGrid, "网格", "ButtonMid", GUILayout.MaxWidth(48));
            if (value != ShowGrid)
            {
                ShowGrid = value;
                MicroGraphUtils.EditorConfig.OverviewConfig.ShowGrid = value;
            }
            GUILayout.Space(2);
            value = GUILayout.Toggle(CanZoom, "缩放", "ButtonMid", GUILayout.MaxWidth(48));
            if (value != CanZoom)
            {
                CanZoom = value;
                MicroGraphUtils.EditorConfig.OverviewConfig.CanZoom = value;
            }
            if (CanZoom)
            {

                float scale = EditorGUILayout.Slider(MicroGraphUtils.EditorConfig.OverviewConfig.Scale, this.ZoomMinScale, this.ZoomMaxScale, GUILayout.MaxWidth(128));
                if (scale != this.scale)
                {
                    Vector3 position = this.viewTransform.position;
                    this.UpdateViewTransform(position, new Vector3(scale, scale, 1));
                }
            }
            GUILayout.EndHorizontal();
        }
        private void m_onBottomToolbarGUI()
        {

        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var item in MicroGraphProvider.GraphCategoryList)
            {
                evt.menu.AppendAction("创建" + item.GraphName, m_onCreateGraphClick, DropdownMenuAction.AlwaysEnabled, item);
            }
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("创建收藏夹", m_onCreateFavoriteClick, DropdownMenuAction.AlwaysEnabled);
        }

        /// <summary>
        /// 创建收藏夹
        /// </summary>
        /// <param name="action"></param>
        private void m_onCreateFavoriteClick(DropdownMenuAction action)
        {
            Vector2 screenPos = owner.GetScreenPosition(action.eventInfo.mousePosition);
            //经过计算得出节点的位置
            var windowMousePosition = owner.rootVisualElement.ChangeCoordinatesTo(owner.rootVisualElement.parent, screenPos - owner.position.position);
            var groupPos = this.contentViewContainer.WorldToLocal(windowMousePosition);
            OverviewFavoriteGroupInfo overviewFavoriteGroupInfo = new OverviewFavoriteGroupInfo();
            overviewFavoriteGroupInfo.FavoriteName = "收藏";
            overviewFavoriteGroupInfo.Pos = groupPos;
            overviewFavoriteGroupInfo.ColumnCount = 0;
            overviewFavoriteGroupInfo.Color = MicroGraphUtils.GetRandomColor();
            MicroGraphUtils.EditorConfig.OverviewConfig.FavoriteGroupInfos.Add(overviewFavoriteGroupInfo);
            MicroGraphUtils.SaveConfig();
            var group = new OverviewFavoriteGroupView(this);
            group.Initialize(overviewFavoriteGroupInfo);
            this.AddElement(group);
            _favoriteGroups.Add(group);
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }
        private void m_onCreateGraphClick(DropdownMenuAction obj)
        {
            BaseMicroGraph graph = MicroGraphUtils.CreateLogicGraph(obj.userData as GraphCategoryModel);
            if (graph == null)
                return;
            MicroGraphWindow.ShowMicroGraph(owner, graph.OnlyId);
        }

        private bool m_onCreateGraphSelectEntry(SearchTreeEntry arg1, SearchWindowContext arg2)
        {
            GraphCategoryModel configData = arg1.userData as GraphCategoryModel;
            string path = EditorUtility.SaveFilePanel("创建逻辑图", Application.dataPath, "MicroGraph", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return false;
            }
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
                return false;
            }
            path = FileUtil.GetProjectRelativePath(path);
            BaseMicroGraph graph = MicroGraphUtils.CreateLogicGraph(configData.GraphType, path);
            MicroGraphWindow.ShowMicroGraph(graph.OnlyId);
            return true;
        }
        /// <summary>
        /// 显示背景网格
        /// </summary>
        internal void m_showGrid()
        {
            //添加网格背景
            GridBackground gridBackground = new InternalGraphGrid();
            gridBackground.name = "___GridBackground";
            Insert(0, gridBackground);
        }
        /// <summary>
        /// 隐藏Grid
        /// </summary>
        internal void m_hideGrid()
        {
            GridBackground gridBackground = this.Q<GridBackground>("___GridBackground");
            if (gridBackground != null)
            {
                gridBackground.RemoveFromHierarchy();
            }
        }

        private class InternalGraphGrid : GridBackground { }
    }
}
