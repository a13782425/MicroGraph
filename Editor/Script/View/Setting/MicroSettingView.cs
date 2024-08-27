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
            if (!string.IsNullOrWhiteSpace(MicroGraphUtils.EditorConfig.editorFont))
            {

#if UNITY_2022_1_OR_NEWER
                if (MicroGraphUtils.CurrentFont.name.StartsWith("LegacyRuntime"))
#else
                if (MicroGraphUtils.CurrentFont.name.StartsWith("Arial"))
#endif
                {
                    _fontField.SetValueWithoutNotify(null);
                    MicroGraphUtils.EditorConfig.editorFont = string.Empty;
                }
                else
                    _fontField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.editorFont));
                //Font font = AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.editorFont);
                //_fontField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Font>(MicroGraphUtils.EditorConfig.editorFont));
                //if (font == null)
                //{
                //    MicroGraphUtils.EditorConfig.editorFont = string.Empty;
                //}
            }
            _undoSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.undoStep);
            _gridToggle.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.defaultOpenGrid);
            _zoomToggle.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.defaultOpenZoom);
            _graphTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.graphTitleLength);
            _nodeTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.nodeTitleLength);
            _groupTitleSliderInt.SetValueWithoutNotify(MicroGraphUtils.EditorConfig.groupTitleLength);
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
                    MicroGraphUtils.EditorConfig.editorFont = AssetDatabase.GetAssetPath(a.newValue);
                else
                    MicroGraphUtils.EditorConfig.editorFont = string.Empty;
                MicroGraphUtils.RefreshFont();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED, nameof(MicroGraphUtils.EditorConfig.editorFont));
            });
            _scrollView.Add(_fontField);

            _recordSavePathToggle = new Toggle("记录上次保存路径：");
            _recordSavePathToggle.value = MicroGraphUtils.EditorConfig.recordSavePath;
            _recordSavePathToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.recordSavePath = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_recordSavePathToggle);

            _gridToggle = new Toggle("默认开启网格：");
            _gridToggle.value = MicroGraphUtils.EditorConfig.defaultOpenGrid;
            _gridToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.defaultOpenGrid = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_gridToggle);

            _zoomToggle = new Toggle("默认开启缩放：");
            _zoomToggle.value = MicroGraphUtils.EditorConfig.defaultOpenZoom;
            _zoomToggle.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.defaultOpenZoom = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_zoomToggle);

            _undoSliderInt = new SliderInt("回退步数：", 1, 1024);
            _undoSliderInt.value = MicroGraphUtils.EditorConfig.undoStep;
            _undoSliderInt.showInputField = true;
            _undoSliderInt.style.flexShrink = 1;
            _undoSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.undoStep = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_undoSliderInt);

            _graphTitleSliderInt = new SliderInt("图名长度：", 1, 1024);
            _graphTitleSliderInt.value = MicroGraphUtils.EditorConfig.graphTitleLength;
            _graphTitleSliderInt.showInputField = true;
            _graphTitleSliderInt.style.flexShrink = 1;
            _graphTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.graphTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_graphTitleSliderInt);

            _nodeTitleSliderInt = new SliderInt("节点名长度：", 1, 1024);
            _nodeTitleSliderInt.value = MicroGraphUtils.EditorConfig.nodeTitleLength;
            _nodeTitleSliderInt.showInputField = true;
            _nodeTitleSliderInt.style.flexShrink = 1;
            _nodeTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.nodeTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_nodeTitleSliderInt);

            _groupTitleSliderInt = new SliderInt("分组名长度：", 1, 1024);
            _groupTitleSliderInt.value = MicroGraphUtils.EditorConfig.groupTitleLength;
            _groupTitleSliderInt.showInputField = true;
            _groupTitleSliderInt.style.flexShrink = 1;
            _groupTitleSliderInt.RegisterValueChangedCallback(a =>
            {
                MicroGraphUtils.EditorConfig.groupTitleLength = a.newValue;
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_SETTING_CHANGED);
            });
            _scrollView.Add(_groupTitleSliderInt);
        }
    }
}
