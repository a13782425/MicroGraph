using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 逻辑图窗口
    /// </summary>
    internal sealed partial class MicroGraphWindow : EditorWindow
    {
        private const string STATE_OVERVIEW = "Overview";
        private const string STATE_GRAPH = "Graph";
        private const string STATE_SETTING = "Setting";

        private GraphOperateModel _operateModel = new GraphOperateModel();
        internal GraphOperateModel operateModel => _operateModel;
        private VisualElement _topToolbarContent;//顶部工具栏容器，在上
        private VisualElement _sidebarContent;//侧边栏容器，在左边
        private VisualElement _graphContent;//视图容器，在右边
        private VisualElement _bottomToolbarContent;//视图容器，在下
        private MicroSettingView _settingView;//设置界面

        private FlyoutMenuView _sidebarView;

        private VisualElement contentContainer;

        public FlyoutButton overviewButton { get; private set; }
        public FlyoutButton loadButton { get; private set; }
        public FlyoutButton graphButton { get; private set; }
        public FlyoutButton saveButton { get; private set; }
        public FlyoutButton settingButton { get; private set; }

        internal ToolbarView topToolbarView { get; private set; }
        internal ToolbarView bottomToolbarView { get; private set; }
        /// <summary>
        /// 总览的视图
        /// </summary>
        internal OverviewGraphView overviewView { get; private set; }
        internal BaseMicroGraphView graphView { get; private set; }

        /// <summary>
        /// 监听者
        /// </summary>
        internal MicroGraphEventListener listener { get; private set; }

        private string _curState = STATE_OVERVIEW;

        /// <summary>
        /// 逻辑图唯一Id
        /// </summary>
        private string _graphId = null;
        /// <summary>
        /// 当前显示逻辑图的唯一ID
        /// </summary>
        public string graphId
        {
            get => _graphId;
            private set
            {
                _graphId = value;
                _operateModel.Refresh(_graphId);
            }
        }

        internal static MicroGraphWindow OpenWindow()
        {
            MicroGraphWindow panel = m_getWindow() ?? CreateWindow<MicroGraphWindow>();
            panel.titleContent = new GUIContent("微图");
            panel.minSize = MicroGraphUtils.MIN_SIZE;
            panel.Focus();
            return panel;
        }
    }
    partial class MicroGraphWindow
    {
        /// <summary>
        /// 显示一个微图
        /// </summary>
        /// <param name="onlyId">图的唯一Id</param>
        public static void ShowMicroGraph(string onlyId)
        {
            MicroGraphWindow window = m_getWindow(onlyId);
            if (window != null)
            {
                //当前逻辑图有被打开的情况
                window.m_focus();
                return;
            }
            //当前逻辑图没有被打开
            window = OpenWindow();
            window.graphId = onlyId;
            window.m_showLogicGraph();
        }
        /// <summary>
        /// 指定一个窗口显示一个微图
        /// 如果这个窗口没有显示微图
        /// </summary>
        /// <param name="window"></param>
        /// <param name="onlyId">图的唯一Id</param>
        public static void ShowMicroGraph(MicroGraphWindow window, string onlyId)
        {
            if (string.IsNullOrWhiteSpace(window.graphId))
            {
                window.graphId = onlyId;
                window.m_showLogicGraph();
            }
            else
            {
                ShowMicroGraph(onlyId);
            }
        }
    }

    // 私有方法
    partial class MicroGraphWindow
    {
        private static MicroGraphWindow m_getWindow(string onlyId = null)
        {
            bool isNull = string.IsNullOrWhiteSpace(onlyId);
            Object[] panels = Resources.FindObjectsOfTypeAll(typeof(MicroGraphWindow));
            MicroGraphWindow panel = null;
            foreach (var item in panels)
            {
                if (item is MicroGraphWindow p)
                {
                    if (isNull)
                    {
                        if (string.IsNullOrWhiteSpace(p.graphId))
                        {
                            panel = p;
                            break;
                        }
                    }
                    else
                    {
                        if (p.graphId == onlyId)
                        {
                            panel = p;
                            break;
                        }
                    }
                }
            }
            return panel;
        }

        private void m_showLogicGraph(bool click = true)
        {
            if (!string.IsNullOrWhiteSpace(this.graphId))
            {
                if (this.operateModel.summaryModel == null)
                {
                    this.Close();
                }
                else
                {
                    if (graphView != null)
                        graphView.Exit();
                    //删除没有的节点
                    this.operateModel.microGraph.Nodes.RemoveAll(n => n == null);
                    this.operateModel.microGraph.Initialize();
                    graphView = Activator.CreateInstance(this.operateModel.categoryModel.ViewType) as BaseMicroGraphView;
                    graphView.Initialize(this);
                    graphButton.SetDisplay(true);
                    saveButton.SetDisplay(true);
                    _graphContent.Add(graphView);
                    if (click)
                        m_onGraphClick(null);
                }
            }
        }
        private void m_focus()
        {
            this.Focus();
            m_onGraphClick(null);
        }
    }

    //生命周期
    partial class MicroGraphWindow
    {

        /// <summary>
        /// 相当于构造函数
        /// 但会在每次编译后执行
        /// </summary>
        private void OnEnable()
        {
            listener = new MicroGraphEventListener();
            MicroGraphEventListener.RegisterListener(listener);
            titleContent = new GUIContent("微图");
            contentContainer = new VisualElement();
            contentContainer.name = "contentContainer";
            this.rootVisualElement.Add(contentContainer);
            if (EditorGUIUtility.isProSkin)
            {
                //黑暗主题
                contentContainer.AddStyleSheet("Uss/DarkTheme");
            }
            else
            {
                //明亮主题
                contentContainer.AddStyleSheet("Uss/LightTheme");
            }
            //contentContainer.AddStyleSheet("Uss/Tailwind");
            contentContainer.AddStyleSheet("Uss/MicroGraphWindow");
            /*
             __________________________________
             | |______________________________|
             | |                              |
             | |                              |
             | |                              |
             | |                              |
             | |______________________________|
             |_|______________________________|
             */

            _sidebarContent = new VisualElement();
            _sidebarContent.name = "sidebarContent";
            contentContainer.Add(_sidebarContent);
            _sidebarView = new FlyoutMenuView();
            _sidebarView.title = "微图";
            _sidebarContent.Add(_sidebarView);

            var rightContent = new VisualElement();
            rightContent.name = "rightContent";
            contentContainer.Add(rightContent);

            _topToolbarContent = new VisualElement();
            _topToolbarContent.name = "topToolbarContent";
            rightContent.Add(_topToolbarContent);


            _graphContent = new VisualElement();
            _graphContent.name = "graphContent";
            rightContent.Add(_graphContent);

            _bottomToolbarContent = new VisualElement();
            _bottomToolbarContent.name = "bottomToolbarContent";
            rightContent.Add(_bottomToolbarContent);
            topToolbarView = new ToolbarView();
            _topToolbarContent.Add(topToolbarView);
            bottomToolbarView = new ToolbarView();
            _bottomToolbarContent.Add(bottomToolbarView);
            //初始化总览图
            overviewView = new OverviewGraphView();
            overviewView.Initialize(this);
            _graphContent.Add(overviewView);

            _settingView = new MicroSettingView(this);
            _settingView.SetDisplay(false);
            _graphContent.Add(_settingView);

            m_addButtons();
            listener.AddListener(MicroGraphEventIds.EDITOR_SETTING_CHANGED, m_onEditorSettingChanged);
            listener.AddListener(MicroGraphEventIds.GRAPH_ASSETS_CHANGED, m_onGraphAssetsChanged);
            listener.AddListener(MicroGraphEventIds.EDITOR_PLAY_MODE_CHANGED, m_onEditorFontChanged);
            m_restoreWindow();
        }
        private void m_restoreWindow()
        {
            if (!string.IsNullOrWhiteSpace(_graphId))
            {
                this.graphId = _graphId;
                m_showLogicGraph(false);
            }
            switch (_curState)
            {
                case STATE_OVERVIEW:
                    m_onOverviewClick(null);
                    break;
                case STATE_GRAPH:
                    m_onGraphClick(null);
                    break;
                case STATE_SETTING:
                    m_onSettingClick(null);
                    break;
            }
            m_onEditorFontChanged(null);
        }

        private void Update()
        {
            overviewView?.OnUpdate();
            graphView?.onUpdate();
        }

        private void OnDisable()
        {
            //当游戏运行状态改变时会调用OnDisable和OnEnable
            MicroGraphEventListener.UnregisterListener(listener);
        }
        private void OnDestroy()
        {
            if (graphView != null)
            {
                graphView.Save();
                graphView.Exit();
                MicroGraphUtils.UnloadObject(graphView.Target);
            }
            overviewView?.Exit();
            MicroGraphUtils.SaveConfig();
        }
        private bool m_onGraphAssetsChanged(object args)
        {
            if (graphView == null)
                return true;
            GraphAssetsChangedEventArgs graphAssetsChanged = args as GraphAssetsChangedEventArgs;
            bool canClose = false;
            foreach (var item in graphAssetsChanged.deletedGraphs)
            {
                if (graphView.SummaryModel.AssetPath == item)
                {
                    canClose = true;
                    break;
                }
            }
            if (canClose)
            {
                graphId = null;
                graphButton.SetDisplay(false);
                saveButton.SetDisplay(false);
                graphView.View.RemoveFromHierarchy();
                graphView.Exit();
                graphView = null;
                if (_curState == STATE_GRAPH)
                {
                    m_onOverviewClick(null);
                }
            }
            return true;
        }
        private bool m_onEditorSettingChanged(object args)
        {
            if (args is string str)
            {
                switch (str)
                {
                    case nameof(MicroGraphUtils.EditorConfig.EditorFont):
                        m_onEditorFontChanged(null);
                        break;
                    default:
                        break;
                }
            }
            if (_curState == STATE_SETTING)
            {
                _settingView.Show();
            }
            return true;
        }
        private bool m_onEditorFontChanged(object args)
        {
            this.contentContainer.style.unityFontDefinition = FontDefinition.FromFont(MicroGraphUtils.CurrentFont);
            return true;
        }
        /// <summary>
        /// 添加侧边栏的button
        /// </summary>
        private void m_addButtons()
        {
            overviewButton = _sidebarView.AddButton("总览");
            overviewButton.icon = MicroGraphUtils.LoadRes<Texture>("Texture/Flyout/flyout_all");

            loadButton = _sidebarView.AddButton("打开");
            loadButton.icon = MicroGraphUtils.LoadRes<Texture>("Texture/Flyout/flyout_load");

            graphButton = _sidebarView.AddButton("图");
            graphButton.icon = MicroGraphUtils.LoadRes<Texture>("Texture/Flyout/flyout_graph");

            saveButton = _sidebarView.AddButton("保存");
            saveButton.icon = MicroGraphUtils.LoadRes<Texture>("Texture/Flyout/flyout_save");

            settingButton = _sidebarView.AddButton("设置");
            settingButton.icon = MicroGraphUtils.LoadRes<Texture>("Texture/Flyout/flyout_setting");
            overviewButton.onClick += m_onOverviewClick;
            loadButton.onClick += m_onLoadClick;
            graphButton.onClick += m_onGraphClick;
            saveButton.onClick += m_onSaveClick;
            settingButton.onClick += m_onSettingClick;
            graphButton.SetDisplay(false);
            saveButton.SetDisplay(false);
        }
        private void m_onLoadClick(VisualElement obj)
        {
            string path = EditorUtility.OpenFilePanelWithFilters("打开微图", Application.dataPath, new[] { "BaseMicroGraph", "asset" });
            var graph = MicroGraphUtils.GetMicroGraph(FileUtil.GetProjectRelativePath(path));
            //var graph = LogicUtils.LoadGraph<BaseLogicGraph>(FileUtil.GetProjectRelativePath(path));
            if (graph != null)
            {
                this.graphId = graph.OnlyId;
                m_showLogicGraph();
            }
        }
        private void m_onOverviewClick(VisualElement evt)
        {
            _curState = STATE_OVERVIEW;
            overviewView?.Show();
            graphView?.Hide();
            _settingView?.Hide();
            overviewButton.Select();
            graphButton.UnSelect();
            settingButton.UnSelect();
        }
        private void m_onGraphClick(VisualElement obj)
        {
            _curState = STATE_GRAPH;
            overviewView?.Hide();
            graphView?.Show();
            _settingView?.Hide();
            graphButton.Select();
            overviewButton.UnSelect();
            settingButton.UnSelect();
        }
        private void m_onSaveClick(VisualElement obj)
        {
            if (!string.IsNullOrWhiteSpace(this.graphId))
            {
                graphView?.Save();
            }
        }

        private void m_onSettingClick(VisualElement obj)
        {
            _curState = STATE_SETTING;
            overviewView?.Hide();
            graphView?.Hide();
            _settingView?.Show();
            settingButton.Select();
            overviewButton.UnSelect();
            graphButton.UnSelect();
        }
    }
}
