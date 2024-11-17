using MicroGraph.Runtime;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal sealed class MicroVariablePropView : VisualElement
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroVariablePropView";
        private BaseMicroGraphView _owner;
        private MicroVariableEditorInfo _editorInfo;
        private TextField _commentField;
        private VariableCategoryModel _categoryModel;
        private IVariableElement _variableElement;
        private VisualElement _element;
        public MicroVariablePropView(BaseMicroGraphView graphView, MicroVariableEditorInfo editorInfo)
        {
            this.AddStyleSheet(STYLE_PATH);
            this._owner = graphView;
            this._editorInfo = editorInfo;
            _categoryModel = MicroGraphProvider.GetVariableCategory(editorInfo.Target.GetValueType());
            if (_categoryModel == null)
            {
                Debug.LogError($"变量类型:{editorInfo.Target.GetValueType()}没有找到");
                return;
            }
            _commentField = new TextField("注释:");
            _commentField.multiline = true;
            _commentField.focusable = true;
            _commentField.AddToClassList("comment_input");
            _commentField.RegisterCallback<FocusInEvent>(m_focusIn);
            _commentField.RegisterCallback<FocusOutEvent>(m_focusOut);
            _commentField.RegisterValueChangedCallback(m_onValueChanged);
            m_setCommentField();
            Add(_commentField);
            if (_categoryModel.VarViewType != null)
            {
                _variableElement = Activator.CreateInstance(_categoryModel.VarViewType) as IVariableElement;
            }
            if (_variableElement != null)
            {
                _element = _variableElement.DrawElement(graphView, editorInfo.Target, editorInfo.Target.HasDefaultValue && _editorInfo.CanDefaultValue);
                Add(_element);
            }
            else
            {
                Label label = new Label("没有找到对应视图类");
                Add(label);
            }
        }



        private void m_setCommentField()
        {
            if (string.IsNullOrWhiteSpace(_editorInfo.Comment))
            {
                if (!_commentField.ClassListContains("comment_input_placeholder"))
                    _commentField.AddToClassList("comment_input_placeholder");
                _commentField.SetValueWithoutNotify("输入注释...");
            }
            else
            {
                if (_commentField.ClassListContains("comment_input_placeholder"))
                    _commentField.RemoveFromClassList("comment_input_placeholder");
                _commentField.SetValueWithoutNotify(_editorInfo.Comment);
            }
        }
        private void m_focusIn(FocusInEvent evt)
        {
            _commentField.SetValueWithoutNotify(_editorInfo.Comment);
        }
        private void m_focusOut(FocusOutEvent evt)
        {
            m_setCommentField();
        }
        private void m_onValueChanged(ChangeEvent<string> evt)
        {
            _editorInfo.Comment = evt.newValue;
            m_setCommentField();
            this._owner.listener.OnEvent(MicroGraphEventIds.VAR_MODIFY, new VarModifyEventArgs() { oldVarName = this._editorInfo.Name, var = this._editorInfo.Target });
        }
    }
}
