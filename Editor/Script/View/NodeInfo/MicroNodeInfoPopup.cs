using MicroGraph.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityPopupWindow = UnityEditor.PopupWindow;

namespace MicroGraph.Editor
{
    internal static class MicroNodeInfoPopup
    {
        internal static bool NodeInfoPopupShow = false;
        private class WindowContent : PopupWindowContent
        {
            private const string STYLE_PATH = "Uss/NodeInfo/MicroNodeInfoPopup";
            private Label _numberLabel;
            private ScrollView _scrollView;
            private VisualElement _scrollViewContainer;
            private Button _openButton;
            //private Label _emptyLabel;
            private BaseMicroGraph microGraph;
            private NodeCategoryModel nodeCategory;
            private VisualElement _selectVe;
            private Color _selectColor;
            public int nodeCount;
            public override void OnOpen()
            {
                if (EditorGUIUtility.isProSkin)
                {
                    //黑暗主题
                    editorWindow.rootVisualElement.AddStyleSheet("Uss/DarkTheme");
                }
                else
                {
                    //明亮主题
                    editorWindow.rootVisualElement.AddStyleSheet("Uss/LightTheme");
                }
                editorWindow.rootVisualElement.style.unityFontDefinition = FontDefinition.FromFont(MicroGraphUtils.CurrentFont);
                editorWindow.rootVisualElement.AddStyleSheet(STYLE_PATH);
                editorWindow.rootVisualElement.AddToClassList("micro_nodeinfo_popup");
                editorWindow.rootVisualElement.focusable = true;
                _numberLabel = new Label();
                _numberLabel.text = $"当前总量: {nodeCount}";
                _numberLabel.AddToClassList("micro_nodeinfo_num_label");
                _numberLabel.style.borderBottomColor = MicroGraphUtils.GetColor(nodeCategory.NodeClassType.Name);
                //_emptyLabel = new Label();
                //_emptyLabel.text = "空";
                //_emptyLabel.AddToClassList("micro_nodeinfo_empty_label");
                //_emptyLabel.SetDisplay(false);
                editorWindow.rootVisualElement.Add(_numberLabel);
                _scrollView = new ScrollView(ScrollViewMode.Vertical);
                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
                _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                _scrollView.AddToClassList("micro_nodeinfo_scrollview");
                editorWindow.rootVisualElement.Add(_scrollView);
                _scrollViewContainer = new VisualElement();
                _scrollViewContainer.AddToClassList("micro_nodeinfo_scrollview_container");
                _scrollView.Add(_scrollViewContainer);
                _openButton = new Button(m_openClick);
                _openButton.text = "打开微图";
                _openButton.AddToClassList("micro_nodeinfo_open_button");
                editorWindow.rootVisualElement.Add(_openButton);

                var nodeList = microGraph.Nodes.Where(a => a.GetType() == nodeCategory.NodeClassType);
                foreach (var node in nodeList)
                {
                    NodeInfoVisualElement nodeInfoVisual = new NodeInfoVisualElement();
                    MicroNodeEditorInfo nodeEditor = microGraph.editorInfo.Nodes.FirstOrDefault(a => a.NodeId == node.OnlyId);
                    nodeInfoVisual.Initialize(node, nodeEditor, nodeCategory);
                    _scrollViewContainer.Add(nodeInfoVisual);
                }
                _selectVe.SetPseudoStates(_selectVe.GetPseudoStates() | CustomPseudoStates.Checked);
                _selectColor = _selectVe.style.backgroundColor.value;
                Color color = MicroGraphUtils.GetColor(nodeCategory.NodeClassType.Name);
                color.a = 0.5f;
                _selectVe.style.backgroundColor = color;
            }

            private void m_openClick()
            {
                string graphPath = AssetDatabase.GetAssetPath(microGraph);
                editorWindow.Close();
                MicroGraphUtils.OpenMicroGraph(graphPath);
            }

            public override void OnClose()
            {
                _selectVe.SetPseudoStates(_selectVe.GetPseudoStates() & (~CustomPseudoStates.Checked));
                _selectVe.style.backgroundColor = _selectColor;
                MicroGraphUtils.UnloadObject(microGraph);
                NodeInfoPopupShow = false;
            }

            public override void OnGUI(Rect rect)
            {
            }
            public override Vector2 GetWindowSize()
            {
                return new Vector2(nodeCount > 2 ? 296 : 284, 320);
            }

