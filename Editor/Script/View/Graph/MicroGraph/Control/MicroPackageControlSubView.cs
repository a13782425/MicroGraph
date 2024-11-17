//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEditor.Experimental.GraphView;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;

//namespace MicroGraph.Editor
//{
//    [MicroGraphOrder(MicroVariableControlSubView.VARIABLE_CONTROL_ORDER + 100)]
//    internal class MicroPackageControlSubView : VisualElement, IMicroSubControl
//    {
//        public VisualElement Panel => this;
//        public string Name => "节点包信息";
//        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroTemplateControlSubView";
//        private BaseMicroGraphView _owner;
//        public BaseMicroGraphView owner => _owner;
//        private ToolbarSearchField _searchField;
//        private ScrollView _scrollView;
//        private VisualElement _itemsContainer;
//        private Label _emptyLabel;
//        private float _scrollValue;
//        /// <summary>
//        /// 更新位置
//        /// </summary>
//        private bool _refreshScrollPos = false;
//        private List<MicroGraphTemplateModel> _allModels = new List<MicroGraphTemplateModel>();
//        private List<MicroGraphTemplateModel> _showModels = new List<MicroGraphTemplateModel>();
//        private List<MicroTemplateElement> _cacheList = new List<MicroTemplateElement>();
//        private MicroGraphConfig _editorModel;
//        public MicroGraphConfig editorModel => _editorModel;
//        public MicroPackageControlSubView(BaseMicroGraphView owner)
//        {
//            this._owner = owner;
//            string graphClassName = owner.CategoryModel.GraphType.FullName;
//            _editorModel = MicroGraphUtils.EditorConfig.GraphConfigs.FirstOrDefault(a => a.GraphClassName == graphClassName);
//            if (_editorModel == null)
//            {
//                _editorModel = new MicroGraphConfig();
//                _editorModel.GraphClassName = graphClassName;
//                MicroGraphUtils.EditorConfig.GraphConfigs.Add(_editorModel);
//                MicroGraphUtils.SaveConfig();
//            }
//            this.AddStyleSheet(STYLE_PATH);
//            this.AddToClassList("microtemplatecontrol");
//            _searchField = new ToolbarSearchField();
//            _searchField.AddToClassList("template_searchField");
//            _searchField.RegisterValueChangedCallback(m_onSearchFieldChanged);
//            this.Add(_searchField);
//            _scrollView = new ScrollView(ScrollViewMode.Vertical);
//            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
//            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
//            _scrollView.AddToClassList("template_scrollview");
//            _itemsContainer = new VisualElement();
//            _itemsContainer.AddToClassList("template_itemcontainer");
//            _itemsContainer.RegisterCallback<GeometryChangedEvent>(m_containerGeometryChanged);
//            _emptyLabel = new Label("模板列表为空");
//            _emptyLabel.AddToClassList("template_emptylabel");
//            _scrollView.contentContainer.Add(_itemsContainer);
//            this.Add(_scrollView);
//            owner.listener.AddListener(MicroGraphEventIds.GRAPH_TEMPLATE_CHANGED, m_refreshTemplate);
//            m_refreshTemplate();
//            RegisterCallback(delegate (DragUpdatedEvent e)
//            {
//                e.StopPropagation();
//            });
//        }

//        private void m_onSearchFieldChanged(ChangeEvent<string> evt)
//        {
//            m_searchTemplate(evt.newValue);
//        }
//        private void m_searchTemplate(string character = "")
//        {
//            _showModels.Clear();
//            if (string.IsNullOrWhiteSpace(character))
//            {
//                _showModels.AddRange(_allModels);
//                if (_allModels.Count == 0)
//                    _emptyLabel.text = "模板列表为空";
//            }
//            else
//            {
//                _showModels.AddRange(_allModels.Where(a => a.Title.Contains(character, StringComparison.OrdinalIgnoreCase)));
//                if (_showModels.Count == 0)
//                    _emptyLabel.text = "搜索结果为空";
//            }
//            m_showTemplate();
//        }
//        private void m_containerGeometryChanged(GeometryChangedEvent evt)
//        {
//            if (_refreshScrollPos)
//                _scrollView.schedule.Execute(() => { _scrollView.verticalScroller.value = _scrollValue; }).StartingIn(1);
//            _refreshScrollPos = false;
//        }

//        private bool m_refreshTemplate(object obj = null)
//        {
//            string graphName = _owner.CategoryModel.GraphType.FullName;
//            _allModels.Clear();
//            _showModels.Clear();
//            _allModels.AddRange(_editorModel.Templates);
//            m_searchTemplate(_searchField.value);
//            return true;
//        }

//        private void m_showTemplate()
//        {
//            //foreach (var item in _itemsContainer.Children())
//            //{
//            //    if (item is MicroTemplateElement element)
//            //        _cacheList.Add(element);
//            //}
//            //_itemsContainer.Clear();
//            //if (_showModels.Count == 0)
//            //{
//            //    _itemsContainer.Add(_emptyLabel);
//            //    return;
//            //}
//            //_scrollValue = _scrollView.verticalScroller.value;
//            //foreach (var item in _showModels)
//            //{
//            //    MicroTemplateElement template = null;
//            //    if (_cacheList.Count > 0)
//            //    {
//            //        template = _cacheList[_cacheList.Count - 1];
//            //        _cacheList.RemoveAt(_cacheList.Count - 1);
//            //    }
//            //    else
//            //        template = new MicroTemplateElement(this);
//            //    template.Refresh(item);
//            //    _itemsContainer.Add(template);
//            //}
//            //_refreshScrollPos = true;
//        }

//        public void Hide()
//        {
//        }

//        public void Show()
//        {
//        }
//        public void Exit()
//        {
//        }
//    }


//}
