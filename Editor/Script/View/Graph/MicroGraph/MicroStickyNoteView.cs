using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [Serializable]
    [Flags]
    internal enum MicroStickyFontStyle
    {
        /// <summary>
        /// 加粗
        /// </summary>
        Bold = 1 << 0,
        /// <summary>
        /// 倾斜
        /// </summary>
        Italic = 1 << 1,
        /// <summary>
        /// 删除线
        /// </summary>
        Strickout = 1 << 2,
        /// <summary>
        /// 下划线
        /// </summary>
        Underline = 1 << 3,
    }

    /// <summary>
    /// 便签节点
    /// </summary>
    internal class MicroStickyNoteView : GraphElement
    {
        //
        // 摘要:
        //     Instantiates a StickyNote with the data read from a UXML file.
        public new class UxmlFactory : UxmlFactory<MicroStickyNoteView>
        {
            //
            // 摘要:
            //     Constructor.
            public UxmlFactory()
            {
            }
        }
        private const string STYLE_PATH = "Uss/MicroGraph/MicroStickyNoteView";
        private static List<int> fontSizes = new List<int>() {
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36
        };
        private BaseMicroGraphView _owner;

        private VisualElement _titleContainer;

        private Label _contents;

        private TextField _contentsField;

        private Button _themeButton;
        private Button _boldButton;
        private Button _italicButton;
        private Button _strickoutButton;
        private Button _underlineButton;
        private PopupField<int> _sizePopup;

        private MicroStickyEditorInfo _editorInfo;
        public MicroStickyEditorInfo editorInfo => _editorInfo;

        private StickyNoteTheme _theme = StickyNoteTheme.Classic;

        private MicroStickyFontStyle _fontStyle;

        //
        // 摘要:
        //     The default size of the [StickyNote].
        public static readonly Vector2 defaultSize = new Vector2(140f, 100f);

        private const string fitTextClass = "fit-text";

        //
        // 摘要:
        //     The visual theme of the [StickyNote].
        public StickyNoteTheme theme
        {
            get
            {
                return _theme;
            }
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    UpdateThemeClasses();
                }
            }
        }

        public Vector3 LastPos { get; internal set; }

        public MicroStickyNoteView()
        {
            m_createElement();
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_sticky");
            this.capabilities |= Capabilities.Selectable;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }
        internal void Initialize(BaseMicroGraphView baseMicroGraphView, MicroStickyEditorInfo sticky)
        {
            this._owner = baseMicroGraphView;
            this._editorInfo = sticky;
            this.theme = (StickyNoteTheme)this._editorInfo.Theme;
            this._fontStyle = (MicroStickyFontStyle)this._editorInfo.FontStyle;
            _contents.style.fontSize = this._editorInfo.FontSize;
            m_changeFontSize(_contentsField, this._editorInfo.FontSize - 1);
            _sizePopup.value = _editorInfo.FontSize;
            if ((_fontStyle & MicroStickyFontStyle.Bold) > 0)
                _boldButton.AddToClassList("select");
            else
                _boldButton.RemoveFromClassList("select");
            if ((_fontStyle & MicroStickyFontStyle.Italic) > 0)
                _italicButton.AddToClassList("select");
            else
                _italicButton.RemoveFromClassList("select");
            if ((_fontStyle & MicroStickyFontStyle.Underline) > 0)
                _underlineButton.AddToClassList("select");
            else
                _underlineButton.RemoveFromClassList("select");
            if ((_fontStyle & MicroStickyFontStyle.Strickout) > 0)
                _strickoutButton.AddToClassList("select");
            else
                _strickoutButton.RemoveFromClassList("select");
            m_refreshContent();
            SetPosition(new Rect(_editorInfo.Pos, _editorInfo.Size));
            LastPos = _editorInfo.Pos;
        }
        private void m_createElement()
        {
            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("UXML/GraphView/StickyNote.uxml");
            if (visualTreeAsset == null)
            {
                visualTreeAsset = EditorGUIUtility.Load("UXML/GraphView/StickyNote.uxml") as VisualTreeAsset;
            }

            visualTreeAsset.CloneTree(this);
            base.capabilities = Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Ascendable | Capabilities.Copiable;
            var titleLabel = this.Q<Label>("title");
            if (titleLabel != null)
            {
                titleLabel.RemoveFromHierarchy();
                titleLabel = null;
            }
            var titleField = this.Q<TextField>("title-field");
            if (titleField != null)
            {
                titleField.RemoveFromHierarchy();
                titleField = null;
            }
            _titleContainer = new VisualElement();
            _titleContainer.AddToClassList("title_container");
            _contents = this.Q<Label>("contents");
            if (_contents != null)
            {
                _contents.parent.Insert(0, _titleContainer);
                _contentsField = _contents.Q<TextField>("contents-field");
                if (_contentsField != null)
                {
                    _contentsField.style.display = DisplayStyle.None;
                    _contentsField.multiline = true;
                    _contentsField.Q("unity-text-input").RegisterCallback<BlurEvent>(OnContentsBlur, TrickleDown.TrickleDown);
                }

                _contents.RegisterCallback<MouseDownEvent>(OnContentsMouseDown);
            }
            m_createTitle();
            AddToClassList("sticky-note");
            AddToClassList("selectable");
            UpdateThemeClasses();
            base.styleSheets.Add(EditorGUIUtility.Load("StyleSheets/GraphView/Selectable.uss") as StyleSheet);
            base.styleSheets.Add(EditorGUIUtility.Load("StyleSheets/GraphView/StickyNote.uss") as StyleSheet);
        }

        private void m_createTitle()
        {
            _themeButton = new Button(m_changedTheme);
            _themeButton.AddToClassList("title_button");
            _themeButton.AddToClassList("theme_button");
            this._titleContainer.Add(_themeButton);
            _boldButton = new Button(m_changedFontBold);
            _boldButton.AddToClassList("title_button");
            _boldButton.AddToClassList("bold_button");
            this._titleContainer.Add(_boldButton);

            _italicButton = new Button(m_changedFontItalic);
            _italicButton.AddToClassList("title_button");
            _italicButton.AddToClassList("italic_button");
            this._titleContainer.Add(_italicButton);

            _underlineButton = new Button(m_changedFontUnderline);
            _underlineButton.AddToClassList("title_button");
            _underlineButton.AddToClassList("underline_button");
            this._titleContainer.Add(_underlineButton);

            _strickoutButton = new Button(m_changedFontStrickout);
            _strickoutButton.AddToClassList("title_button");
            _strickoutButton.AddToClassList("strickout_button");
            this._titleContainer.Add(_strickoutButton);

            _sizePopup = new PopupField<int>(fontSizes, 0);
            _sizePopup.RegisterValueChangedCallback(m_fontSizePopupChanged);
            _sizePopup.AddToClassList("size_popup");
            this._titleContainer.Add(_sizePopup);
        }

        private void m_refreshContent()
        {
            string content = _editorInfo.Content;
            if ((_fontStyle & MicroStickyFontStyle.Bold) > 0)
                content = "<b>" + content + "</b>";
            if ((_fontStyle & MicroStickyFontStyle.Italic) > 0)
                content = "<i>" + content + "</i>";
            if ((_fontStyle & MicroStickyFontStyle.Underline) > 0)
                content = "<u>" + content + "</u>";
            if ((_fontStyle & MicroStickyFontStyle.Strickout) > 0)
                content = "<s>" + content + "</s>";
            _contents.text = content;
        }
        private void m_fontSizePopupChanged(ChangeEvent<int> evt)
        {
            int fontSize = evt.newValue;
            _contents.style.fontSize = fontSize;
            _editorInfo.FontSize = fontSize;
            m_changeFontSize(_contentsField, fontSize - 1);


        }
        private void m_changeFontSize(VisualElement element, int editorFontSize)
        {
            foreach (VisualElement child in element.Children())
            {
                child.style.fontSize = editorFontSize;
                m_changeFontSize(child, editorFontSize);
            }
        }
        private void m_changedTheme()
        {
            if (this._theme == StickyNoteTheme.Classic)
                this.theme = StickyNoteTheme.Black;
            else
                this.theme = StickyNoteTheme.Classic;
            _editorInfo.Theme = (int)this._theme;
        }

        private void m_changedFontBold()
        {
            _fontStyle = _fontStyle ^ MicroStickyFontStyle.Bold;
            if ((_fontStyle & MicroStickyFontStyle.Bold) > 0)
                _boldButton.AddToClassList("select");
            else
                _boldButton.RemoveFromClassList("select");
            _editorInfo.FontStyle = (int)this._fontStyle;
            m_refreshContent();
        }

        private void m_changedFontItalic()
        {
            _fontStyle = _fontStyle ^ MicroStickyFontStyle.Italic;
            if ((_fontStyle & MicroStickyFontStyle.Italic) > 0)
                _italicButton.AddToClassList("select");
            else
                _italicButton.RemoveFromClassList("select");
            _editorInfo.FontStyle = (int)this._fontStyle;
            m_refreshContent();
        }

        private void m_changedFontUnderline()
        {
            _fontStyle = _fontStyle ^ MicroStickyFontStyle.Underline;
            if ((_fontStyle & MicroStickyFontStyle.Underline) > 0)
                _underlineButton.AddToClassList("select");
            else
                _underlineButton.RemoveFromClassList("select");
            _editorInfo.FontStyle = (int)this._fontStyle;
            m_refreshContent();
        }

        private void m_changedFontStrickout()
        {
            _fontStyle = _fontStyle ^ MicroStickyFontStyle.Strickout;
            if ((_fontStyle & MicroStickyFontStyle.Strickout) > 0)
                _strickoutButton.AddToClassList("select");
            else
                _strickoutButton.RemoveFromClassList("select");
            _editorInfo.FontStyle = (int)this._fontStyle;
            m_refreshContent();
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_owner.View.selection.OfType<Node>().Count() > 1)
                evt.menu.AppendAction("添加模板", (e) => _owner.AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
            evt.StopPropagation();
        }

        private void OnContentsBlur(BlurEvent e)
        {
            bool flag = _contents.text != _contentsField.value;
            _editorInfo.Content = _contentsField.value;
            _contentsField.style.display = DisplayStyle.None;
            m_refreshContent();
        }
        private void OnContentsMouseDown(MouseDownEvent e)
        {
            if (e.button == 0 && e.clickCount == 2)
            {
                _contentsField.value = _editorInfo.Content;
                _contentsField.style.display = DisplayStyle.Flex;
                _contentsField.Q(TextInputBaseField<string>.textInputUssName).Focus();
                e.StopPropagation();
                e.PreventDefault();
            }
        }
        private void UpdateThemeClasses()
        {
            foreach (StickyNoteTheme value in Enum.GetValues(typeof(StickyNoteTheme)))
            {
                if (_theme != value)
                {
                    RemoveFromClassList("theme-" + value.ToString().ToLower());
                }
                else
                {
                    AddToClassList("theme-" + value.ToString().ToLower());
                }
            }
        }
        public override void SetPosition(Rect newPos)
        {
            _editorInfo.Pos = newPos.position;
            _editorInfo.Size = newPos.size;
            base.SetPosition(newPos);
        }

        internal void OnDestory()
        {
        }
    }
}