            public void Show(Rect rect, BaseMicroGraph graph, NodeCategoryModel nodeCategory, VisualElement selectVe)
            {
                this.microGraph = graph;
                this.nodeCategory = nodeCategory;
                this._selectVe = selectVe;
                UnityPopupWindow.Show(rect, this);
            }
        }
        private class NodeInfoVisualElement : VisualElement
        {
            public new class UxmlFactory : UxmlFactory<NodeInfoVisualElement>
            {
            }
            private Label _nodeTitleLabel;
            private Label _varTitleLabel;
            private VisualElement _varContainer;
            private Label _varEdgeTitleLabel;
            private VisualElement _varEdgeContainer;
            private ScrollView _nodeContainer;
            public NodeInfoVisualElement()
            {
                this.AddToClassList("nodeinfo_visual");
                _nodeContainer = new ScrollView(ScrollViewMode.Vertical);
                _nodeContainer.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                _nodeContainer.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                _nodeContainer.AddToClassList("nodeinfo_container");
                _nodeTitleLabel = new Label();
                _nodeTitleLabel.AddToClassList("nodeinfo_title_label");

                _varTitleLabel = new Label("变量信息");
                _varContainer = new VisualElement();
                _varEdgeTitleLabel = new Label("变量连线");
                _varEdgeContainer = new VisualElement();
                _varTitleLabel.AddToClassList("nodeinfo_title_label");
                _varContainer.AddToClassList("nodeinfo_var_container");
                _varEdgeTitleLabel.AddToClassList("nodeinfo_title_label");
                _varEdgeContainer.AddToClassList("nodeinfo_var_container");
                _varContainer.SetEnabled(false);
                _varEdgeContainer.SetEnabled(false);
                _nodeContainer.Add(_nodeTitleLabel);
                _nodeContainer.Add(_varTitleLabel);
                _nodeContainer.Add(_varContainer);
                _nodeContainer.Add(_varEdgeTitleLabel);
                _nodeContainer.Add(_varEdgeContainer);
                this.Add(_nodeContainer);
                this.RegisterCallback<WheelEvent>(e => e.StopPropagation());
            }

            public void Initialize(BaseMicroNode node, MicroNodeEditorInfo nodeEditor, NodeCategoryModel nodeCategory)
            {
                _nodeTitleLabel.text = "节点标题：" + nodeEditor.Title;
                _varContainer.Clear();
                _varEdgeContainer.Clear();
                Label idLabel = new Label($"唯一Id: {node.OnlyId}");
                idLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _varContainer.Add(idLabel);
                foreach (KeyValuePair<string, FieldInfo> item in nodeCategory.GetNodeFieldInfos())
                {
                    VisualElement subContainer = new VisualElement();
                    subContainer.AddToClassList("nodeinfo_var_sub_container");
                    FieldInfo fieldInfo = item.Value;
                    if (fieldInfo.FieldIsInput())
                    {
                        if (node.VariableEdges.FirstOrDefault(a => a.fieldName == fieldInfo.Name && a.isInput) != null)
                        {
                            subContainer.Add(new Label($"{fieldInfo.GetFieldDisplayName()}: 端口已连线"));
                            _varContainer.Add(subContainer);
                            continue;
                        }
                    }
                    Label label = new Label();
                    if (fieldInfo.FieldType.IsGenericType)
                    {
                        if (fieldInfo.FieldType.IsArray)
                        {
                            label.text = $"{fieldInfo.GetFieldDisplayName()}: 数组类型";
                        }
                        else if (IsDictionary(fieldInfo.FieldType))
                        {
                            label.text = $"{fieldInfo.GetFieldDisplayName()}: 字典类型";
                        }
                        else if (IsCollection(fieldInfo.FieldType))
                        {
                            label.text = $"{fieldInfo.GetFieldDisplayName()}: 集合类型";
                        }
                        else
                        {
                            label.text = $"{fieldInfo.GetFieldDisplayName()}: 其他泛型";
                        }
                    }
                    else
                    {
                        object value = fieldInfo.GetValue(node);
                        label.text = $"{fieldInfo.GetFieldDisplayName()}: {(value == null ? "空" : value.ToString())}";
                    }
                    subContainer.Add(label);
                    _varContainer.Add(subContainer);
                }
                if (node.VariableEdges.Count == 0)
                {
                    _varEdgeContainer.Add(new Label($"没有连线"));
                }
                else
                {
                    foreach (var item in node.VariableEdges)
                    {
                        VisualElement subContent = new VisualElement();
                        subContent.AddToClassList("nodeinfo_var_sub_container");
                        subContent.Add(new Label($"是否是入端口：" + (item.isInput ? "是" : "否")));
                        subContent.Add(new Label($"变量名：" + item.varName));
                        subContent.Add(new Label($"字段名：" + item.fieldName));
                        _varEdgeContainer.Add(subContent);
                    }
                }
            }
            // 检查指定类型是否是字典类型
            private static bool IsDictionary(Type type)
            {
                bool isIDictionary = typeof(IDictionary).IsAssignableFrom(type);
                bool isGenericDict = type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

                return isIDictionary || isGenericDict;
            }

            // 检查指定类型是否是集合类型
            private static bool IsCollection(Type type)
            {
                bool isICollection = typeof(ICollection).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type);
                bool isGenericEnumerable = typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
                bool hasGenericInterface = type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return isICollection || isGenericEnumerable || hasGenericInterface;
            }
        }

        internal static PopupWindowContent ShowNodeInfoPopup(MicroNodeInfoWindow window, BaseMicroGraph graph, NodeCategoryModel nodeCategory, VisualElement selectVe)
        {
            WindowContent windowContent = new WindowContent();
            windowContent.nodeCount = graph.Nodes.Count(a => a.GetType() == nodeCategory.NodeClassType);
            Rect rect = window.rootVisualElement.worldBound;
            rect.x += window.position.width;
            rect.y -= 320;
            rect.width = windowContent.nodeCount > 2 ? 296 : 284;
            rect.height = 320;
            windowContent.Show(rect, graph, nodeCategory, selectVe);
            return windowContent;
        }
    }
}
