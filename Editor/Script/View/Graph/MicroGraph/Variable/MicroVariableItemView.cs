using MicroGraph.Runtime;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroGraphUtils;

namespace MicroGraph.Editor
{
    internal sealed class MicroVariableItemView : BlackboardField
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroVariableItem";
        private BaseMicroGraphView _owner;
        public BaseMicroGraphView owner => _owner;
        private MicroVariableEditorInfo _editorInfo;
        internal MicroVariableEditorInfo editorInfo { get { return _editorInfo; } }
        private VisualElement _varIcon;
        private Label _varLabel;
        public MicroVariableItemView(BaseMicroGraphView graphView, MicroVariableEditorInfo editorInfo) : base(null, editorInfo.Name, editorInfo.Target.GetDisplayName())
        {
            this.AddStyleSheet(STYLE_PATH);
            this._owner = graphView;
            this._editorInfo = editorInfo;
            this.tooltip = editorInfo.Name;
            _varIcon = this.Q("icon");
            _varIcon.style.backgroundColor = MicroGraphUtils.GetColor(editorInfo.Target.GetValueType());
            _varIcon.visible = true;
            _varLabel = new Label();
            _varLabel.AddToClassList("var_count_label");
            _varLabel.text = graphView.editorInfo.VariableNodes.Count(a => a.Name == _editorInfo.Name).ToString();
            _varIcon.Add(_varLabel);
            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) =>
            {
                text = e.newValue;
                this.tooltip = text;
            });
            (this.Q("textField") as TextField).RegisterCallback<FocusOutEvent>((e) =>
            {
                if (m_checkVerifyVarName(text))
                {
                    BaseMicroVariable variable = graphView.Target.Variables.FirstOrDefault(a => a.Name == text);
                    if (variable != null && variable != _editorInfo.Target)
                    {
                        text = this._editorInfo.Name;
                        graphView.owner.ShowNotification(new GUIContent("变量名必须是唯一的"));
                    }
                    else
                    {
                        string oldName = this._editorInfo.Name;
                        this._editorInfo.Name = text;
                        this._editorInfo.Target.Name = text;
                        this.tooltip = text;
                        this._owner.listener.OnEvent(MicroGraphEventIds.VAR_MODIFY, new VarModifyEventArgs() { oldVarName = oldName, var = this._editorInfo.Target });
                    }
                }
                else
                {
                    text = this._editorInfo.Name;
                }
            });
            this._owner.listener.AddListener(MicroGraphEventIds.VAR_MODIFY, m_varModify);
            this._owner.listener.AddListener(MicroGraphEventIds.VAR_NODE_MODIFY, m_varNodeModify);
        }

        public void OnDestory()
        {
            this._owner.listener.RemoveListener(MicroGraphEventIds.VAR_MODIFY, m_varModify);
            this._owner.listener.RemoveListener(MicroGraphEventIds.VAR_NODE_MODIFY, m_varNodeModify);
        }

        public override bool IsRenamable()
        {
            if (!_editorInfo.CanRename)
            {
                _owner.owner.ShowNotification(new GUIContent("该变量不允许改名"), NOTIFICATION_TIME);
            }
            return base.IsRenamable() && _editorInfo.CanRename;
        }
        private bool m_varNodeModify(object args)
        {
            _varLabel.text = _owner.editorInfo.VariableNodes.Count(a => a.Name == _editorInfo.Name).ToString();
            return true;
        }
        private bool m_varModify(object args)
        {
            if (args is VarModifyEventArgs varModify)
            {
                if (varModify.var == this._editorInfo.Target)
                {
                    this.editorInfo.Name = varModify.var.Name;
                    this.text = this._editorInfo.Target.Name;
                }
                return true;
            }
            return false;
        }

        private bool m_checkVerifyVarName(string varName)
        {
            varName = varName.Trim();
            if (string.IsNullOrWhiteSpace(varName))
            {
                _owner.owner.ShowNotification(new GUIContent("变量名不能为空"), NOTIFICATION_TIME);
                return false;
            }
            char[] strs = varName.ToArray();
            if (strs.Length > 20)
            {
                _owner.owner.ShowNotification(new GUIContent("变量名不能超过20个字符"), NOTIFICATION_TIME);
                return false;
            }
            bool result = true;
            int length = 0;
            while (length < strs.Length)
            {
                char c = strs[length];
                if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && c != '_')
                {
                    if (length == 0)
                    {
                        result = false;
                        goto End;
                    }
                    else if (c < '0' || c > '9')
                    {
                        result = false;
                        goto End;
                    }

                }
                length++;
            }
        End: if (!result)
            {
                _owner.owner.ShowNotification(new GUIContent("变量名不合法"), NOTIFICATION_TIME);
            }
            return result;
        }

        protected override void BuildFieldContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_editorInfo.CanRename)
            {
                evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            }
            if (_editorInfo.CanDelete)
            {
                evt.menu.AppendAction("删除", (a) =>
                {
                    _owner.View.DeleteElements(new[] { this });
                }, DropdownMenuAction.AlwaysEnabled);
            }
            evt.StopPropagation();
        }
    }
}
