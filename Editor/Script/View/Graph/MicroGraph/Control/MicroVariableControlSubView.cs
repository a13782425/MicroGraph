using MicroGraph.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图变量面板
    /// </summary>
    internal sealed class MicroVariableControlSubView : Blackboard, IMicroSubControl
    {
        private const string STYLE_PATH = "Uss/MicroGraph/Control/MicroVariableControlSubView";
        private BaseMicroGraphView _owner;
        private MicroVariableRowView _lastSelectVar;
        public MicroVariableControlSubView(BaseMicroGraphView graph) : base(graph)
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microvariablepanel");
            _owner = graph;
            scrollable = true;
            this.capabilities |= Capabilities.Droppable;
            ScrollView scrollView = this.Q<ScrollView>();
            scrollView.mode = ScrollViewMode.Vertical;
            this.Q("titleLabel").RemoveFromHierarchy();
            this.Q("subTitleLabel").RemoveFromHierarchy();
            this.Q<Resizer>().RemoveFromHierarchy();
            this.style.position = Position.Relative;
            this.addItemRequested += m_addItem;

            //_owner.listener.AddListener(GraphEventId.VAR_ADD, m_varAdd);
        }

        private void m_mouseDownEvent(MouseDownEvent evt)
        {
            if (evt.shiftKey)
            {
                VisualElement element = evt.target as VisualElement;
                if (element == null)
                    return;
                MicroVariableRowView itemView = element.GetFirstAncestorOfType<MicroVariableRowView>();
                if (itemView == null)
                    return;
                if (_lastSelectVar == null)
                    return;
                var tempList = this.Children().ToList();
                int firstIndex = 0;
                int lastIndex = 0;
                for (int i = 0; i < tempList.Count; i++)
                {
                    var temp = tempList[i] as MicroVariableRowView;
                    if (temp == null)
                        continue;
                    if (temp == _lastSelectVar)
                        firstIndex = i;
                    if (temp == itemView)
                        lastIndex = i;
                }
                if (firstIndex > lastIndex)
                {
                    int temp = lastIndex;
                    lastIndex = firstIndex;
                    firstIndex = temp;
                }
                for (int i = firstIndex; i < lastIndex; i++)
                {
                    var temp = tempList[i] as MicroVariableRowView;
                    this.AddToSelection(temp.ItemView);
                }
            }
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            if (Event.current != null && Event.current.shift)
                return;
            var itemView = selectable as MicroVariableItemView;
            _lastSelectVar = itemView?.GetFirstAncestorOfType<MicroVariableRowView>();
        }
        public override void ClearSelection()
        {
            if (Event.current != null && Event.current.shift)
                return;
            _lastSelectVar = null;
            base.ClearSelection();
        }

        private void m_onDragUpdatedEvent(DragUpdatedEvent evt)
        {
            List<ISelectable> dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;
            if (dragData != null)
                dragging = dragData.OfType<MicroVariableItemView>().Any();
            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        private void m_onDragPerformEvent(DragPerformEvent evt)
        {
            if (evt == null) return;
        }

        public void Show()
        {
            m_updateVariableList();
        }

        /// <summary>
        /// 加号事件
        /// </summary>
        /// <param name="blackboard"></param>
        private void m_addItem(Blackboard blackboard)
        {
            var parameterType = new GenericMenu();
            foreach (var item in _owner.CategoryModel.VariableCategories)
            {
                parameterType.AddItem(new GUIContent(item.VarName), false, m_createVariable, item);
            }
            parameterType.ShowAsContext();
        }
        /// <summary>
        /// 创建一个变量
        /// </summary>
        /// <param name="userData"></param>
        private void m_createVariable(object userData)
        {
            VariableCategoryModel tempVar = userData as VariableCategoryModel;
            if (tempVar == null)
                return;
            string varName = m_getUniqueName("New" + tempVar.VarName);
            _owner.AddVariable(varName, tempVar.VarType);
        }

        public MicroVariableItemView AddVariableView(MicroVariableEditorInfo editorInfo)
        {
            MicroVariableItemView itemView = new MicroVariableItemView(_owner, editorInfo);
            var row = new MicroVariableRowView(itemView, new MicroVariablePropView(_owner, editorInfo));
            row.expanded = false;
            itemView.RegisterCallback<MouseDownEvent>(m_mouseDownEvent);
            contentContainer.Add(row);
            return itemView;
        }
        public void RemoveVariableView(MicroVariableEditorInfo editorInfo)
        {
            foreach (var item in contentContainer.Children().OfType<MicroVariableRowView>())
            {
                if (item.ItemView.editorInfo == editorInfo)
                {
                    this.AddToSelection(item.ItemView);
                    break;
                }
            }
        }
        /// <summary>
        /// 当有新增变量
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool m_varAdd(object args)
        {
            m_updateVariableList();
            return true;
        }
        private void m_updateVariableList()
        {
            contentContainer.Clear();
            foreach (var variable in _owner.editorInfo.Variables)
            {
                AddVariableView(variable);
            }
        }
        private string m_getUniqueName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (_owner.Target.Variables.Any(e => e.Name == name))
                name = uniqueName + (i++);
            return name;
        }

        public void Hide()
        {
        }
        public void Exit()
        {
        }
    }
}
