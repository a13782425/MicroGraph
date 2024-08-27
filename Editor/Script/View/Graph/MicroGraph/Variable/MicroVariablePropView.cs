using MicroGraph.Runtime;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal sealed class MicroVariablePropView : VisualElement
    {
        private BaseMicroGraphView _owner;
        private MicroVariableEditorInfo _editorInfo;

        private VariableCategoryModel _categoryModel;
        private IVariableElement _variableElement;
        private VisualElement _element;
        public MicroVariablePropView(BaseMicroGraphView graphView, MicroVariableEditorInfo editorInfo)
        {
            this._owner = graphView;
            this._editorInfo = editorInfo;
            _categoryModel = MicroGraphProvider.GetVariableCategory(editorInfo.Target.GetValueType());
            if (_categoryModel == null)
            {
                Debug.LogError($"变量类型:{editorInfo.Target.GetValueType()}没有找到");
                return;
            }
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
    }
}
