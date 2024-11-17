using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if !UNITY_2022_3_OR_NEWER
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal class MicroNodeInfoWindow : EditorWindow
    {
        private enum SortEnum
        {
            /// <summary>
            /// 名字正序
            /// </summary>
            NamePositive,
            /// <summary>
            /// 名字倒序
            /// </summary>
            NameNegative,
            /// <summary>
            /// 使用正序
            /// </summary>
            UsePositive,
            /// <summary>
            /// 使用倒序
            /// </summary>
            UseNegative
        }
        private SortEnum _sortEnum = SortEnum.NamePositive;
        private VisualElement contentContainer;
        private VisualElement titleContainer;
        private ListView nodeListView;
        private List<NodeTypeModel> nodeTypeModels = new List<NodeTypeModel>();
        private Dictionary<Type, NodeTypeModel> nodeTypeModelDics = new Dictionary<Type, NodeTypeModel>();
        private PopupField<GraphCategoryModel> _graphPopup;
        private TextField _searchField;
        private List<GraphCategoryModel> _graphList = new List<GraphCategoryModel>();
        /// <summary>
        /// 监听者
        /// </summary>
        private MicroGraphEventListener _listener;
        private void OnEnable()
        {
            this.minSize = new Vector2(420, 580);
            _listener = new MicroGraphEventListener();
            MicroGraphEventListener.RegisterListener(_listener);
            nodeTypeModels.Clear();
            nodeTypeModelDics.Clear();
            this.titleContent = new GUIContent("节点信息");
            contentContainer = new VisualElement();
            contentContainer.name = "contentContainer";
            contentContainer.AddToClassList("content_container");
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
            contentContainer.AddStyleSheet("Uss/MicroNodeInfoWindow");
            titleContainer = new VisualElement();
            titleContainer.AddToClassList("title_container");
            _graphPopup = new PopupField<GraphCategoryModel>(_graphList, -1, m_popupFormat, m_popupFormat);
            _graphPopup.label = "微图分类:";
            _graphPopup.AddToClassList("graph_popup");
            _searchField = new TextField();
            _searchField.label = "节点搜索:";
            _searchField.AddToClassList("node_search_input");
            titleContainer.Add(_graphPopup);
            VisualElement placeholder = new VisualElement();
            placeholder.AddToClassList("placeholder");
            titleContainer.Add(placeholder);
            titleContainer.Add(_searchField);
            nodeListView = new ListView(nodeTypeModels);
            nodeListView.AddToClassList("node_listview");
            this.contentContainer.Add(titleContainer);
            this.contentContainer.Add(nodeListView);
            this.contentContainer.AddManipulator(new ContextualMenuManipulator(onBuildContextualMenu));
            m_collectGraphs();
            m_drawUsageRate();
            _listener.AddListener(MicroGraphEventIds.EDITOR_SETTING_CHANGED, m_onEditorSettingChanged);
            _listener.AddListener(MicroGraphEventIds.EDITOR_PLAY_MODE_CHANGED, m_onEditorFontChanged);
            m_onEditorFontChanged(null);
            _graphPopup.RegisterValueChangedCallback(m_graphPopupChanged);
            _searchField.RegisterValueChangedCallback(m_searchFieldChanged);
            _graphPopup.index = 0;
            m_graphPopupChanged(ChangeEvent<GraphCategoryModel>.GetPooled());
        }

        private void OnDisable()
        {
            MicroGraphEventListener.UnregisterListener(_listener);
            //当游戏运行状态改变时会调用OnDisable和OnEnable
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
            return true;
        }
        private void m_sort()
        {
            switch (_sortEnum)
            {
                case SortEnum.NamePositive:
                    nodeTypeModels.Sort((a, b) => a.nodeCategory.NodeFullName.CompareTo(b.nodeCategory.NodeFullName));
                    break;
                case SortEnum.NameNegative:
                    nodeTypeModels.Sort((a, b) => b.nodeCategory.NodeFullName.CompareTo(a.nodeCategory.NodeFullName));
                    break;
                case SortEnum.UsePositive:
                    nodeTypeModels.Sort((a, b) => a.useCount.CompareTo(b.useCount));
                    break;
                case SortEnum.UseNegative:
                    nodeTypeModels.Sort((a, b) => b.useCount.CompareTo(a.useCount));
                    break;
            }
            nodeListView.Rebuild();
        }
        private bool m_onEditorFontChanged(object args)
        {
            this.contentContainer.style.unityFontDefinition = FontDefinition.FromFont(MicroGraphUtils.CurrentFont);
            return true;
        }
        private void onBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("节点名正序", (e) =>
            {
                _sortEnum = SortEnum.NamePositive;
                m_sort();
            },
            _sortEnum == SortEnum.NamePositive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("节点名倒序", (e) =>
            {
                _sortEnum = SortEnum.NameNegative;
                m_sort();
            },
            _sortEnum == SortEnum.NameNegative ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("使用量正序", (e) =>
            {
                _sortEnum = SortEnum.UsePositive;
                m_sort();
            },
            _sortEnum == SortEnum.UsePositive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("使用量倒序", (e) =>
            {
                _sortEnum = SortEnum.UseNegative;
                m_sort();
            },
            _sortEnum == SortEnum.UseNegative ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);


            evt.StopPropagation();
        }

        private void m_drawUsageRate()
        {
            nodeListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            nodeListView.horizontalScrollingEnabled = false;
            nodeListView.showFoldoutHeader = false;
            nodeListView.showAddRemoveFooter = false;
            nodeListView.makeItem += onMakeItem;
            nodeListView.bindItem += onBindItem;
            nodeListView.reorderable = false;
        }

        private void onBindItem(VisualElement element, int arg2)
        {
            if (element is NodeUsageElement usageElement)
            {
                usageElement.Refresh(arg2, nodeTypeModels[arg2]);
            }
        }

        private VisualElement onMakeItem()
        {
            return new NodeUsageElement(this);
        }
        private string m_popupFormat(GraphCategoryModel model)
        {
            return model == null ? "全部" : model.GraphName;
        }
        private void m_graphPopupChanged(ChangeEvent<GraphCategoryModel> evt)
        {
            _searchField.SetValueWithoutNotify("");
            if (evt.newValue == null)
            {
                m_collectNodes();
                m_collectUsageRate();
            }
            else
            {
                m_collectNodes(evt.newValue);
                m_collectUsageRate(evt.newValue.GraphType.Name);
            }
            m_sort();
        }
        private void m_searchFieldChanged(ChangeEvent<string> evt)
        {
            nodeTypeModels.Clear();
            string str = evt.newValue;
            if (string.IsNullOrWhiteSpace(str))
            {
                nodeTypeModels.AddRange(nodeTypeModelDics.Values);
            }
            else
            {
                nodeTypeModels.AddRange(
                    nodeTypeModelDics.Values.Where(
                        a => a.nodeCategory.NodeFullName.Contains(str, StringComparison.OrdinalIgnoreCase)
                        || a.nodeCategory.NodeClassType.FullName.Contains(str, StringComparison.OrdinalIgnoreCase)));
            }
            m_sort();
        }

        private void m_collectGraphs()
        {
            _graphList.Clear();
            _graphList.Add(null);
            _graphList.AddRange(MicroGraphProvider.GraphCategoryList);
        }
        private void m_collectUsageRate(string graphName = nameof(BaseMicroGraph))
        {
            string[] guids = AssetDatabase.FindAssets($"t: {graphName}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseMicroGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(assetPath);
                if (logicGraph == null)
                    continue;
                foreach (var item in logicGraph.Nodes)
                {
                    if (item == null)
                        continue;
                    if (nodeTypeModelDics.TryGetValue(item.GetType(), out NodeTypeModel typeModel))
                    {
                        typeModel.useCount++;
                        if (!typeModel.graphPaths.Contains(assetPath))
                        {
                            typeModel.graphPaths.Add(assetPath);
                        }
                    }
                }
                MicroGraphUtils.UnloadObject(logicGraph);
            }
            nodeTypeModels.Sort((a, b) => a.nodeCategory.NodeFullName.CompareTo(b.nodeCategory.NodeFullName));
        }

        private void m_collectNodes(GraphCategoryModel graphCategory = null)
        {
            nodeTypeModels.Clear();
            nodeTypeModelDics.Clear();
            List<NodeCategoryModel> types = new List<NodeCategoryModel>();
            if (graphCategory == null)
            {
                types.AddRange(MicroGraphProvider.NodeCategoryMapping.Values);
            }
            else
            {
                BaseMicroGraphView graphView = (BaseMicroGraphView)Activator.CreateInstance(graphCategory.ViewType);
                MicroGraphProvider.InitGraphCategory(graphView, graphCategory);
                types.AddRange(graphCategory.NodeCategories);
                graphView = null;
            }

            foreach (var item in types)
            {
                if (item.EnableState == MicroNodeEnableState.Exclude)
                    continue;
                NodeTypeModel model = new NodeTypeModel();
                model.nodeCategory = item;
                nodeTypeModels.Add(model);
                nodeTypeModelDics.Add(item.NodeClassType, model);
            }
        }

        private class NodeUsageElement : VisualElement
        {
            private static Texture2D foldout_out;
            private static Texture2D foldout_in;
            static NodeUsageElement()
            {
                foldout_out = (Texture2D)EditorGUIUtility.IconContent("d_IN_foldout").image;
                foldout_in = (Texture2D)EditorGUIUtility.IconContent("d_IN_foldout_on").image;
            }

            private VisualElement arrowElement;
            private Label nameLabel;
            private Toggle isEnableToggle;
            private Label countLabel;
            private VisualElement normalContent;
            private VisualElement usageContent;
            private List<Label> labels = new List<Label>();
            private NodeTypeModel model;
            private MicroNodeInfoWindow window;
            private PopupWindowContent _curPopupWindow;
            private int index;
            public NodeUsageElement(MicroNodeInfoWindow window)
            {
                this.window = window;
                this.AddToClassList("node_usage_element");
                normalContent = new VisualElement();
                normalContent.AddToClassList("normal_content");
                normalContent.AddManipulator(new Clickable(m_titleClick));
                normalContent.AddManipulator(new ContextualMenuManipulator(onNormalContextualMenu));
                arrowElement = new VisualElement();
                arrowElement.AddToClassList("usage_arrow");
                nameLabel = new Label();
                nameLabel.AddToClassList("name_label");
                isEnableToggle = new Toggle();
                isEnableToggle.AddToClassList("enable_label");
                countLabel = new Label();
                countLabel.AddToClassList("count_label");
                usageContent = new VisualElement();
                usageContent.AddToClassList("usage_content");
                normalContent.Add(arrowElement);
                normalContent.Add(nameLabel);
                normalContent.Add(isEnableToggle);
                normalContent.Add(countLabel);
                this.Add(normalContent);
                this.Add(usageContent);
                isEnableToggle.SetEnabled(false);
            }

            private void onNormalContextualMenu(ContextualMenuPopulateEvent evt)
            {
                evt.menu.AppendAction("打开代码", m_openCode, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
            }

            private void m_openCode(DropdownMenuAction action)
            {
                if (model == null)
                    return;
                string[] guids = AssetDatabase.FindAssets(model.nodeCategory.NodeClassType.Name);
                foreach (var item in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(item);
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (monoScript != null && monoScript.GetClass() == model.nodeCategory.NodeClassType)
                    {
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                        break;
                    }
                }
            }

            private void m_titleClick(EventBase evt)
            {
                if (model == null)
                    return;
                model.isFold = !model.isFold;
                arrowElement.style.backgroundImage = new StyleBackground(model.isFold ? foldout_in : foldout_out);
                this.window.nodeListView.selectedIndex = this.index;
                m_showGraphPath();
            }

            public void Refresh(int index, NodeTypeModel model)
            {
                this.index = index;
                this.model = model;
                nameLabel.text = "节点名: " + model.nodeCategory.NodeFullName;
                isEnableToggle.value = model.nodeCategory.EnableState == MicroNodeEnableState.Enabled;
                countLabel.text = "使用量: " + model.useCount.ToString();
                m_showGraphPath();
            }
            private void m_showGraphPath()
            {
                foreach (var item in usageContent.Children())
                {
                    item.SetDisplay(false);
                }
                arrowElement.style.backgroundImage = new StyleBackground(model.isFold ? foldout_in : foldout_out);
                if (!model.isFold)
                    return;
                int count = 0;
                if (model.graphPaths.Count == 0)
                {
                    Label lab = m_getLabel(count);
                    lab.userData = null;
                    lab.text = "无";
                    return;
                }
                foreach (var item in model.graphPaths)
                {
                    Label lab = m_getLabel(count);
                    lab.userData = item;
                    count++;
                    lab.text = item;
                }
            }

            private Label m_getLabel(int index)
            {
                Label lab = null;
                if (usageContent.childCount > index)
                {
                    lab = (Label)usageContent[index];
                    lab.SetDisplay(true);
                }
                else
                {
                    lab = new Label();
                    lab.AddToClassList("graph_path_label");
                    Color color = MicroGraphUtils.GetColor(model.nodeCategory.NodeClassType.Name);
                    lab.style.borderTopColor = color;
                    lab.style.borderLeftColor = color;
                    lab.style.borderBottomColor = color;
                    lab.focusable = true;
                    lab.AddManipulator(new Clickable(onLabelClick));
                    lab.AddManipulator(new ContextualMenuManipulator(onLabelContextualMenu));
                    this.usageContent.Add(lab);
                }
                return lab;
            }


            private void onLabelClick(EventBase evt)
            {
                if (evt.target is not Label label)
                    return;
                if (label.text == "无")
                    return;
                if (label.userData is not string path)
                    return;
                BaseMicroGraph graph = MicroGraphUtils.GetMicroGraph(path);
                if (graph == null)
                    return;
                _curPopupWindow = MicroNodeInfoPopup.ShowNodeInfoPopup(this.window, graph, model.nodeCategory, label);
            }
            private void onLabelContextualMenu(ContextualMenuPopulateEvent evt)
            {
                if (evt.target is not Label label)
                    return;
                if (label.text == "无")
                    return;
                evt.menu.AppendAction("定位微图", m_location, DropdownMenuAction.AlwaysEnabled, evt.target);
                evt.menu.AppendAction("打开微图", m_openGraph, DropdownMenuAction.AlwaysEnabled, evt.target);
                evt.menu.AppendSeparator();
            }

            private void m_openGraph(DropdownMenuAction action)
            {
                if (action.userData is not Label label)
                    return;
                if (label.userData is not string path)
                    return;
                MicroGraphUtils.OpenMicroGraph(path);
            }

            private void m_location(DropdownMenuAction action)
            {
                if (action.userData is not Label label)
                    return;
                if (label.userData is not string path)
                    return;

                BaseMicroGraph graph = MicroGraphUtils.GetMicroGraph(path);
                if (graph == null)
                    return;
                Selection.activeObject = graph;
                EditorGUIUtility.PingObject(graph);
                MicroGraphUtils.UnloadObject(graph);
            }
        }

        private class NodeTypeModel
        {
            public NodeCategoryModel nodeCategory;
            public int useCount;
            public HashSet<string> graphPaths = new HashSet<string>();
            /// <summary>
            /// 折叠
            /// </summary>
            public bool isFold = false;
            public float Height => isFold ? 32 : (graphPaths.Count + 1) * 32;
        }
    }
}
