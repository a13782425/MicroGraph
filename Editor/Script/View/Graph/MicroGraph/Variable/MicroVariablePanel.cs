using MicroGraph.Runtime;
using System;
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
    [Obsolete("已经弃用,请使用MicroVariableControlSubView", true)]
    internal sealed class MicroVariablePanel : Blackboard
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroVariablePanel";
        private BaseMicroGraphView _owner;
        public MicroVariablePanel(BaseMicroGraphView graph) : base(graph)
        {
            this.AddStyleSheet(STYLE_PATH);
            this.AddToClassList("microvariablepanel");
            _owner = graph;
            title = "自定义变量";
            scrollable = true;
            ScrollView scrollView = this.Q<ScrollView>();
            scrollView.mode = ScrollViewMode.Vertical;
            this.Q("subTitleLabel").RemoveFromHierarchy();
            this.Q<Resizer>().RemoveFromHierarchy();
            this.style.position = Position.Relative;
            this.addItemRequested += m_addItem;
            //_owner.listener.AddListener(GraphEventId.VAR_ADD, m_varAdd);
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
    }
}
