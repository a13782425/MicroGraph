using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace MicroGraph.Editor
{
    public sealed partial class MicroPort
    {
        private const MicroPortType VAR_PORT_TYPE = MicroPortType.VarPort | MicroPortType.BaseVarPort;
        /// <summary>
        /// 检测是否可以连接
        /// </summary>
        /// <returns>返回是否可以连接</returns>
        public delegate bool CheckCanLinkDelegate(MicroPort mine, MicroPort target);

        /// <summary>
        /// 检测是否可以连接
        /// </summary>
        /// <returns>返回是否可以连接</returns>
        public delegate void PortModifyDelegate(MicroPort mine, MicroPort target);

        public Port view { get; private set; }

        private Type _type;
        public Type type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (_type != null && view != null && (_portType & VAR_PORT_TYPE) > MicroPortType.None)
                {
                    view.portColor = MicroGraphUtils.GetColor(_type);
                    this.view.tooltip = _type.Name + ": " + title;
                }
                else
                    this.view.tooltip = title;
            }
        }

        private MicroPortType _portType;
        /// <summary>
        /// 当前端口类型
        /// </summary>
        public MicroPortType portType => _portType;

        private bool _isInput = false;
        /// <summary>
        /// 是否是入端口
        /// </summary>
        public bool IsInput => _isInput;

        /// <summary>
        /// 当节点连接
        /// </summary>
        public event PortModifyDelegate onConnect;
        /// <summary>
        /// 当节点断开连接
        /// </summary>
        public event PortModifyDelegate onDisconnect;
        /// <summary>
        /// 是否可以被连接
        /// (自己,目标,是否可以连接)
        /// </summary>
        public event CheckCanLinkDelegate onCanLink;
        public string title
        {
            get => view.portName;
            set
            {
                view.portName = value;
                if (_type != null)
                    this.view.tooltip = _type.Name + ": " + value;
                else
                    this.view.tooltip = value;
            }
        }

        public Color color { get => view.portColor; set => view.portColor = value; }
        /// <summary>
        /// 端口在BaseMicroNodeView中的唯一的Key
        /// <para>自行设置</para>
        /// <para>通过BaseMicroNodeView可以查到对应端口</para>
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 是否可以多连
        /// </summary>
        public bool isMulti { get; set; }


        public MicroPort(MicroPortType portType, Orientation portOrientation, Direction portDirection, bool isMulti = true)
        {
            this.isMulti = isMulti;
            this._portType = portType;
            this._isInput = portDirection == Direction.Input;
            if (portType == MicroPortType.NodePort && portDirection == Direction.Input)
            {
                throw new ArgumentException("BaseMicroNode引用端口只能是output");
            }
            this.view = new InternalPort(this, portOrientation, portDirection, Capacity.Multi);
        }


    }
    //public
    partial class MicroPort
    {
        internal bool CanConnectPort(MicroPort targetPort)
        {
            bool result = true;
            if (onCanLink != null)
            {
                foreach (var item in onCanLink.GetInvocationList())
                {
                    result = ((CheckCanLinkDelegate)item).Invoke(this, targetPort);
                    if (!result)
                        goto End;
                }
            }
            if (!isMulti)
                result = view.connections.Count() == 0;
            End: return result;
        }
        /// <summary>
        /// 获取当前端口所在节点的ID
        /// <para>如果返回-1则是没有找到</para>
        /// </summary>
        /// <returns></returns>
        public int GetNodeId()
        {
            Node node = view.GetFirstAncestorOfType<Node>();
            if (node == null)
                return -1;
            switch (node)
            {
                case BaseMicroNodeView.InternalNodeView nodeView:
                    return nodeView.nodeView.Target.OnlyId;
                case MicroVariableNodeView.InternalNodeView variableView:
                    return variableView.nodeView.NodeId;
                default:
                    return -1;
            }
        }
        /// <summary>
        /// 清空检测可以连接的事件
        /// </summary>
        public void ClearCanConnectEvent()
        {
            if (onCanLink != null)
            {
                foreach (var item in onCanLink.GetInvocationList())
                {
                    onCanLink -= (CheckCanLinkDelegate)item;
                }
            }
        }

        public void Connect(MicroPort connectPort)
        {
            m_connectPort(connectPort);
        }
        internal void Connect(Edge edge)
        {
            m_connectEdge(edge);
        }
        public void ConnectWithoutNotify(MicroPort connectPort)
        {
            m_connectPort(connectPort, false);
        }
        internal void ConnectWithoutNotify(Edge edge)
        {
            m_connectEdge(edge, false);
        }
        public void Disonnect(MicroPort disconnectPort)
        {
            m_disconnectPort(disconnectPort);
        }
        public void Disonnect(Edge edge)
        {
            m_disconnectEdge(edge);
        }
        public void DisonnectWithoutNotify(MicroPort disconnectPort)
        {
            m_disconnectPort(disconnectPort, false);
        }
        public void DisonnectWithoutNotify(Edge edge)
        {
            m_disconnectEdge(edge, false);
        }
    }
    //private
    partial class MicroPort
    {
        private void m_connectPort(MicroPort connectPort, bool sendCallback = true)
        {
            MicroEdgeView edgeView = new MicroEdgeView();
            if (this._isInput)
            {
                edgeView.input = this;
                edgeView.output = connectPort;
            }
            else
            {
                edgeView.input = connectPort;
                edgeView.output = this;
            }
            this.view.Connect(edgeView);
            connectPort.view.Connect(edgeView);
            this.view.GetFirstAncestorOfType<BaseMicroGraphView.InternalGraphView>().AddElement(edgeView);
            if (sendCallback)
            {
                this.onConnect?.Invoke(this, connectPort);
                connectPort.onConnect?.Invoke(connectPort, this);
            }
        }
        private void m_connectEdge(Edge edge, bool sendCallback = true)
        {
            this.view.Connect(edge);
            MicroPort connectPort = null;
            if (this._isInput)
            {
                connectPort = edge.output as MicroPort.InternalPort;
            }
            else
            {
                connectPort = edge.input as MicroPort.InternalPort;
            }
            if (sendCallback)
            {
                this.onConnect?.Invoke(this, connectPort);
            }
        }
        private void m_disconnectPort(MicroPort disconnectPort, bool sendCallback = true)
        {
            if (sendCallback)
            {
                Edge edge = null;
                foreach (var item in disconnectPort.view.connections)
                {
                    if (this.IsInput)
                    {
                        if (item.input == this.view && item.output == disconnectPort.view)
                        {
                            edge = item;
                            break;
                        }
                    }
                    else
                    {
                        if (item.input == disconnectPort.view && item.output == this.view)
                        {
                            edge = item;
                            break;
                        }
                    }
                }

                if (edge != null)
                    this.view.Disconnect(edge);
                this.onDisconnect?.Invoke(this, disconnectPort);
            }
        }
        private void m_disconnectEdge(Edge edge, bool sendCallback = true)
        {
            if (sendCallback)
            {
                this.view.Disconnect(edge);
                MicroPort disconnectPort = (InternalPort)edge.input;
                if (this.IsInput)
                    disconnectPort = (InternalPort)edge.output;
                this.onDisconnect?.Invoke(this, disconnectPort);
            }
        }
    }
    partial class MicroPort
    {
        internal class InternalPort : Port
        {

            private const string STYLE_PATH = "Uss/MicroGraph/MicroPort";
            internal readonly MicroPort microPort;
            internal InternalPort(MicroPort microPort, Orientation portOrientation, Direction portDirection, Capacity portCapacity)
                : base(portOrientation, portDirection, portCapacity, null)
            {
                this.microPort = microPort;
                this.AddStyleSheet(STYLE_PATH);
                MicroEdgeConnectorListener listener = new MicroEdgeConnectorListener();
                m_EdgeConnector = new MicroEdgeConnector(listener);
                this.AddManipulator(this.m_EdgeConnector);
                this.AddToClassList($"{microPort._portType.ToString().ToLower()}_{direction.ToString().ToLower()}");
                if ((microPort._portType & VAR_PORT_TYPE) > MicroPortType.None)
                {
                    this.AddToClassList("var_node_port");
                }
                this.m_ConnectorText.style.textOverflow = TextOverflow.Ellipsis;
                this.m_ConnectorText.style.overflow = Overflow.Hidden;
                this.m_ConnectorText.style.whiteSpace = WhiteSpace.NoWrap;
                if (microPort._type != null)
                    this.portColor = MicroGraphUtils.GetColor(microPort._type);
            }

            public void SetLabelTooltip(string tooltip)
            {
                m_ConnectorText.tooltip = tooltip;
            }

            public override void Connect(Edge edge)
            {
                base.Connect(edge);
            }
            public override void Disconnect(Edge edge)
            {
                base.Disconnect(edge);
            }
            public override void DisconnectAll()
            {
                base.DisconnectAll();
            }
            public override bool ContainsPoint(Vector2 localPoint)
            {
                //TODO: 计算localPoint与m_ConnectorBox的关系
                Rect rect = m_ConnectorBox.layout;
                Rect rect2;
                if (direction == Direction.Input)
                {
                    rect2 = new Rect(0f - rect.xMin, 0f - rect.yMin, rect.width + rect.xMin, this.layout.height);
                }
                else
                {
                    rect2 = new Rect(0f, 0f - rect.yMin, this.layout.width - rect.xMin, this.layout.height);
                }

                return rect2.Contains(this.ChangeCoordinatesTo(m_ConnectorBox, localPoint));
            }

            public static implicit operator MicroPort(InternalPort port)
            {
                return port.microPort;
            }
        }

        public static implicit operator Port(MicroPort port)
        {
            return port.view;
        }
    }

}
