using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal class MicroNodeControlSubView : VisualElement, IMicroSubControl
    {
        private BaseMicroGraphView _owner;
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroNodeControlSubView";
        private Label _warningLabel;
        private ScrollView _nodeContent;
        private Label _nodeTitleLabel;
        private Label _nodeClassLabel;
        private Label _varTitleLabel;
        private VisualElement _varContent;
        private Label _varEdgeTitleLabel;
        private VisualElement _varEdgeContent;
        public MicroNodeControlSubView(BaseMicroGraphView owner)
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micronodecontrol");
            this._owner = owner;
            _warningLabel = new Label();
            _warningLabel.AddToClassList("warning_label");
            _warningLabel.text = "暂无选中节点";
            this.Add(_warningLabel);
            _nodeContent = new ScrollView(ScrollViewMode.Vertical);
            _nodeContent.AddToClassList("nodecontent");
            _nodeTitleLabel = new Label();
            _nodeClassLabel = new Label();
            _varTitleLabel = new Label();
            _varContent = new VisualElement();
            _varEdgeTitleLabel = new Label();
            _varEdgeContent = new VisualElement();
            _nodeTitleLabel.AddToClassList("title_label");
            _nodeClassLabel.AddToClassList("class_label");
            _varTitleLabel.AddToClassList("title_label");
            _varContent.AddToClassList("var_content");
            _varEdgeTitleLabel.AddToClassList("title_label");
            _varEdgeContent.AddToClassList("var_content");
            _varContent.SetEnabled(false);
            _varEdgeContent.SetEnabled(false);
            _nodeContent.Add(_nodeTitleLabel);
            _nodeContent.Add(_nodeClassLabel);
            _nodeContent.Add(_varTitleLabel);
            _nodeContent.Add(_varContent);
            _nodeContent.Add(_varEdgeTitleLabel);
            _nodeContent.Add(_varEdgeContent);
            this.Add(_nodeContent);
            owner.onSelectChanged += m_onSelectChanged;
        }

        private void m_onSelectChanged(List<ISelectable> list)
        {
            _warningLabel.SetDisplay(true);
            _nodeContent.SetDisplay(false);
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
            _nodeContent.SetDisplay(true);
            _nodeTitleLabel.text = "节点标题: " + nodeView.nodeView.Title;
            _nodeClassLabel.text = "节点类名: " + nodeView.nodeView.Target.GetType().FullName;
            _varContent.Clear();
            _varEdgeContent.Clear();
            _varTitleLabel.text = "变量信息";
            _varEdgeTitleLabel.text = "变量连线";
            Label idLabel = new Label($"唯一Id: {nodeView.nodeView.Target.OnlyId}");
            idLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _varContent.Add(idLabel);
            foreach (KeyValuePair<string, FieldInfo> item in nodeView.nodeView.category.GetNodeFieldInfos())
            {
                VisualElement subContent = new VisualElement();
                subContent.AddToClassList("var_sub_content");
                FieldInfo fieldInfo = item.Value;
                if (fieldInfo.FieldIsInput())
                {
                    if (nodeView.nodeView.Target.VariableEdges.FirstOrDefault(a => a.fieldName == fieldInfo.Name && a.isInput) != null)
                    {
                        subContent.Add(new Label($"{fieldInfo.GetFieldDisplayName()}: 端口已连线"));
                        _varContent.Add(subContent);
                        continue;
                    }
                }
                Label label = new Label();
                object value = fieldInfo.GetValue(nodeView.nodeView.Target);
                label.text = $"{fieldInfo.GetFieldDisplayName()}: {(value == null ? "空" : value.ToString())}";
                subContent.Add(label);
                _varContent.Add(subContent);
            }
            if (nodeView.nodeView.Target.VariableEdges.Count == 0)
            {
                _varEdgeContent.Add(new Label($"没有连线"));
            }
            else
            {
                foreach (var item in nodeView.nodeView.Target.VariableEdges)
                {
                    VisualElement subContent = new VisualElement();
                    subContent.AddToClassList("var_sub_content");
                    subContent.Add(new Label($"是否是入端口：" + (item.isInput ? "是" : "否")));
                    subContent.Add(new Label($"变量名：" + item.varName));
                    subContent.Add(new Label($"字段名：" + item.fieldName));
                    _varEdgeContent.Add(subContent);
                }
            }
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
