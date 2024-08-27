using MicroGraph.Runtime;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 节点字段元素类型
    /// </summary>
    public enum NodeFieldElementType
    {
        /// <summary>
        /// 基础的类型，有In和Out，替换原本的类型渲染
        /// </summary>
        Basics,
        /// <summary>
        /// 在Basics前装饰
        /// </summary>
        PreDecorate,
        /// <summary>
        /// 在Basics后装饰
        /// </summary>
        NextDecorate,
    }
    /// <summary>
    /// 变量视图
    /// 自定义高于系统
    /// </summary>
    public interface IVariableElement
    {
        /// <summary>
        /// 画变量面板
        /// </summary>
        /// <param name="graphView">微图视图</param>
        /// <param name="variable">对应变量</param>
        /// <param name="hasDefalt">是否有默认值</param>
        /// <returns></returns>
        VisualElement DrawElement(BaseMicroGraphView graphView, BaseMicroVariable variable, bool hasDefalt);
    }
    /// <summary>
    /// 微节点元素
    /// </summary>
    public interface INodeFieldElement
    {
        /// <summary>
        /// 是否中断默认渲染
        /// 如果不中断，则在绘制完当前元素，还会继续绘制默认元素
        /// </summary>
        NodeFieldElementType ElementType { get; }
        /// <summary>
        /// 变量名
        /// </summary>
        FieldInfo Field { get; }
        /// <summary>
        /// 用户数据
        /// </summary>
        object UserData { get; set; }
        /// <summary>
        /// 根视图
        /// </summary>
        VisualElement Root { get; }
        /// <summary>
        /// 端口，可为空
        /// </summary>
        MicroPort Port { get; }
        /// <summary>
        /// 画元素
        /// </summary>
        /// <param name="nodeView">节点视图</param>
        /// <param name="field">字段</param>
        /// <param name="portDir">端口方向</param>
        /// <returns></returns>
        void DrawElement(BaseMicroNodeView nodeView, FieldInfo field, PortDirEnum portDir);
        /// <summary>
        /// 画变量的链接
        /// </summary>
        void DrawLink(BaseMicroNodeView nodeView);
    }
    public abstract class BaseNodeFieldElement<T> : INodeFieldElement
    {
        public const string LABEL_TITLE_STYLE_CLASS = "base_node_title";
        public const string FIXED_PORT_WIDTH = "fixed_width_port";
        public const string STYLE_PATH = "Uss/MicroGraph/Element/BaseNodeFieldElement";
        protected const MicroPortType CAN_LINK_PORT_TYPE = MicroPortType.BaseVarPort;
        public virtual NodeFieldElementType ElementType => NodeFieldElementType.Basics;

        protected VisualElement inputField;
        private PropertyInfo _displayLabelProp;
        private string _title = "";
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;

                if (Port != null)
                    Port.title = value;
                else if (inputField != null)
                {
                    if (_displayLabelProp == null)
                    {
                        _displayLabelProp = inputField.GetType().GetProperty("label", BindingFlags.Instance | BindingFlags.Public);
                    }
                    _displayLabelProp?.SetValue(inputField, value ?? "");
                }
            }
        }
        public object UserData { get; set; }
        public MicroPort Port { get; protected set; }
        private VisualElement _root;
        public VisualElement Root => _root;
        public virtual T Value { get; set; }
        public FieldInfo Field { get; protected set; }
        public BaseMicroNodeView nodeView { get; protected set; }
        public BaseNodeFieldElement()
        {
            this._root = new VisualElement();
            this._root.name = this.GetType().Name;
            this._root.AddStyleSheet(STYLE_PATH);
            this._root.AddToClassList("base_node_field");
        }
        public virtual void DrawElement(BaseMicroNodeView nodeView, FieldInfo field, PortDirEnum portDir)
        {
            this.Field = field;
            this.Root.AddToClassList($"{portDir.ToString().ToLower()}_node_field");
            this.nodeView = nodeView;
            inputField = getInputElement();
            if (inputField != null)
            {
                var labelProp = inputField.GetType().GetProperty("labelElement");
                if (labelProp != null)
                {
                    var labelElement = (VisualElement)labelProp.GetValue(inputField);
                    labelElement.tooltip = field.GetFieldDisplayName();
                    labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
                }
                _root.Add(inputField);
                inputField.RegisterCallback<ChangeEvent<T>>(m_onValueChanged);
                Value = (T)Field.GetValue(this.nodeView.Target);
            }
            if (portDir != PortDirEnum.None)
            {
                Port = new MicroPort(MicroPortType.VarPort, Orientation.Horizontal, portDir == PortDirEnum.In ? Direction.Input : Direction.Output, false);
                Port.onConnect += OnPortConnect;
                Port.onDisconnect += OnPortDisconnect;
                Port.onCanLink += OnPortCanConnect;
                Port.type = Field.FieldType;
                Port.key = Field.Name;
                if (portDir == PortDirEnum.In)
                {
                    this.Root.Insert(0, Port);
                    if (inputField != null)
                    {
                        inputField.SetDisplay(true);
                        this.Port.view.AddToClassList(FIXED_PORT_WIDTH);
                    }
                }
                else
                {
                    this.Root.Add(Port);
                    inputField?.SetDisplay(false);
                    this.Port.view.RemoveFromClassList(FIXED_PORT_WIDTH);
                }
                Port.view.portColor = MicroGraphUtils.GetColor(Field.FieldType);
            }
            this.Title = Field.GetFieldDisplayName();
        }

        public virtual void DrawLink(BaseMicroNodeView nodeView)
        {
            if (Port == null)
                return;
            bool isInput = Port.IsInput;
            var edges = nodeView.Target.VariableEdges.Where(a => a.fieldName == this.Field.Name && a.isInput == isInput).ToList();
            int count = 0;
            foreach (var item in edges)
            {
                count++;
                Node node = nodeView.GetGraphViewElement<Node>(item.nodeId);
                if (node == null)
                {
                    nodeView.Target.VariableEdges.Remove(item);
                    continue;
                }
                var varNodeView = (MicroVariableNodeView)node;
                Port.ConnectWithoutNotify(isInput ? varNodeView.OutPut : varNodeView.Input);
            }
            inputField?.SetDisplay(Port.IsInput && count == 0);
        }

        public virtual void DrawVarLink(MicroVariableEdge microVariable)
        {
            if (Port == null)
                return;
            Node node = nodeView.owner.GetElement<Node>(microVariable.nodeId);
            if (node == null)
            {
                nodeView.Target.VariableEdges.Remove(microVariable);
                return;
            }

            if (microVariable.isInput != Port.IsInput)
                return;
            switch (node)
            {
                case MicroVariableNodeView.InternalNodeView varNodeView:
                    Port.ConnectWithoutNotify(microVariable.isInput ? varNodeView.nodeView.OutPut : varNodeView.nodeView.Input);
                    inputField?.SetDisplay(false);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 当端口发生连接
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void OnPortConnect(MicroPort mine, MicroPort target)
        {
            if (inputField != null && mine.IsInput)
                inputField.SetDisplay(false);
            this.nodeView.AddVariableEdge(mine, target);
        }
        /// <summary>
        /// 当端口断开连接
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        protected virtual void OnPortDisconnect(MicroPort mine, MicroPort target)
        {
            if (inputField != null && mine.IsInput)
                inputField.SetDisplay(true);
            this.nodeView.RemoveVariableEdge(mine, target);
        }
        /// <summary>
        /// 检查端口是否可连接
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual bool OnPortCanConnect(MicroPort mine, MicroPort target)
        {

            if ((target.portType & CAN_LINK_PORT_TYPE) > MicroPortType.None)
            {
                if (mine.type == target.type || target.type.IsSubclassOf(mine.type))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取输入视图
        /// </summary>
        /// <returns></returns>
        protected abstract VisualElement getInputElement();
        private void m_onValueChanged(ChangeEvent<T> evt)
        {
            Value = evt.newValue;
            Field.SetValue(this.nodeView.Target, evt.newValue);
        }
    }


    //public abstract class BaseNodeFieldElement : INodeFieldElement
    //{
    //    protected const string LABEL_TITLE_STYLE_CLASS = "base_node_title";
    //    protected const string FIXED_PORT_WIDTH = "fixed_width_port";
    //    private const string STYLE_PATH = "Uss/MicroGraph/Element/BaseNodeFieldElement";


    //    private const MicroPortType CAN_LINK_PORT_TYPE = MicroPortType.BaseVarPort;
    //    protected Func<INodeFieldElement, object> getValueAction;
    //    public event Func<INodeFieldElement, object> onGetValue { add => getValueAction += value; remove => getValueAction -= value; }

    //    protected Action<INodeFieldElement, object> setValueAction;
    //    public event Action<INodeFieldElement, object> onSetValue { add => setValueAction += value; remove => setValueAction -= value; }
    //    public event PortModifyDelegate onPortConnect;
    //    public event PortModifyDelegate onPortDisconnect;
    //    public event CheckCanLinkDelegate onPortCanConnect;
    //    /// <summary>
    //    /// 当前类显示的类型
    //    /// </summary>
    //    protected abstract Type CurType { get; }

    //    private PortDirEnum _portDir = PortDirEnum.None;
    //    public BaseMicroGraphView Owner => this.Root.GetFirstOfType<BaseMicroGraphView.InternalGraphView>()?.graphView;
    //    public virtual VisualElement DisplayElement { get; }

    //    private PropertyInfo _displayLabelProp;
    //    private string _title = "";
    //    public virtual string Title
    //    {
    //        get
    //        {
    //            return _title;
    //        }
    //        set
    //        {
    //            _title = value;
    //            if (PortDir == PortDirEnum.None)
    //            {
    //                if (DisplayElement == null)
    //                    return;
    //                if (_displayLabelProp == null && DisplayElement != null)
    //                {
    //                    _displayLabelProp = DisplayElement.GetType().GetProperty("label", BindingFlags.Instance | BindingFlags.Public);
    //                }
    //                _displayLabelProp?.SetValue(this.DisplayElement, value ?? "");
    //                return;
    //            }
    //            if (Port != null)
    //                Port.title = value;
    //        }
    //    }
    //    public object UserData { get; set; }

    //    public bool IsInit { get; private set; }

    //    public PortDirEnum PortDir
    //    {
    //        get => _portDir;
    //        set
    //        {
    //            if (_portDir != PortDirEnum.None)
    //            {
    //                Debug.LogError("端口方向已确定无法改变");
    //                return;
    //            }
    //            _portDir = value;
    //            if (!IsInit)
    //                return;
    //            this.Root.AddToClassList($"{_portDir.ToString().ToLower()}_node_field");
    //            m_initPort();
    //        }
    //    }
    //    public MicroPort Port { get; private set; }
    //    private VisualElement _root;
    //    public VisualElement Root => _root;

    //    public BaseNodeFieldElement()
    //    {
    //        this._root = new VisualElement();
    //        this._root.name = this.GetType().Name;
    //        this._root.AddStyleSheet(STYLE_PATH);
    //        this._root.AddToClassList("base_node_field");
    //    }

    //    public virtual void Initialize()
    //    {
    //        if (PortDir != PortDirEnum.None)
    //            m_initPort();
    //        if (!string.IsNullOrWhiteSpace(_title))
    //            this.Title = _title;
    //        IsInit = true;
    //    }
    //    /// <summary>
    //    /// 获取视图元素
    //    /// </summary>
    //    /// <returns></returns>
    //    protected abstract VisualElement getVisualElement();
    //    public void Add(VisualElement element) => this.Root.Add(element);
    //    public void Insert(int index, VisualElement element) => this.Root.Insert(index, element);
    //    /// <summary>
    //    /// 当端口发生连接
    //    /// </summary>
    //    /// <param name="mine"></param>
    //    /// <param name="target"></param>
    //    protected virtual void OnPortConnect(MicroPort mine, MicroPort target)
    //    {
    //        if (DisplayElement != null && mine.IsInput)
    //            DisplayElement.SetDisplay(false);
    //        onPortConnect?.Invoke(mine, target);
    //    }
    //    /// <summary>
    //    /// 当端口断开连接
    //    /// </summary>
    //    /// <param name="mine"></param>
    //    /// <param name="target"></param>
    //    protected virtual void OnPortDisconnect(MicroPort mine, MicroPort target)
    //    {
    //        if (DisplayElement != null && mine.IsInput)
    //            DisplayElement.SetDisplay(true);
    //        onPortDisconnect?.Invoke(mine, target);
    //    }
    //    /// <summary>
    //    /// 检查端口是否可连接
    //    /// </summary>
    //    /// <param name="mine"></param>
    //    /// <param name="target"></param>
    //    /// <returns></returns>
    //    protected virtual bool OnPortCanConnect(MicroPort mine, MicroPort target)
    //    {
    //        if (onPortCanConnect != null)
    //        {
    //            return onPortCanConnect.Invoke(mine, target);
    //        }
    //        else
    //        {
    //            if ((target.portType & CAN_LINK_PORT_TYPE) > MicroPortType.None)
    //            {
    //                if (mine.type == target.type || target.type.IsSubclassOf(mine.type))
    //                {
    //                    return true;
    //                }
    //            }
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// 刷新端口
    //    /// </summary>
    //    private void m_initPort()
    //    {
    //        Port = new MicroPort(MicroPortType.VarPort, Orientation.Horizontal, _portDir == PortDirEnum.In ? Direction.Input : Direction.Output, false);
    //        Port.title = Title;
    //        Port.onConnect += OnPortConnect;
    //        Port.onDisconnect += OnPortDisconnect;
    //        Port.onCanLink += OnPortCanConnect;
    //        Port.type = CurType;
    //        if (_portDir == PortDirEnum.In)
    //        {
    //            this.Root.Insert(0, Port);
    //            if (DisplayElement != null)
    //            {
    //                DisplayElement.SetDisplay(true);
    //                this.Port.view.AddToClassList(FIXED_PORT_WIDTH);
    //            }
    //        }
    //        else
    //        {
    //            this.Root.Add(Port);
    //            DisplayElement?.SetDisplay(false);
    //            this.Port.view.RemoveFromClassList(FIXED_PORT_WIDTH);
    //        }
    //        Port.view.portColor = MicroGraphUtils.GetColor(CurType);
    //    }
    //}
    //public abstract class BaseNodeFieldElement<T> : BaseNodeFieldElement
    //{
    //    private VisualElement _displayElement;
    //    public override VisualElement DisplayElement => _displayElement;
    //    public virtual T Value { get; set; }
    //    protected override Type CurType => typeof(T);
    //    public override void Initialize()
    //    {
    //        _displayElement = getVisualElement();
    //        if (_displayElement == null)
    //            goto End;
    //        var labelProp = _displayElement.GetType().GetProperty("labelElement");
    //        if (labelProp != null)
    //        {
    //            var labelElement = (VisualElement)labelProp.GetValue(_displayElement);
    //            labelElement.AddToClassList(LABEL_TITLE_STYLE_CLASS);
    //        }
    //        this.Add(_displayElement);
    //        DisplayElement.RegisterCallback<ChangeEvent<T>>(m_onValueChanged);
    //        if (this.getValueAction != null)
    //            Value = (T)this.getValueAction.Invoke(this);
    //        End: base.Initialize();
    //    }

    //    private void m_onValueChanged(ChangeEvent<T> evt)
    //    {
    //        Value = evt.newValue;
    //        setValueAction?.Invoke(this, Value);
    //    }
    //}
}
