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
    [MicroGraphOrder(MicroVariableControlSubView.VARIABLE_CONTROL_ORDER + 200)]
    internal class MicroTemplateControlSubView : VisualElement, IMicroSubControl
    {
        public VisualElement Panel => this;
        public string Name => "模板信息";
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroTemplateControlSubView";
        private BaseMicroGraphView _owner;
        public BaseMicroGraphView owner => _owner;
        private ToolbarSearchField _searchField;
        private ScrollView _scrollView;
        private VisualElement _itemsContainer;
        private Label _emptyLabel;
        private float _scrollValue;
        /// <summary>
        /// 更新位置
        /// </summary>
        private bool _refreshScrollPos = false;
        private List<MicroGraphTemplateModel> _allModels = new List<MicroGraphTemplateModel>();
        private List<MicroGraphTemplateModel> _showModels = new List<MicroGraphTemplateModel>();
        private List<MicroTemplateElement> _cacheList = new List<MicroTemplateElement>();
        private MicroGraphConfig _editorModel;
        public MicroGraphConfig editorModel => _editorModel;
        public MicroTemplateControlSubView(BaseMicroGraphView owner)
        {
            this._owner = owner;
            string graphClassName = owner.CategoryModel.GraphType.FullName;
            _editorModel = MicroGraphUtils.EditorConfig.GraphConfigs.FirstOrDefault(a => a.GraphClassName == graphClassName);
            if (_editorModel == null)
            {
                _editorModel = new MicroGraphConfig();
                _editorModel.GraphClassName = graphClassName;
                MicroGraphUtils.EditorConfig.GraphConfigs.Add(_editorModel);
                MicroGraphUtils.SaveConfig();
            }
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microtemplatecontrol");
            _searchField = new ToolbarSearchField();
            _searchField.AddToClassList("template_searchField");
            _searchField.RegisterValueChangedCallback(m_onSearchFieldChanged);
            this.Add(_searchField);
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.AddToClassList("template_scrollview");
            _itemsContainer = new VisualElement();
            _itemsContainer.AddToClassList("template_itemcontainer");
            _itemsContainer.RegisterCallback<GeometryChangedEvent>(m_containerGeometryChanged);
            _emptyLabel = new Label("模板列表为空");
            _emptyLabel.AddToClassList("template_emptylabel");
            _scrollView.contentContainer.Add(_itemsContainer);
            this.Add(_scrollView);
            owner.listener.AddListener(MicroGraphEventIds.GRAPH_TEMPLATE_CHANGED, m_refreshTemplate);
            m_refreshTemplate();
            RegisterCallback(delegate (DragUpdatedEvent e)
            {
                e.StopPropagation();
            });
        }

        private void m_onSearchFieldChanged(ChangeEvent<string> evt)
        {
            m_searchTemplate(evt.newValue);
        }
        private void m_searchTemplate(string character = "")
        {
            _showModels.Clear();
            if (string.IsNullOrWhiteSpace(character))
            {
                _showModels.AddRange(_allModels);
                if (_allModels.Count == 0)
                    _emptyLabel.text = "模板列表为空";
            }
            else
            {
                _showModels.AddRange(_allModels.Where(a => a.Title.Contains(character, StringComparison.OrdinalIgnoreCase)));
                if (_showModels.Count == 0)
                    _emptyLabel.text = "搜索结果为空";
            }
            m_showTemplate();
        }
        private void m_containerGeometryChanged(GeometryChangedEvent evt)
        {
            if (_refreshScrollPos)
                _scrollView.schedule.Execute(() => { _scrollView.verticalScroller.value = _scrollValue; }).StartingIn(1);
            _refreshScrollPos = false;
        }

        private bool m_refreshTemplate(object obj = null)
        {
            string graphName = _owner.CategoryModel.GraphType.FullName;
            _allModels.Clear();
            _showModels.Clear();
            _allModels.AddRange(_editorModel.Templates);
            m_searchTemplate(_searchField.value);
            return true;
        }

        private void m_showTemplate()
        {
            foreach (var item in _itemsContainer.Children())
            {
                if (item is MicroTemplateElement element)
                    _cacheList.Add(element);
            }
            _itemsContainer.Clear();
            if (_showModels.Count == 0)
            {
                _itemsContainer.Add(_emptyLabel);
                return;
            }
            _scrollValue = _scrollView.verticalScroller.value;
            foreach (var item in _showModels)
            {
                MicroTemplateElement template = null;
                if (_cacheList.Count > 0)
                {
                    template = _cacheList[_cacheList.Count - 1];
                    _cacheList.RemoveAt(_cacheList.Count - 1);
                }
                else
                    template = new MicroTemplateElement(this);
                template.Refresh(item);
                _itemsContainer.Add(template);
            }
            _refreshScrollPos = true;
        }

        public void Hide()
        {
        }

        public void Show()
        {
        }
        public void Exit()
        {
        }
    }
    internal class MicroTemplateElement : GraphElement
    {
        private readonly static Color NODE_COLOR;
        private readonly static Color VAR_NODE_COLOR;
        private readonly static Color STICKY_COLOR;

        private readonly static Vector2 NODE_SIZE = new Vector2(160, 160);
        private readonly static Vector2 VAR_SIZE = new Vector2(160, 80);

        private static Vector3[] s_cachedRect = new Vector3[4];
        static MicroTemplateElement()
        {
            NODE_COLOR = MicroGraphUtils.GetColor(typeof(MicroNodeSerializeModel));
            VAR_NODE_COLOR = MicroGraphUtils.GetColor(typeof(MicroVarNodeSerializeModel).Name);
            STICKY_COLOR = MicroGraphUtils.GetColor(typeof(MicroStickySerializeModel).Name);
        }
        public BaseMicroGraphView owner => _control.owner;
        public MicroGraphTemplateModel templateModel => _item;
        private IMGUIContainer _container;
        private Image _warning;
        private EditorLabelElement _templateLabel;
        private MicroTemplateControlSubView _control;
        private MicroGraphTemplateModel _item;
        private List<string> _warningList = new List<string>();
        /// <summary>
        /// 收藏所有元素的矩形
        /// 没有缩放
        /// </summary>
        private Rect _templateRect;
        private Rect _contentRect = new Rect(4, 4, 64, 64);
        public MicroTemplateElement(MicroTemplateControlSubView control)
        {
            this._control = control;
            this.AddToClassList("template_element");
            _container = new IMGUIContainer(DrawTemplateContent);
            _container.AddToClassList("template_element_gui");
            _warning = new Image();
            _warning.AddToClassList("template_element_warnning");
            _templateLabel = new EditorLabelElement("", true);
            _templateLabel.AddToClassList("template_element_name");
            _templateLabel.onRename += onRename;
            this.Add(this._container);
            this.Add(this._warning);
            this.Add(this._templateLabel);
            this.capabilities |= Capabilities.Droppable | Capabilities.Selectable;
            this.AddManipulator(new SelectionDropper());
            this.AddManipulator(new ContextualMenuManipulator(m_contextMenu));
            _warning.SetDisplay(false);
        }
        internal void Refresh(MicroGraphTemplateModel item)
        {
            this._item = item;
            _templateLabel.text = item.Title;
            this.tooltip = item.Title;
            _warning.SetDisplay(false);
            _warningList.Clear();
            bool isWarning = false;
            GraphCategoryModel categoryModel = MicroGraphProvider.GetGraphCategory(item.GraphClassName);
            foreach (var node in item.Nodes)
            {
                NodeCategoryModel nodeCategory = categoryModel.NodeCategories.FirstOrDefault(a => a.NodeClassType.FullName == node.ClassName);
                if (nodeCategory == null || nodeCategory.EnableState != Runtime.MicroNodeEnableState.Enabled)
                {
                    isWarning = true;
                    break;
                }
            }
            _warning.SetDisplay(isWarning);
            if (isWarning)
            {
                _warning.tooltip = "该模板中部分节点不存在或不可用";
            }

        }
        private void m_contextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("删除", (e) =>
            {
                _control.editorModel.Templates.Remove(_item);
                MicroGraphUtils.SaveConfig();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.GRAPH_TEMPLATE_CHANGED);
            });
            evt.StopPropagation();
        }

        private void onRename(string oldName, string newName)
        {
            if (!MicroGraphUtils.TitleValidity(newName, MicroGraphUtils.EditorConfig.GroupTitleLength))
            {
                _templateLabel.text = oldName;
                _control.owner.owner.ShowNotification(new GUIContent("模板名不合法"), MicroGraphUtils.NOTIFICATION_TIME);
                return;
            }
            this.tooltip = newName;
            _item.Title = newName;
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.GRAPH_TEMPLATE_CHANGED);
        }

        private void DrawTemplateContent()
        {
            Color color = Handles.color;
            Rect rect = _container.layout;
            //(x:7.00, y:5.00, width:72.00, height:72.00)
            m_calculateTemplateRect();
            m_drawElements();
            Handles.color = color;
        }

        private void m_drawElements()
        {
            float xFactor = _contentRect.width / _templateRect.width;
            float yFactor = _contentRect.height / _templateRect.height;
            float factor = 0;
            float xOffset = 0;
            float yOffset = 0;
            if (xFactor > yFactor)
            {
                factor = yFactor;
                xOffset = _contentRect.width - _contentRect.width / (xFactor / yFactor);
                xOffset *= 0.5f;
            }
            else
            {
                factor = xFactor;
                yOffset = _contentRect.height - _contentRect.height / (yFactor / xFactor);
                yOffset *= 0.5f;
            }

            Color faceColor = NODE_COLOR;
            faceColor.a = 0.3f;

            foreach (var item in _item.Nodes)
            {
                Rect rect = m_calculateElementRect(item.Pos, NODE_SIZE, factor, xOffset, yOffset);
                s_cachedRect[0].Set(rect.xMin, rect.yMin, 0f);
                s_cachedRect[1].Set(rect.xMax, rect.yMin, 0f);
                s_cachedRect[2].Set(rect.xMax, rect.yMax, 0f);
                s_cachedRect[3].Set(rect.xMin, rect.yMax, 0f);
                Handles.DrawSolidRectangleWithOutline(s_cachedRect, faceColor, NODE_COLOR);
            }

            faceColor = VAR_NODE_COLOR;
            faceColor.a = 0.3f;
            foreach (var item in _item.VarNodes)
            {
                Rect rect = m_calculateElementRect(item.Pos, VAR_SIZE, factor, xOffset, yOffset);
                s_cachedRect[0].Set(rect.xMin, rect.yMin, 0f);
                s_cachedRect[1].Set(rect.xMax, rect.yMin, 0f);
                s_cachedRect[2].Set(rect.xMax, rect.yMax, 0f);
                s_cachedRect[3].Set(rect.xMin, rect.yMax, 0f);
                Handles.DrawSolidRectangleWithOutline(s_cachedRect, faceColor, VAR_NODE_COLOR);
            }

            faceColor = STICKY_COLOR;
            faceColor.a = 0.3f;
            foreach (var item in _item.Stickys)
            {
                Rect rect = m_calculateElementRect(item.Pos, NODE_SIZE, factor, xOffset, yOffset);
                s_cachedRect[0].Set(rect.xMin, rect.yMin, 0f);
                s_cachedRect[1].Set(rect.xMax, rect.yMin, 0f);
                s_cachedRect[2].Set(rect.xMax, rect.yMax, 0f);
                s_cachedRect[3].Set(rect.xMin, rect.yMax, 0f);
                Handles.DrawSolidRectangleWithOutline(s_cachedRect, faceColor, STICKY_COLOR);
            }
        }

        private Rect m_calculateElementRect(Vector2 pos, Vector2 size, float factor, float xOffset, float yOffset)
        {
            Vector2 min = pos;
            min.y -= size.y;
            Vector2 max = pos;
            max.x += size.x;
            Rect rect = new Rect();
            rect.min = min - _templateRect.min;
            rect.max = rect.min + size;
            rect.min = rect.min * factor;
            rect.max = rect.max * factor;

            rect.xMin += _contentRect.x + xOffset;
            rect.yMin += _contentRect.y + yOffset;
            rect.xMax += _contentRect.x + xOffset;
            rect.yMax += _contentRect.y + yOffset;
            return rect;
        }

        private void m_calculateTemplateRect()
        {
            _templateRect = new Rect();
            _templateRect.xMax = float.MinValue;
            _templateRect.yMax = float.MinValue;
            _templateRect.xMin = float.MaxValue;
            _templateRect.yMin = float.MaxValue;

            foreach (var item in _item.Nodes)
                m_calculate(item.Pos, NODE_SIZE);
            foreach (var item in _item.VarNodes)
                m_calculate(item.Pos, VAR_SIZE);
            foreach (var item in _item.Stickys)
                m_calculate(item.Pos, NODE_SIZE);
            void m_calculate(Vector2 pos, Vector2 size)
            {
                Vector2 min = pos;
                min.y -= size.y;
                Vector2 max = pos;
                max.x += size.x;
                if (_templateRect.min.x > min.x)
                    _templateRect.xMin = min.x;
                if (_templateRect.min.y > min.y)
                    _templateRect.yMin = min.y;

                if (_templateRect.max.x < max.x)
                    _templateRect.xMax = max.x;
                if (_templateRect.max.y < max.y)
                    _templateRect.yMax = max.y;
            }
        }
    }
}
