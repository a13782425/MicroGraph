using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 搜索界面
    /// </summary>
    internal class MicroSearchView : GraphElement
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroSearchView";
        private BaseMicroGraphView _owner;
        private Button _closeBtn;
        private TextField _searchField;
        private Label _resultLabel;
        private SeparatorElement _separatorElement;
        private Button _prevButton;
        private Button _nextButton;
        private List<int> _resultList = new List<int>();
        private int _curIndex;
        public MicroSearchView(BaseMicroGraphView graph)
        {
            this._owner = graph;
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("micro_search");
            _searchField = new TextField();
            _searchField.multiline = false;
            _searchField.focusable = true;
            _searchField.RegisterValueChangedCallback(m_searchFieldChanged);
            _searchField.AddToClassList("micro_search_search_input");
            this.Add(_searchField);
            _resultLabel = new Label();
            _resultLabel.AddToClassList("micro_search_result_label");
            this.Add(_resultLabel);
            _separatorElement = new SeparatorElement();
            _separatorElement.direction = SeparatorDirection.Vertical;
            _separatorElement.color = new Color32(28, 28, 28, 255);
            _separatorElement.style.height = Length.Percent(60);
            this.Add(_separatorElement);

            _prevButton = new Button(m_prevClick);
            _prevButton.AddToClassList("micro_search_prev_btn");
            _nextButton = new Button(m_nextClick);
            _nextButton.AddToClassList("micro_search_next_btn");
            this.Add(this._prevButton);
            this.Add(this._nextButton);
            _closeBtn = new Button(m_close);
            _closeBtn.AddToClassList("micro_search_close_btn");
            this.Add(_closeBtn);
        }

        private void m_prevClick()
        {
            _curIndex--;
            focusElement();
        }

        private void m_nextClick()
        {
            _curIndex++;
            focusElement();
        }

        private void focusElement()
        {
            int index = _curIndex - 1;
            if (index < 0)
            {
                index = _resultList.Count - 1;
                _curIndex = index + 1;
            }
            else if (index >= _resultList.Count)
            {
                index = 0;
                _curIndex = index + 1;
            }
            if (index > -1 && index < _resultList.Count)
            {
                _owner.View.ClearSelection();
                int onlyId = _resultList[index];
                GraphElement graphElement = _owner.GetElement<GraphElement>(onlyId);
                _resultLabel.text = _curIndex + "/" + _resultList.Count;
                if (graphElement == null)
                    return;
                _owner.View.AddToSelection(graphElement);
                _owner.View.FrameSelection();
            }
        }

        private void m_searchFieldChanged(ChangeEvent<string> evt)
        {
            _resultList.Clear();
            _curIndex = 0;
            if (string.IsNullOrWhiteSpace(evt.newValue))
            {
                _resultLabel.text = "0/0";
                return;
            }
            _resultList.AddRange(_owner.editorInfo.Nodes
                .Where(node =>
                {
                    var nodeView = _owner.GetElement<BaseMicroNodeView.InternalNodeView>(node.NodeId);
                    foreach (var element in nodeView.nodeView.nodefieldElements)
                    {
                        if (element.ToValueString().Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    if (node.NodeId.ToString() == evt.newValue)
                        return true;
                    return node.Title.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase);
                })
                .Select(a => a.NodeId));
            _resultList.AddRange(_owner.editorInfo.VariableNodes
                .Where(node => node.Name.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                .Select(a => a.NodeId));
            _resultList.AddRange(_owner.editorInfo.Stickys
                .Where(node => node.Content.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                .Select(a => a.NodeId));
            if (_resultList.Count > 0)
            {
                _curIndex = 1;
                _resultLabel.text = _curIndex + "/" + _resultList.Count;
                focusElement();
            }
            else
                _resultLabel.text = "0/0";
        }

        private void m_close()
        {
            _owner.View.Focus();
            this.SetDisplay(false);
        }

        public void Search()
        {
            if (this.style.display == DisplayStyle.Flex)
                m_close();
            else
            {
                this.SetDisplay(true);
                _resultLabel.text = "";
                _searchField.SetValueWithoutNotify("");
                _searchField.Focus();
            }
        }
    }
}
