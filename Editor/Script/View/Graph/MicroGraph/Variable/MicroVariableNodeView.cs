using MicroGraph.Runtime;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroGraphUtils;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 变量Node节点
    /// </summary>
    public sealed partial class MicroVariableNodeView
    {
        private const MicroPortType CAN_LINK_PORT_TYPE = MicroPortType.VarPort;
        private const string STYLE_PATH = "Uss/MicroGraph/MicroVariableNodeView";
        public Node view => _internalNodeView;
        public BaseMicroVariable Target { get; private set; }
        public int NodeId => editorInfo == null ? 0 : editorInfo.NodeId;
        internal MicroVariableNodeEditorInfo editorInfo { get; private set; }
        public BaseMicroGraphView owner { get; private set; }
        /// <summary>
        /// 入端口
        /// </summary>
        public MicroPort Input { get; private set; }

        /// <summary>
        /// 出端口
        /// </summary>
        public MicroPort OutPut { get; private set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get => _titleLabel.text; private set { _titleLabel.text = value; } }
        /// <summary>
        /// 最后的位置
        /// </summary>
        internal Vector2 LastPos { get; set; }

        private EditorLabelElement _titleLabel;

        private InternalNodeView _internalNodeView;

        public MicroVariableNodeView()
        {
            _internalNodeView = new InternalNodeView(this);
            this.view.AddStyleSheet(STYLE_PATH);
        }
    }
    //internal
    partial class MicroVariableNodeView
    {
        internal void Initialize(BaseMicroGraphView graphView, MicroVariableNodeEditorInfo editorInfo)
        {
            this.owner = graphView;
            this.editorInfo = editorInfo;
            this.Target = editorInfo.EditorInfo.Target;
            m_initNodeView();
            this.view.SetPosition(new Rect(editorInfo.Pos, Vector2.one));
            this.LastPos = editorInfo.Pos;
            this.Title = this.Target.Name;
            this.owner.listener.AddListener(MicroGraphEventIds.VAR_MODIFY, m_varModify);
        }

        internal void OnDestory()
        {
            this.owner.listener.RemoveListener(MicroGraphEventIds.VAR_MODIFY, m_varModify);
        }
    }

    //private
    partial class MicroVariableNodeView
    {
        internal void m_initNodeView()
        {
            this._internalNodeView.AddToClassList("internal_variable_node");
            //移除右上角折叠按钮
            this._internalNodeView.titleButtonContainer.RemoveFromHierarchy();
            //view.topContainer.style.height = 24;
            this._internalNodeView.inputContainer.RemoveFromHierarchy();
            this._internalNodeView.outputContainer.RemoveFromHierarchy();
            var titleLabel = this._internalNodeView.titleContainer.Q("title-label");
            titleLabel.RemoveFromHierarchy();
            _titleLabel = new EditorLabelElement();
            this._internalNodeView.titleContainer.Add(this._internalNodeView.inputContainer);
            this._internalNodeView.titleContainer.Add(_titleLabel);
            this._internalNodeView.titleContainer.Add(this._internalNodeView.outputContainer);
            if (editorInfo.EditorInfo.CanAssign)
            {
                Input = new MicroPort(MicroPortType.BaseVarPort, Orientation.Horizontal, Direction.Input);
                this._internalNodeView.inputContainer.Add(Input);
                Input.type = this.Target.GetValueType();
                Input.view.portColor = MicroGraphUtils.GetColor(Input.type);
            }
            else
            {
                this._internalNodeView.inputContainer.style.width = 33;
            }
            OutPut = new MicroPort(MicroPortType.BaseVarPort, Orientation.Horizontal, Direction.Output);
            this._internalNodeView.outputContainer.Add(OutPut);
            OutPut.type = this.Target.GetValueType();
            OutPut.view.portColor = MicroGraphUtils.GetColor(OutPut.type);
            OutPut.onCanLink += onPortCanConnect;
            var contents = this._internalNodeView.Q("contents");
            contents.RemoveFromHierarchy();
            _titleLabel.onRename += titleLabel_onRename;
        }
        private bool m_varModify(object args)
        {
            if (args is VarModifyEventArgs varModify)
            {
                if (varModify.var == this.Target)
                {
                    this.editorInfo.Name = varModify.var.Name;
                    this.Title = this.editorInfo.Name;
                }
                return true;
            }
            return false;
        }
        private void titleLabel_onRename(string arg1, string arg2)
        {
            if (!editorInfo.EditorInfo.CanRename)
            {
                Title = this.editorInfo.Name;
                owner.owner.ShowNotification(new GUIContent("该变量不允许改名"), NOTIFICATION_TIME);
                return;
            }
            if (m_checkVerifyVarName(arg2))
            {
                BaseMicroVariable variable = owner.Target.Variables.FirstOrDefault(a => a.Name == arg2);
                if (variable != null && variable != this.Target)
                {
                    owner.owner.ShowNotification(new GUIContent("变量名必须是唯一的"), NOTIFICATION_TIME);
                }
                else
                {
                    string oldName = this.editorInfo.Name;
                    this.editorInfo.Name = arg2;
                    this.Target.Name = arg2;
                    this.owner.listener.OnEvent(MicroGraphEventIds.VAR_MODIFY, new VarModifyEventArgs() { oldVarName = oldName, var = this.Target });
                }
            }
            else
            {
                Title = this.editorInfo.Name;
            }
        }

        private bool onPortCanConnect(MicroPort mine, MicroPort target)
        {
            if ((target.portType & CAN_LINK_PORT_TYPE) > MicroPortType.None)
            {
                if (target.type == null)
                    return false;
                if (mine.type == target.type || mine.type.IsSubclassOf(target.type))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 检查变量名是否合法
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        private bool m_checkVerifyVarName(string varName)
        {
            varName = varName.Trim();
            if (string.IsNullOrWhiteSpace(varName))
            {
                owner.owner.ShowNotification(new GUIContent("变量名不能为空"), NOTIFICATION_TIME);
                return false;
            }
            char[] strs = varName.ToArray();
            if (strs.Length > 20)
            {
                owner.owner.ShowNotification(new GUIContent("变量名不能超过20个字符"), NOTIFICATION_TIME);
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
                owner.owner.ShowNotification(new GUIContent("变量名不合法"));
            }
            return result;
        }
    }
    //Class
    partial class MicroVariableNodeView
    {
        internal class InternalNodeView : Node
        {
            internal readonly MicroVariableNodeView nodeView;

            internal InternalNodeView(MicroVariableNodeView variableNodeView)
            {
                this.nodeView = variableNodeView;

            }
            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                if (nodeView.owner.View.selection.OfType<Node>().Count() > 1)
                    evt.menu.AppendAction("添加模板", (e) => nodeView.owner.AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
                evt.StopPropagation();
            }
            public override void SetPosition(Rect newPos)
            {
                base.SetPosition(newPos);
                nodeView.editorInfo.Pos = newPos.position;
            }

            public static implicit operator MicroVariableNodeView(InternalNodeView node)
            {
                return node.nodeView;
            }
            public static implicit operator InternalNodeView(MicroVariableNodeView node)
            {
                return node._internalNodeView;
            }

        }
        public static implicit operator Node(MicroVariableNodeView nodeView)
        {
            return nodeView.view;
        }
        public static implicit operator MicroVariableNodeView(Node node)
        {
            if (node is InternalNodeView nodeView)
                return nodeView.nodeView;
            return null;
        }
    }
}
