using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphOrder(MicroVariableControlSubView.VARIABLE_CONTROL_ORDER + 300)]
    internal class MicroNodeControlSubView : VisualElement, IMicroSubControl
    {
        public VisualElement Panel => this;
        public string Name => "节点信息";
        private BaseMicroGraphView _owner;
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroNodeControlSubView";
        private Label _warningLabel;
        private ScrollView _nodeContainer;
        private Label _nodeTitleLabel;
        private Label _nodeClassLabel;
        private Label _varTitleLabel;
        private VisualElement _varContainer;
        private Label _varEdgeTitleLabel;
        private VisualElement _varEdgeContainer;
        public MicroNodeControlSubView(BaseMicroGraphView owner)
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micronodecontrol");
            this._owner = owner;
            _warningLabel = new Label();
            _warningLabel.AddToClassList("warning_label");
            _warningLabel.text = "暂无选中节点";
            this.Add(_warningLabel);
            _nodeContainer = new ScrollView(ScrollViewMode.Vertical);
            _nodeContainer.AddToClassList("nodecontainer");
            _nodeTitleLabel = new Label();
            _nodeClassLabel = new Label();
            _varTitleLabel = new Label();
            _varContainer = new VisualElement();
            _varEdgeTitleLabel = new Label();
            _varEdgeContainer = new VisualElement();
            _nodeTitleLabel.AddToClassList("title_label");
            _nodeClassLabel.AddToClassList("class_label");
            _varTitleLabel.AddToClassList("title_label");
            _varContainer.AddToClassList("var_container");
            _varEdgeTitleLabel.AddToClassList("title_label");
            _varEdgeContainer.AddToClassList("var_container");
            _varContainer.SetEnabled(false);
            _varEdgeContainer.SetEnabled(false);
            _nodeContainer.Add(_nodeTitleLabel);
            _nodeContainer.Add(_nodeClassLabel);
            _nodeContainer.Add(_varTitleLabel);
            _nodeContainer.Add(_varContainer);
            _nodeContainer.Add(_varEdgeTitleLabel);
            _nodeContainer.Add(_varEdgeContainer);
            this.Add(_nodeContainer);
            owner.onSelectChanged += m_onSelectChanged;
        }

        private void m_onSelectChanged(List<ISelectable> list)
        {
            _warningLabel.SetDisplay(true);
            _nodeContainer.SetDisplay(false);
            if (list.Count == 0)
            {
                _warningLabel.text = "请选中节点";
            }
            else if (list.Count == 1)
            {
                if (list[0] is Node node)
                {
                    ShowNodeInfo(node);
                }
                else
                {
                    _warningLabel.text = "请选中节点";
                }
            }
            else
            {
                _warningLabel.text = "当前选中节点过多";
            }
        }
        public void ShowNodeInfo(Node node)
        {
            if (node is MicroVariableNodeView.InternalNodeView)
            {
                _warningLabel.text = "当前选中为变量节点";
                return;
            }
            if (node is not BaseMicroNodeView.InternalNodeView nodeView)
            {
                _warningLabel.text = "请选中节点";
                return;
            }
            _warningLabel.SetDisplay(false);
            _nodeContainer.SetDisplay(true);
            _nodeTitleLabel.text = "节点标题: " + nodeView.nodeView.Title;
            _nodeClassLabel.text = "节点类名: " + nodeView.nodeView.Target.GetType().FullName;
            _varContainer.Clear();
            _varEdgeContainer.Clear();
            _varTitleLabel.text = "变量信息";
            _varEdgeTitleLabel.text = "变量连线";
            Label idLabel = new Label($"唯一Id: {nodeView.nodeView.Target.OnlyId}");
            idLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _varContainer.Add(idLabel);
            foreach (KeyValuePair<string, FieldInfo> item in nodeView.nodeView.category.GetNodeFieldInfos())
            {
                VisualElement subContainer = new VisualElement();
                subContainer.AddToClassList("var_sub_container");
                FieldInfo fieldInfo = item.Value;
                if (fieldInfo.FieldIsInput())
                {
                    if (nodeView.nodeView.Target.VariableEdges.FirstOrDefault(a => a.fieldName == fieldInfo.Name && a.isInput) != null)
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
                    object value = fieldInfo.GetValue(nodeView.nodeView.Target);
                    label.text = $"{fieldInfo.GetFieldDisplayName()}: {(value == null ? "空" : value.ToString())}";
                }
                subContainer.Add(label);
                _varContainer.Add(subContainer);
            }
            if (nodeView.nodeView.Target.VariableEdges.Count == 0)
            {
                _varEdgeContainer.Add(new Label($"没有连线"));
            }
            else
            {
                foreach (var item in nodeView.nodeView.Target.VariableEdges)
                {
                    VisualElement subContainer = new VisualElement();
                    subContainer.AddToClassList("var_sub_container");
                    subContainer.Add(new Label($"是否是入端口：" + (item.isInput ? "是" : "否")));
                    subContainer.Add(new Label($"变量名：" + item.varName));
                    subContainer.Add(new Label($"字段名：" + item.fieldName));
                    _varEdgeContainer.Add(subContainer);
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
        public void Show()
        {
        }

        public void Hide()
        {
        }
        public void Exit()
        {
        }
    }
}
