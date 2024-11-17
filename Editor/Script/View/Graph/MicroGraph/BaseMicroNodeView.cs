using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.BaseMicroGraphView;
using static MicroGraph.Editor.MicroGraphUtils;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 默认的微图节点视图
    /// </summary>
    public partial class BaseMicroNodeView
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroNodeView";
        private static readonly List<int> EMPTY_LIST = new List<int>();
        /// <summary>
        /// 忽略的字段
        /// </summary>
        private readonly string[] ignoreFields = { "_onlyId" };
        public Node view => _internalNodeView;
        /// <summary>
        /// 用户数据
        /// </summary>
        public object userData { get; set; }
        /// <summary>
        /// 当前微节点视图所对应的微节点
        /// </summary>
        public BaseMicroNode Target { get; private set; }
        internal MicroNodeEditorInfo editorInfo { get; private set; }
        public BaseMicroGraphView owner { get; private set; }

        private InternalNodeView _internalNodeView;
        private List<INodeFieldElement> _nextDrawElements = new List<INodeFieldElement>();
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => editorInfo.Title;
            set
            {
                if (!MicroGraphUtils.TitleValidity(value, MicroGraphUtils.EditorConfig.NodeTitleLength))
                {
                    _titleLabel.text = editorInfo.Title;
                    owner.owner.ShowNotification(new GUIContent("标题不合法"), NOTIFICATION_TIME);
                }
                else
                {
                    editorInfo.Title = value;
                    _titleLabel.text = value;
                }
            }
        }
        /// <summary>
        /// 最后的位置
        /// </summary>
        internal Vector2 LastPos { get; set; }
        /// <summary>
        /// 入端口
        /// </summary>
        public MicroPort Input { get; private set; }

        /// <summary>
        /// 出端口
        /// </summary>
        public MicroPort OutPut { get; private set; }
        private NodeTitleColorType _titleColor = NodeTitleColorType.Default;
        public NodeTitleColorType TitleColor
        {
            get => _titleColor;
            set
            {
                int v = (int)value;
                if (v < 0)
                {
                    _titleColor = category.NodeTitleColor;
                    editorInfo.TitleColor = -1;
                }
                else
                {
                    _titleColor = value;
                    editorInfo.TitleColor = v;
                }
                VariantColor color = m_getVariantColor(_titleColor);
                titleContainer.style.backgroundColor = color.s900.Fade(0.8f);
                titleContainer.style.borderTopColor = color.s300.Fade(0.8f);
                titleContainer.style.borderRightColor = color.s300.Fade(0.8f);
                titleContainer.style.borderLeftColor = color.s300.Fade(0.8f);
            }
        }
        /// <summary>
        /// 节点类型信息
        /// </summary>
        protected internal NodeCategoryModel category { get; private set; }
        /// <summary>
        /// 标题容器
        /// </summary>
        protected VisualElement titleContainer { get => view.titleContainer; }
        /// <summary>
        /// 内容容器
        /// </summary>
        protected VisualElement contentContainer { get; private set; }
        /// <summary>
        /// 节点图标
        /// </summary>
        protected Image nodeIcon => _nodeIcon;

        private List<INodeFieldElement> _nodeFieldElements = new List<INodeFieldElement>();
        /// <summary>
        /// 当前界面创建的所有NodeFieldElement
        /// </summary>
        protected internal List<INodeFieldElement> nodefieldElements => _nodeFieldElements;

        /// <summary>
        /// 所有节点
        /// </summary>
        private List<MicroPort> _microPorts = new List<MicroPort>();
        protected internal List<MicroPort> microPorts => _microPorts;

        private EditorLabelElement _titleLabel;
        /// <summary>
        /// 微节点布局
        /// </summary>
        private MicroNodeLayout _nodeLayer;
        /// <summary>
        /// 节点类型Icon
        /// </summary>
        private Image _nodeIcon;
        /// <summary>
        /// 加锁按钮
        /// </summary>
        private Button _lockButton;
#if MICRO_GRAPH_DEBUG
        /// <summary>
        /// 调试视图
        /// </summary>
        private MicroNodeDebuggerView _debuggerView;
