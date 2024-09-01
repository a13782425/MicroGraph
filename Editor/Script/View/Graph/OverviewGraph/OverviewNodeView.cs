using MicroGraph.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroGraphUtils;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 总览图上的节点
    /// </summary>
    internal sealed class OverviewNodeView : Node
    {
        private const string STYLE_PATH = "Uss/OverviewGraph/OverviewNodeView";
        private EditorLabelElement _titleElement;
        public new string title { get => _titleElement.text; set => _titleElement.text = value; }
        internal bool IsRefresh { get; set; }
        private Label _createTimeLabel;
        private Label _modifyTimeLabel;
        private EditorLabelElement _desLabel;
        public OverviewGraphView owner { get; private set; }

        private GraphSummaryModel _model;
        /// <summary>
        /// 图的简介
        /// </summary>
        public GraphSummaryModel SummaryModel => _model;
        /// <summary>
        /// 收藏信息
        /// </summary>
        private OverviewFavoriteGroupInfo _favoriteGroup = null;
        /// <summary>
        /// 收藏信息
        /// </summary>
        public OverviewFavoriteGroupInfo FavoriteGroup => _favoriteGroup;
        private VisualElement _contentContainer;
        /// <summary>
        /// 内容容器
        /// </summary>
        public override VisualElement contentContainer => _contentContainer;
        public OverviewNodeView()
        {
            this.capabilities = Capabilities.Selectable;
            this.AddToClassList("internalNodeView");
            this.AddStyleSheet(STYLE_PATH);
            _contentContainer = new VisualElement();
            _contentContainer.name = "nodeContainer";
            this.topContainer.parent.Add(contentContainer);
            this.topContainer.RemoveFromHierarchy();
        }
        internal void Initialize(OverviewGraphView graphView, GraphSummaryModel data = null, OverviewFavoriteGroupInfo groupInfo = null)
        {
            owner = graphView;
            _model = data;
            this._favoriteGroup = groupInfo;
            while (titleContainer.childCount > 0)
            {
                titleContainer[0].RemoveFromHierarchy();
            }
            OnInit();
        }
        private void OnInit()
        {
            var separator = new SeparatorElement(SeparatorDirection.Horizontal);
            separator.thickness = 4;
            separator.color = MicroGraphUtils.GetColor(_model.GraphClassName);

            contentContainer.Add(separator);
            _desLabel = new EditorLabelElement();
            _desLabel.text = "这里是描述";
            _desLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            _desLabel.style.color = new Color(137 / 255f, 137 / 255f, 137 / 255f);
            _desLabel.style.fontSize = 10;
            _desLabel.Q<Label>("title_label").style.whiteSpace = WhiteSpace.Normal;
            _desLabel.maxLength = 60;
            _desLabel.input_field.multiline = true;
            _desLabel.onRename += m_onDesRename;
            contentContainer.Add(_desLabel);
            _createTimeLabel = new Label();
            _createTimeLabel.AddToClassList("time_label");
            _modifyTimeLabel = new Label();
            _modifyTimeLabel.AddToClassList("time_label");
            contentContainer.Add(_createTimeLabel);
            contentContainer.Add(_modifyTimeLabel);

            _titleElement = new EditorLabelElement(_model.MicroName);
            _titleElement.onRename += m_onRename;
            this.titleContainer.Add(this._titleElement);

            Refresh();

            this.RegisterCallback<MouseDownEvent>(m_onClick);

        }
        internal void Refresh()
        {
            title = _model.MicroName;
            _desLabel.text = _model.Describe;
            _createTimeLabel.text = "创建时间:  " + MicroGraphUtils.FormatTime(_model.CreateTime);
            _modifyTimeLabel.text = "修改时间:  " + MicroGraphUtils.FormatTime(_model.ModifyTime);
        }
        private void m_onRename(string arg1, string arg2)
        {
            if (!MicroGraphUtils.TitleValidity(arg2, MicroGraphUtils.EditorConfig.GraphTitleLength))
            {
                owner.owner.ShowNotification(new GUIContent("标题不合法"), NOTIFICATION_TIME);
                title = arg1;
                return;
            }
            Runtime.BaseMicroGraph graph = MicroGraphUtils.GetMicroGraph(_model.AssetPath);
            if (graph != null)
            {
                graph.editorInfo.Title = arg2;
                _model.MicroName = arg2;
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                MicroGraphUtils.UnloadObject(graph);
            }
            else
                owner.owner.ShowNotification(new GUIContent("需要改名的逻辑图没有找到"), NOTIFICATION_TIME);
        }

        private void m_onDesRename(string arg1, string arg2)
        {
            var graph = MicroGraphUtils.GetMicroGraph(_model.AssetPath);
            if (graph != null)
            {
                graph.editorInfo.Describe = arg2;
                _model.Describe = arg2;
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                MicroGraphUtils.UnloadObject(graph);
            }
            else
                owner.owner.ShowNotification(new GUIContent("需要改名的逻辑图没有找到"), NOTIFICATION_TIME);
        }
        private void m_onClick(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                MicroGraphWindow.ShowMicroGraph(_model.OnlyId);
            }
        }
        private void m_openGraph(DropdownMenuAction action)
        {
            MicroGraphWindow.ShowMicroGraph(_model.OnlyId);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("打开", m_openGraph, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("定位", m_selectGraph, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendSeparator();
            if (this._favoriteGroup == null)
            {
                evt.menu.AppendSeparator();
                foreach (var item in MicroGraphUtils.EditorConfig.OverviewConfig.FavoriteGroupInfos)
                {
                    evt.menu.AppendAction($"添加收藏/{item.FavoriteName}", m_addFavoriteGroup, DropdownMenuAction.AlwaysEnabled, item);
                }
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("删除", m_removeGraph, DropdownMenuAction.AlwaysEnabled);
            }
            else
            {
                evt.menu.AppendAction("移除收藏", m_removeFavoriteGraph, DropdownMenuAction.AlwaysEnabled);
            }
            evt.StopImmediatePropagation();
        }

        private void m_addFavoriteGroup(DropdownMenuAction action)
        {
            OverviewFavoriteGroupInfo info = action.userData as OverviewFavoriteGroupInfo;
            if (info.Graphs.Contains(this.SummaryModel.OnlyId))
            {
                owner.owner.ShowNotification(new GUIContent($"收藏夹:『{info.FavoriteName}』中已存在 {SummaryModel.MicroName}"), NOTIFICATION_TIME);
                return;
            }
            info.Graphs.Add(this.SummaryModel.OnlyId);
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }

        private void m_removeFavoriteGraph(DropdownMenuAction action)
        {
            if (_favoriteGroup.Graphs.Contains(this.SummaryModel.OnlyId))
            {
                _favoriteGroup.Graphs.Remove(this.SummaryModel.OnlyId);
            }
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.OVERVIEW_CHANGED);
        }

        private void m_removeGraph(DropdownMenuAction action)
        {
            if (EditorUtility.DisplayDialog("警告", $"正在删除 {MicroGraphProvider.GetGraphCategory(SummaryModel).GraphName}: {SummaryModel.MicroName} ", "确定", "取消"))
            {
                MicroGraphUtils.RemoveGraph(SummaryModel.AssetPath);
            }
        }
        private void m_selectGraph(DropdownMenuAction action)
        {
            BaseMicroGraph graph = MicroGraphUtils.GetMicroGraph(_model.AssetPath);
            if (graph != null)
            {
                Selection.activeObject = graph;
                EditorGUIUtility.PingObject(graph);
                MicroGraphUtils.UnloadObject(graph);
            }
        }
    }
}
