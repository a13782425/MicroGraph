using UnityEditor;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [MicroGraphOrder(MicroVariableControlSubView.VARIABLE_CONTROL_ORDER + 400)]
    internal class MicroGraphControlSubView : VisualElement, IMicroSubControl
    {
        private BaseMicroGraphView _owner;
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroGraphControlSubView";
        private Label _nameLabel;
        private Label _desLabel;
        private Label _createTimeLabel;
        private Label _modifyTimeLabel;
        private Label _pathLabel;
        private Button _locationButton;
        public VisualElement Panel => this;
        public string Name => "微图信息";

        public MicroGraphControlSubView(BaseMicroGraphView owner)
        {
            this._owner = owner;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micrographcontrol");
            _nameLabel = new Label(owner.editorInfo.Title);
            _desLabel = new Label(string.IsNullOrWhiteSpace(owner.editorInfo.Describe) ? "这里是描述" : owner.editorInfo.Describe);
            _createTimeLabel = new Label("创建时间:  " + MicroGraphUtils.FormatTime(owner.editorInfo.CreateTime));
            _modifyTimeLabel = new Label("修改时间:  " + MicroGraphUtils.FormatTime(owner.editorInfo.ModifyTime));
            _pathLabel = new Label("路径: " + AssetDatabase.GetAssetPath(owner.Target));
            _nameLabel.AddToClassList("name_label");
            _desLabel.AddToClassList("des_label");
            _createTimeLabel.AddToClassList("time_label");
            _modifyTimeLabel.AddToClassList("time_label");
            _pathLabel.AddToClassList("path_label");
            this.Add(_nameLabel);
            this.Add(_desLabel);
            this.Add(_createTimeLabel);
            this.Add(_modifyTimeLabel);
            this.Add(_pathLabel);
            _locationButton = new Button(m_location);
            _locationButton.text = "定位";
            this.Add(_locationButton);

        }

        private void m_location()
        {
            if (_owner.Target != null)
            {
                Selection.activeObject = _owner.Target;
                EditorGUIUtility.PingObject(_owner.Target);
            }
        }

        public void Show()
        {
            _nameLabel.text = _owner.editorInfo.Title;
            _desLabel.text = string.IsNullOrWhiteSpace(_owner.editorInfo.Describe) ? "这里是描述" : _owner.editorInfo.Describe;
            _createTimeLabel.text = "创建时间:  " + MicroGraphUtils.FormatTime(_owner.editorInfo.CreateTime);
            _modifyTimeLabel.text = "修改时间:  " + MicroGraphUtils.FormatTime(_owner.editorInfo.ModifyTime);
            _pathLabel.text = "路径: " + AssetDatabase.GetAssetPath(_owner.Target);
        }

        public void Hide()
        {
        }
        public void Exit()
        {
        }
    }
}