#endif

        public BaseMicroNodeView()
        {
            _internalNodeView = new InternalNodeView(this);
            this.view.AddStyleSheet(STYLE_PATH);
            contentContainer = new VisualElement();
            contentContainer.name = "nodeContainer";
            contentContainer.AddToClassList("internal_node_container");
            this.view.topContainer.parent.Add(contentContainer);
            while (view.titleContainer.childCount > 0)
            {
                view.titleContainer[0].RemoveFromHierarchy();
            }
            _nodeIcon = new Image();
            _nodeIcon.AddToClassList("node_icon");
            this.titleContainer.Add(_nodeIcon);
            _titleLabel = new EditorLabelElement("");
            this.titleContainer.Add(_titleLabel);
            _titleLabel.onRename += onTitleRename;
            _lockButton = new Button(() =>
            {
                _lockButton.RemoveFromClassList(editorInfo.IsLock ? "node_state_lock" : "node_state_unlock");
                editorInfo.IsLock = !editorInfo.IsLock;
                _lockButton.AddToClassList(editorInfo.IsLock ? "node_state_lock" : "node_state_unlock");
                this.contentContainer.SetEnabled(!editorInfo.IsLock);
            });
            _lockButton.AddToClassList("node_icon");
            this.titleContainer.Add(_lockButton);
        }
    }
    //公共方法
    partial class BaseMicroNodeView
    {
        public void AddStyleSheet(string stylePath)
        {
            _internalNodeView.AddStyleSheet(stylePath);
        }
        public void RemoveStyleSheet(string stylePath)
        {
            _internalNodeView.RemoveStyleSheet(stylePath);
        }
        public void Add(VisualElement child)
        {
            if (child is MicroPort.InternalPort port)
                AddMicroPort(port);
            contentContainer.Add(child);
        }
        public void Add(INodeFieldElement child)
        {
            if (child.Port != null)
                AddMicroPort(child.Port);
            nodefieldElements.Add(child);
            contentContainer.Add(child.Root);
        }
        public void Insert(int index, VisualElement element)
        {
            contentContainer.Insert(index, element);
        }
        public void Remove(VisualElement element)
        {
            contentContainer.Remove(element);
        }
        public void RemoveAt(int index)
        {
            contentContainer.RemoveAt(index);
        }
        public void Clear()
        {
            contentContainer.Clear();
        }
        public void AddManipulator(IManipulator manipulator)
        {
            _internalNodeView.AddManipulator(manipulator);
        }
        public void RemoveManipulator(IManipulator manipulator)
        {
            _internalNodeView.AddManipulator(manipulator);
        }
        /// <summary>
        /// 画指定字段名的UI
        /// </summary>
        /// <param name="fieldName"></param>
        public void DrawUI(string fieldName)
        {
            if (!category.GetNodeFieldInfos().TryGetValue(fieldName, out FieldInfo fieldInfo))
            {
                Debug.LogWarning($"字段:[{fieldName}], 没有找到");
                return;
            }
            DrawUI(fieldInfo);
        }
        /// <summary>
        /// 画指定字段的UI
        /// </summary>
        /// <param name="fieldInfo"></param>
        public void DrawUI(FieldInfo fieldInfo)
        {
            _nextDrawElements.Clear();
            INodeFieldElement baseField = null;
            foreach (var item in fieldInfo.GetCustomAttributes())
            {
                if (item is NodeInputAttribute || item is NodeOutputAttribute)
                    continue;
                var elementType = MicroGraphProvider.GetNodeElementType(item.GetType());
                if (elementType != null)
                {
                    INodeFieldElement nodeElement = Activator.CreateInstance(elementType) as INodeFieldElement;
                    switch (nodeElement.ElementType)
                    {
                        case NodeFieldElementType.Basics:
                            baseField = nodeElement;
                            break;
                        case NodeFieldElementType.PreDecorate:
                            {
                                nodeElement.DrawElement(this, fieldInfo, PortDirEnum.None);
                                this.Add(nodeElement);
                            }
                            break;
                        case NodeFieldElementType.NextDecorate:
                            _nextDrawElements.Add(nodeElement);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (baseField == null)
            {
                var elementType = MicroGraphProvider.GetNodeElementType(fieldInfo.FieldType);
                if (elementType != null)
                {
                    baseField = Activator.CreateInstance(elementType) as INodeFieldElement;
                }
            }
            if (baseField == null)
                return;
            PortDirEnum dir = fieldInfo.FieldPortDir();
            if (dir == PortDirEnum.None)
            {
                baseField.DrawElement(this, fieldInfo, dir);
                this.Add(baseField);
            }
            else
            {
                if ((dir & PortDirEnum.In) == PortDirEnum.In)
                {
                    baseField.DrawElement(this, fieldInfo, PortDirEnum.In);
                    this.Add(baseField);
                }
                if ((dir & PortDirEnum.Out) == PortDirEnum.Out)
                {
                    if ((dir & PortDirEnum.In) == PortDirEnum.In)
                    {
                        baseField = Activator.CreateInstance(baseField.GetType()) as INodeFieldElement;
                    }
                    baseField.DrawElement(this, fieldInfo, PortDirEnum.Out);
                    this.Add(baseField);
                }
            }
            foreach (var item in _nextDrawElements)
            {
                item.DrawElement(this, fieldInfo, PortDirEnum.None);
                this.Add(item);
            }
        }

        /// <summary>
        /// 画基础节点的连线
        /// </summary>
        public void DrawBaseLink()
        {
            if (this.OutPut == null)
            {
                this.Target.Childs.Clear();
                return;
            }
            var childs = this.Target.Childs;
            DrawBaseLink(this.OutPut, ref childs);
        }
        /// <summary>
        /// 画基础节点的连线
        /// </summary>
        /// <param name="port"></param>
        /// <param name="nodeIds"></param>
        public void DrawBaseLink(MicroPort port, ref List<int> nodeIds)
        {
            List<int> tempList = nodeIds.ToList();
            foreach (var item in tempList)
            {
                Node node = owner.GetElement<Node>(item);
                if (node == null)
                {
                    nodeIds.Remove(item);
                    continue;
                }
                switch (node)
                {
                    case InternalNodeView nv:
                        if (nv.nodeView.Input == null)
                            nodeIds.Remove(item);
                        else
                            port.ConnectWithoutNotify(nv.nodeView.Input);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 画元素的连线
        /// </summary>
        public void DrawElementLink()
        {
            for (int i = _nodeFieldElements.Count - 1; i >= 0; i--)
                _nodeFieldElements[i].DrawLink(this);
        }
        ///// <summary>
        ///// 画变量连线
        ///// </summary>
        //protected void DrawVarLink()
        //{
        //    List<MicroVariableEdge> varEdges = Target.VariableEdges.ToList();
        //    foreach (MicroVariableEdge item in varEdges)
        //    {
        //        DrawVarLink(item);
        //    }
        //}
        /// <summary>
        /// 画变量连线
        /// </summary>
        public void DrawVarLink(MicroVariableEdge varEdge)
        {
            Node node = owner.GetElement<Node>(varEdge.nodeId);
            if (node == null)
            {
                Target.VariableEdges.Remove(varEdge);
                return;
            }
            var port = this.microPorts.FirstOrDefault(a => a.key == varEdge.fieldName && a.IsInput == varEdge.isInput);
            if (port == null)
            {
                Target.VariableEdges.Remove(varEdge);
                return;
            }
            switch (node)
            {
                case MicroVariableNodeView.InternalNodeView varNodeView:
                    port.ConnectWithoutNotify(varEdge.isInput ? varNodeView.nodeView.OutPut : varNodeView.nodeView.Input);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 添加一个变量线数据
        /// <para>不会添加连线，只会添加数据</para>
        /// </summary>
        /// <param name="port"></param>
        /// <param name="nodeView"></param>
        public MicroVariableEdge AddVariableEdge(MicroPort mine, MicroPort target)
        {
            MicroVariableEdge edgeData = new MicroVariableEdge();
            edgeData.isInput = mine.IsInput;
            Node node = target.view.GetFirstAncestorOfType<Node>();
            if (node is MicroVariableNodeView.InternalNodeView varNode)
            {
                edgeData.nodeId = varNode.nodeView.editorInfo.NodeId;
                edgeData.fieldName = mine.key;
                edgeData.varName = varNode.nodeView.Target.Name;
                this.Target.VariableEdges.Add(edgeData);
                return edgeData;
            }
            else
            {
                owner.owner.ShowNotification(new GUIContent("字段节点暂不支持非MicroVariableNodeView类型"), NOTIFICATION_TIME);
                mine.DisonnectWithoutNotify(target);
                return null;
            }
        }

        /// <summary>
        /// 删除一个变量线数据
        /// <para>不会删除连线，只会删除数据</para>
        /// </summary>
        /// <param name="port"></param>
        /// <param name="nodeView"></param>
        public void RemoveVariableEdge(MicroPort mine, MicroPort target)
        {
            int nodeId = 0;
            string fieldName = "";
            string varName = "";
            Node node = target.view.GetFirstAncestorOfType<Node>();
            if (node is MicroVariableNodeView.InternalNodeView varNode)
            {
                nodeId = varNode.nodeView.editorInfo.NodeId;
                fieldName = mine.key;
                varName = varNode.nodeView.Target.Name;
            }
            else
            {
                owner.owner.ShowNotification(new GUIContent("字段节点暂不支持非MicroVariableNodeView类型"), NOTIFICATION_TIME);
                mine.DisonnectWithoutNotify(target);
                return;
            }
            Target.VariableEdges.RemoveAll(a =>
            {
                if (a.nodeId == nodeId && a.isInput == mine.IsInput && a.fieldName == fieldName && a.varName == varName)
                    return true;
                return false;
            });
        }
        /// <summary>
        /// 手动添加一个端口
        /// </summary>
        public void AddMicroPort(MicroPort port)
        {
            if (!microPorts.Contains(port))
                microPorts.Add(port);
        }
        /// <summary>
        /// 手动获取一个端口
        /// </summary>
        /// <param name="key">端口的唯一Key</param>
        /// <param name="isInput">是否为输入端口</param>
        /// <returns></returns>
        public virtual MicroPort GetMicroPort(string key, bool isInput = false)
        {
            return _microPorts.FirstOrDefault(a => a.key == key && a.IsInput == isInput);
        }
        /// <summary>
        /// 手动删除一个端口
        /// </summary>
        /// <returns></returns>
        public virtual bool RemoveMicroPort(string key, bool isInput = false)
        {
            return _microPorts.RemoveAll(a => a.key == key && a.IsInput == isInput) > 0;
        }
        /// <summary>
        /// 手动删除一个端口
        /// </summary>
        /// <returns></returns>
        public virtual bool RemoveMicroPort(MicroPort port)
        {
            return _microPorts.Remove(port);
        }
        /// <summary>
        /// 获取所有父节点
        /// </summary>
        /// <returns></returns>
        public List<int> GetParents()
        {
            if (Input == null)
                return EMPTY_LIST;
            List<int> parents = new List<int>();
            foreach (var item in Input.view.connections)
            {
                if (item.output.node is not InternalNodeView nodeView)
                    continue;
                parents.Add(nodeView.nodeView.Target.OnlyId);
            }
            return parents;
        }
        /// <summary>
        /// 获取对应的节点元素
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public IEnumerable<INodeFieldElement> GetNodeFieldElement(string fieldName) => _nodeFieldElements.Where(a => a.Field != null && a.Field.Name == fieldName);
        /// <summary>
        /// 获取对应的节点元素
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public IEnumerable<INodeFieldElement> GetNodeFieldElement(FieldInfo fieldInfo) => _nodeFieldElements.Where(a => a.Field == fieldInfo);
        /// <summary>
        /// 获取当前视图中的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public T GetGraphViewElement<T>(int onlyId) where T : GraphElement => this.owner.GetElement<T>(onlyId);

    }
    //internal 方法
    partial class BaseMicroNodeView
    {
        internal void Initialize(BaseMicroGraphView graphView, MicroNodeEditorInfo editorInfo, NodeCategoryModel nodeCategory)
        {
            owner = graphView;
            this.editorInfo = editorInfo;
            Target = editorInfo.Target;
            category = nodeCategory;
            Title = editorInfo.Title;
            _nodeLayer = category.IsHorizontal ? new HorizontalMicroNodeLayout(this) : new VerticalMicroNodeLayout(this);
            _nodeLayer.NodeLayout();
            if ((category.PortDir & PortDirEnum.In) == PortDirEnum.In)
            {
                Input = new MicroPort(MicroPortType.BaseNodePort, _nodeLayer.orientation, Direction.Input);
                Input.type = this.Target.GetType();
                Input.onConnect += onBaseInputConnect;
                Input.onDisconnect += onBaseInputDisconnect;
                Input.key = "__BaseIn";
                _microPorts.Add(Input);
                this.view.inputContainer.Add(Input);
            }
            else
                this.view.inputContainer.RemoveFromHierarchy();
            if ((category.PortDir & PortDirEnum.Out) == PortDirEnum.Out)
            {
                OutPut = new MicroPort(MicroPortType.BaseNodePort, _nodeLayer.orientation, Direction.Output);
                OutPut.type = this.Target.GetType();
                OutPut.onCanLink += onBasePortCanConnect;
                OutPut.onConnect += onBaseOutputConnect;
                OutPut.onDisconnect += onBaseOutputDisconnect;
                OutPut.key = "__BaseOut";
                _microPorts.Add(OutPut);
                this.view.outputContainer.Add(OutPut);
            }
            else
                this.view.outputContainer.RemoveFromHierarchy();
            _lockButton.AddToClassList(editorInfo.IsLock ? "node_state_lock" : "node_state_unlock");
            _nodeIcon.AddToClassList("node_type_" + category.NodeType.ToString().ToLower());
            _nodeIcon.sprite = Resources.Load<Sprite>("__MicroGraph/Texture/Node/node_" + category.NodeType.ToString().ToLower());
            this.contentContainer.SetEnabled(!editorInfo.IsLock);
            if (nodeCategory.MinWidth > 0)
                this.view.style.minWidth = nodeCategory.MinWidth;
            onInit();
            this.DrawUI();
            this.view.SetPosition(new Rect(editorInfo.Pos, Vector2.one));
            this.LastPos = editorInfo.Pos;
            this.TitleColor = (NodeTitleColorType)editorInfo.TitleColor;
#if MICRO_GRAPH_DEBUG
            graphView.listener.AddListener(MicroGraphEventIds.DEBUGGER_LOCAL_GRAPH_STATE_CHANGED, m_onGraphDebuggerChanged);
#endif
        }
        /// <summary>
        /// 内部退出
        /// </summary>
        internal void InternalExit()
        {
#if MICRO_GRAPH_DEBUG
            owner.listener.RemoveListener(MicroGraphEventIds.DEBUGGER_LOCAL_GRAPH_STATE_CHANGED, m_onGraphDebuggerChanged);
            _debuggerView?.Disable();
#endif
            onExit();

        }

    }
    //protected 方法
    partial class BaseMicroNodeView
    {
        protected virtual void buildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //evt.menu.AppendAction($"创建{title}", m_createMicroGraph, DropdownMenuAction.AlwaysEnabled);
            int selectCount = this.owner.View.selection.OfType<Node>().Count();
            if (selectCount > 1)
            {
                evt.menu.AppendAction("添加模板", (e) => this.owner.AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
                evt.StopPropagation();
            }
            if (!this.view.selected)
            {
                evt.StopPropagation();
                return;
            }
            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript, this.GetType() == typeof(BaseMicroNodeView) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
            evt.menu.AppendSeparator();
            if (selectCount > 1)
            {
                evt.menu.AppendAction("添加模板", (e) => this.owner.AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
            }
            else
            {
                string msg = editorInfo.TitleColor < 0 ? "默认颜色" : ((NodeTitleColorType)editorInfo.TitleColor).ToString();
                evt.menu.AppendAction($"当前颜色: {msg}", null, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("修改标题颜色/默认颜色/当前节点", (a) => TitleColor = (NodeTitleColorType)a.userData, DropdownMenuAction.AlwaysEnabled, (NodeTitleColorType)(-1));
                evt.menu.AppendAction("修改标题颜色/默认颜色/同类型节点", a => m_changeAllTitleColor((NodeTitleColorType)a.userData), DropdownMenuAction.AlwaysEnabled, (NodeTitleColorType)(-1));
                foreach (var item in nodeColorTypes)
                {
                    string menu = "修改标题颜色/";
                    menu += item.ToString();
                    evt.menu.AppendAction(menu + "/当前节点", (a) => TitleColor = (NodeTitleColorType)a.userData, DropdownMenuAction.AlwaysEnabled, item);
                    evt.menu.AppendAction(menu + "/同类型节点", a => m_changeAllTitleColor((NodeTitleColorType)a.userData), DropdownMenuAction.AlwaysEnabled, item);
                }

                evt.menu.AppendSeparator();
            }
            evt.menu.AppendAction("删除", onDeleteNodeView, this.owner.CategoryModel.IsUniqueNode(this.Target.GetType()) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);

            evt.StopPropagation();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void onInit() { }
        /// <summary>
        /// 退出
        /// </summary>
        protected virtual void onExit() { }
        /// <summary>
        /// 出端口是否可以连接
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>        
        protected virtual bool onBasePortCanConnect(MicroPort mine, MicroPort target)
        {
            MicroGroupView groupView = this.view.GetContainingScope() as MicroGroupView;
            if (groupView == null || !groupView.editorInfo.IsPackage)
                return target.portType == MicroPortType.BaseNodePort;
            if (target.portType == MicroPortType.PackagePort)
                return true;
            if (target.view.node == null)
                return false;
            return target.view.node.GetContainingScope() == groupView;
        }

        /// <summary>
        /// 出端口连接回调
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void onBaseOutputConnect(MicroPort mine, MicroPort target)
        {
            switch (target.view.node)
            {
                case BaseMicroNodeView.InternalNodeView nodeView:
                    this.Target.Childs.Add(nodeView.nodeView.Target.OnlyId);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 入端口连接回调
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void onBaseInputConnect(MicroPort mine, MicroPort target) { }

        /// <summary>
        /// 出端口断开连接回调
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void onBaseOutputDisconnect(MicroPort mine, MicroPort target)
        {
            switch (target.view.node)
            {
                case BaseMicroNodeView.InternalNodeView nodeView:
                    this.Target.Childs.Remove(nodeView.nodeView.Target.OnlyId);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 入端口断开连接回调
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void onBaseInputDisconnect(MicroPort mine, MicroPort target) { }

        /// <summary>
        /// 标题修改回调
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        protected virtual void onTitleRename(string oldName, string newName)
        {
            Title = newName;
        }

        /// <summary>
        /// 绘制UI
        /// </summary>
        protected internal virtual void DrawUI()
        {
            foreach (KeyValuePair<string, FieldInfo> item in category.GetNodeFieldInfos())
            {
                FieldInfo fieldInfo = item.Value;
                DrawUI(fieldInfo);
            }
        }

        /// <summary>
        /// 画已经连接的线
        /// </summary>
        protected internal virtual void DrawLink()
        {
            DrawBaseLink();
            DrawElementLink();
            //DrawVarLink();
        }

        /// <summary>
        /// 查看节点视图代码
        /// </summary>
        /// <param name="action"></param>
        protected void onOpenNodeViewScript(DropdownMenuAction action)
        {
            string[] guids = AssetDatabase.FindAssets(this.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == this.GetType())
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }
        /// <summary>
        /// 查看节点代码
        /// </summary>
        /// <param name="action"></param>
        protected void onOpenNodeScript(DropdownMenuAction action)
        {
            string[] guids = AssetDatabase.FindAssets(this.Target.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == this.Target.GetType())
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }
        protected void onDeleteNodeView(DropdownMenuAction action)
        {
            this.owner.View.DeleteElements(new GraphElement[] { this.view });
        }
    }
    //private
    partial class BaseMicroNodeView
    {
        private readonly static Color s_defaultColor = new Color(0.247f, 0.247f, 0.247f, 0.804f);
        private readonly static VariantColor s_defaultVariantColor = new VariantColor(s_defaultColor);

        protected readonly static List<NodeTitleColorType> nodeColorTypes = new List<NodeTitleColorType>
        {
            NodeTitleColorType.Default,
            NodeTitleColorType.Slate,
            NodeTitleColorType.Gray,
            NodeTitleColorType.Zinc,
            NodeTitleColorType.Neutral,
            NodeTitleColorType.Stone,
            NodeTitleColorType.Red,
            NodeTitleColorType.Orange,
            NodeTitleColorType.Amber,
            NodeTitleColorType.Yellow,
            NodeTitleColorType.Lime,
            NodeTitleColorType.Green,
            NodeTitleColorType.Emerald,
            NodeTitleColorType.Teal,
            NodeTitleColorType.Cyan,
            NodeTitleColorType.Sky,
            NodeTitleColorType.Blue,
            NodeTitleColorType.Indigo,
            NodeTitleColorType.Violet,
            NodeTitleColorType.Purple,
            NodeTitleColorType.Fuchsia,
            NodeTitleColorType.Pink,
            NodeTitleColorType.Rose
        };

        /// <summary>
        /// 获取虚拟颜色
        /// </summary>
        /// <param name="colorType"></param>
        /// <returns></returns>
        private VariantColor m_getVariantColor(NodeTitleColorType colorType)
        {
            switch (colorType)
            {
                case NodeTitleColorType.Default:
                    return s_defaultVariantColor;
                case NodeTitleColorType.Slate:
                    return TailwindPalette.Slate;
                case NodeTitleColorType.Gray:
                    return TailwindPalette.Gray;
                case NodeTitleColorType.Zinc:
                    return TailwindPalette.Zinc;
                case NodeTitleColorType.Neutral:
                    return TailwindPalette.Neutral;
                case NodeTitleColorType.Stone:
                    return TailwindPalette.Stone;
                case NodeTitleColorType.Red:
                    return TailwindPalette.Red;
                case NodeTitleColorType.Orange:
                    return TailwindPalette.Orange;
                case NodeTitleColorType.Amber:
                    return TailwindPalette.Amber;
                case NodeTitleColorType.Yellow:
                    return TailwindPalette.Yellow;
                case NodeTitleColorType.Lime:
                    return TailwindPalette.Lime;
                case NodeTitleColorType.Green:
                    return TailwindPalette.Green;
                case NodeTitleColorType.Emerald:
                    return TailwindPalette.Emerald;
                case NodeTitleColorType.Teal:
                    return TailwindPalette.Teal;
                case NodeTitleColorType.Cyan:
                    return TailwindPalette.Cyan;
                case NodeTitleColorType.Sky:
                    return TailwindPalette.Sky;
                case NodeTitleColorType.Blue:
                    return TailwindPalette.Blue;
                case NodeTitleColorType.Indigo:
                    return TailwindPalette.Indigo;
                case NodeTitleColorType.Violet:
                    return TailwindPalette.Violet;
                case NodeTitleColorType.Purple:
                    return TailwindPalette.Purple;
                case NodeTitleColorType.Fuchsia:
                    return TailwindPalette.Fuchsia;
                case NodeTitleColorType.Pink:
                    return TailwindPalette.Pink;
                case NodeTitleColorType.Rose:
                    return TailwindPalette.Rose;
            }
            return EditorPalette.VariantSurfaceColorFixed;
        }

        /// <summary>
        /// 获取具体颜色
        /// </summary>
        /// <param name="colorType"></param>
        /// <returns></returns>
        private Color m_getColor(NodeTitleColorType colorType)
        {
            switch (colorType)
            {
                case NodeTitleColorType.Default:
                    return s_defaultVariantColor.s500;
                case NodeTitleColorType.Slate:
                    return TailwindPalette.Slate.s500;
                case NodeTitleColorType.Gray:
                    return TailwindPalette.Gray.s500;
                case NodeTitleColorType.Zinc:
                    return TailwindPalette.Zinc.s500;
                case NodeTitleColorType.Neutral:
                    return TailwindPalette.Neutral.s500;
                case NodeTitleColorType.Stone:
                    return TailwindPalette.Stone.s500;
                case NodeTitleColorType.Red:
                    return TailwindPalette.Red.s500;
                case NodeTitleColorType.Orange:
                    return TailwindPalette.Orange.s500;
                case NodeTitleColorType.Amber:
                    return TailwindPalette.Amber.s500;
                case NodeTitleColorType.Yellow:
                    return TailwindPalette.Yellow.s500;
                case NodeTitleColorType.Lime:
                    return TailwindPalette.Lime.s500;
                case NodeTitleColorType.Green:
                    return TailwindPalette.Green.s500;
                case NodeTitleColorType.Emerald:
                    return TailwindPalette.Emerald.s500;
                case NodeTitleColorType.Teal:
                    return TailwindPalette.Teal.s500;
                case NodeTitleColorType.Cyan:
                    return TailwindPalette.Cyan.s500;
                case NodeTitleColorType.Sky:
                    return TailwindPalette.Sky.s500;
                case NodeTitleColorType.Blue:
                    return TailwindPalette.Blue.s500;
                case NodeTitleColorType.Indigo:
                    return TailwindPalette.Indigo.s500;
                case NodeTitleColorType.Violet:
                    return TailwindPalette.Violet.s500;
                case NodeTitleColorType.Purple:
                    return TailwindPalette.Purple.s500;
                case NodeTitleColorType.Fuchsia:
                    return TailwindPalette.Fuchsia.s500;
                case NodeTitleColorType.Pink:
                    return TailwindPalette.Pink.s500;
                case NodeTitleColorType.Rose:
                    return TailwindPalette.Rose.s500;
            }
            return EditorPalette.BackgroundColor;
        }

        /// <summary>
        /// 修改当前类型节点标题颜色
        /// </summary>
        /// <param name="action"></param>
        private void m_changeAllTitleColor(NodeTitleColorType colorType)
        {
            foreach (var item in owner.View.nodes.OfType<BaseMicroNodeView>())
            {
                if (item.Target.GetType() == this.Target.GetType())
                    item.TitleColor = colorType;
            }
        }

#if MICRO_GRAPH_DEBUG
        private bool m_onGraphDebuggerChanged(object args)
        {
            if (owner.DebuggerState == BaseMicroGraphView.MicroGraphDebuggerState.None)
            {
                if (_debuggerView != null)
                {
                    _debuggerView.Disable();
                }
                return true;
            }
            else
            {
                if (_debuggerView == null)
                    _debuggerView = new MicroNodeDebuggerView();
                _debuggerView.Initialize(this);
            }
            return true;
        }
#endif
    }
    //Class
    partial class BaseMicroNodeView
    {
        internal class InternalNodeView : Node
        {
            internal readonly BaseMicroNodeView nodeView;
            internal InternalNodeView(BaseMicroNodeView baseNodeView)
            {
                this.AddToClassList("internal_node");
                this.topContainer.AddToClassList("internal_node_top");
                nodeView = baseNodeView;
            }

            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                nodeView.buildContextualMenu(evt);
            }
            public override void SetPosition(Rect newPos)
            {
                if (nodeView.editorInfo.IsLock)
                    return;
#if MICRO_GRAPH_DEBUG
                if (nodeView.owner.DebuggerState == MicroGraphDebuggerState.Attach)
                    return;
#endif

                base.SetPosition(newPos);
                nodeView.editorInfo.Pos = newPos.position;
            }

            public static implicit operator BaseMicroNodeView(InternalNodeView node)
            {
                return node.nodeView;
            }
            public static implicit operator InternalNodeView(BaseMicroNodeView node)
            {
                return node._internalNodeView;
            }
        }
        public static implicit operator Node(BaseMicroNodeView nodeView)
        {
            return nodeView.view;
        }
        public static implicit operator BaseMicroNodeView(Node node)
        {
            if (node is InternalNodeView nodeView)
                return nodeView.nodeView;
            return null;
        }
    }
    public class BaseMicroNodeView<TNode> : BaseMicroNodeView where TNode : BaseMicroNode
    {
        public TNode node => Target as TNode;
        public void DrawUI<TField>(Expression<Func<TNode, TField>> expression)
        {
            string fieldName = MicroGraphUtils.GetMemberName(expression);
            DrawUI(fieldName);
        }
    }
}
