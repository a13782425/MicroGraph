using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 设置界面
    /// </summary>
    internal sealed class MicroSettingView : VisualElement
    {
        private const string STYLE_PATH = "Uss/MicroSettingView";
        private VisualElement _container;
        private VisualElement _bottomContainer;
        private Button _saveButton;
        private ScrollView _scrollView;
        private SliderInt _undoSliderInt;
        private SliderInt _graphTitleSliderInt;
        private SliderInt _nodeTitleSliderInt;
        private SliderInt _groupTitleSliderInt;
        private Toggle _recordSavePathToggle;
        private Toggle _gridToggle;
        private Toggle _zoomToggle;
        private ObjectField _fontField;

        private MicroGraphWindow _window;

        public MicroSettingView(MicroGraphWindow window)
        {
            this.AddStyleSheet(STYLE_PATH);
            _container = new VisualElement();
            _container.name = "contentContainer";
            _container.style.flexDirection = FlexDirection.Column;
            _bottomContainer = new VisualElement();
            _bottomContainer.name = "bottomContainer";
            this.Add(_container);
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _container.Add(_scrollView);
            this.Add(_bottomContainer);
            _saveButton = new Button(() => MicroGraphUtils.SaveConfig());
            _saveButton.text = "保存";
            _bottomContainer.Add(_saveButton);
            m_addSetting();
        }
        public void Show()
        {
            this.SetDisplay(true);
            if (!string.IsNullOrWhiteSpace(MicroGraphUtils.EditorConfig.EditorFont))
            {

#if UNITY_2022_1_OR_NEWER
                if (MicroGraphUtils.CurrentFont.name.StartsWith("LegacyRuntime"))
#else
                if (MicroGraphUtils.CurrentFont.name.StartsWith("Arial"))
#endif
                {
                    _fontField.SetValueWithoutNotify(null);
                    MicroGraphUtils.EditorConfig.EditorFont = string.Empty;
                }
                else
                    _fontField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.EditorFont));
                //Font font = AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.editorFont);
                //_fontField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.editorFont));
                //if (font == null)
                //{
                //    MicroGraphUtils.EditorConfig.editorFont = string.Empty;
                //}
            }
            _undoSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.UndoStep);
            _gridToggle.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.DefaultOpenGrid);
            _zoomToggle.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.DefaultOpenZoom);
            _graphTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.GraphTitleLength);
            _nodeTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.NodeTitleLength);
            _groupTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.GroupTitleLength);
        }
        public void Hide()
        {
            this.SetDisplay(false);
        }

        private void m_addSetting()
        {
            _fontField = new ObjectField("自定义字体(实验功能):");
            _fontField.objectType = typeof(Font);
            _fontField.RegisterValueChangedCallback((a) =>
            {
                if (a.newValue != null)
                    MicroGraphUtils.EditorConfig.EditorFont = AssetDatabase.GetAssetPath(a.newValue);
                else
                    MicroGraphUtils.EditorConfig.EditorFont = string.Empty;
                MicroGraphUtils.RefreshFont();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED, nameof(MicroGraphUtils.EditorConfig.EditorFont));
            });
            _scrollView.Add(_fontField);

            _recordSavePathToggle = new Toggle("记录上次保存路径：");
            _recordSavePathToggle.value = MicroGraphUtils.EditorConfig.RecordSavePath;
            _recordSavePathToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.RecordSavePath = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_recordSavePathToggle);

            _gridToggle = new Toggle("默认开启网格：");
            _gridToggle.value = MicroGraphUtils.EditorConfig.DefaultOpenGrid;
            _gridToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.DefaultOpenGrid = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_gridToggle);

            _zoomToggle = new Toggle("默认开启缩放：");
            _zoomToggle.value = MicroGraphUtils.EditorConfig.DefaultOpenZoom;
            _zoomToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.DefaultOpenZoom = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_zoomToggle);

            _undoSliderInt = new SliderInt("回退步数：", 1, 1024);
            _undoSliderInt.value = MicroGraphUtils.EditorConfig.UndoStep;
            _undoSliderInt.showInputField = true;
            _undoSliderInt.style.flexShrink = 1;
            _undoSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.UndoStep = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_undoSliderInt);

            _graphTitleSliderInt = new SliderInt("图名长度：", 1, 1024);
            _graphTitleSliderInt.value = MicroGraphUtils.EditorConfig.GraphTitleLength;
            _graphTitleSliderInt.showInputField = true;
            _graphTitleSliderInt.style.flexShrink = 1;
            _graphTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.GraphTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_graphTitleSliderInt);

            _nodeTitleSliderInt = new SliderInt("节点名长度：", 1, 1024);
            _nodeTitleSliderInt.value = MicroGraphUtils.EditorConfig.NodeTitleLength;
            _nodeTitleSliderInt.showInputField = true;
            _nodeTitleSliderInt.style.flexShrink = 1;
            _nodeTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.NodeTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_nodeTitleSliderInt);

            _groupTitleSliderInt = new SliderInt("分组名长度：", 1, 1024);
            _groupTitleSliderInt.value = MicroGraphUtils.EditorConfig.GroupTitleLength;
            _groupTitleSliderInt.showInputField = true;
            _groupTitleSliderInt.style.flexShrink = 1;
            _groupTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.GroupTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_groupTitleSliderInt);
        }
    }
}
