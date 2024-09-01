using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static MicroGraph.Editor.MicroGraphUtils;
using UnityDebug = UnityEngine.Debug;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 默认的微图视图
    /// </summary>
    public partial class BaseMicroGraphView
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroGraphView";
        /// <summary>
        /// 特殊的操作批次ID
        /// </summary>
        internal const int SPECIAL_OPERATE_BATCH_ID = -1;

        public GraphView View => _internalGraphView;
        /// <summary>
        /// 当前微图
        /// </summary>
        public BaseMicroGraph Target => _operateModel?.microGraph;
        /// <summary>
        /// 单个微图对象的简介信息
        /// </summary>
        public GraphSummaryModel SummaryModel => _operateModel?.summaryModel;
        /// <summary>
        /// 当前微图的信息
        /// </summary>
        public GraphCategoryModel CategoryModel => _operateModel?.categoryModel;
        /// <summary>
        /// 编辑器相关信息
        /// </summary>
        internal MicroGraphEditorInfo editorInfo => Target.editorInfo;

        private MicroGraphWindow _owner;
        /// <summary>
        /// 归属
        /// </summary>
        public EditorWindow owner => _owner;
        /// <summary>
        /// 针对于一个图的事件监听器
        /// </summary>
        public MicroGraphEventListener listener => _owner.listener;
        /// <summary>
        /// 当前样式
        /// </summary>
        public IStyle Style { get => _internalGraphView.style; }

        /// <summary>
        /// 控制面板
        /// </summary>
        private MicroControlView _controlView;

        private MicroAlignmentView _alignmentView;

        /// <summary>
        /// 微图选中发生变化回调
        /// </summary>
        public event Action<List<ISelectable>> onSelectChanged;

        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool ShowGrid
        {
            get => editorInfo.ShowGrid;
            set
            {
                if (editorInfo.ShowGrid == value)
                    return;
                editorInfo.ShowGrid = value;
                if (value)
                    _internalGraphView.m_showGrid();
                else
                    _internalGraphView.m_hideGrid();
            }
        }
        /// <summary>
        /// 是否可以缩放
        /// </summary>
        public bool CanZoom
        {
            get => editorInfo.CanZoom;
            set
            {
                if (editorInfo.CanZoom == value)
                    return;
                editorInfo.CanZoom = value;
                if (value)
                    _internalGraphView.AddManipulator(_contentZoomer);
                else
                    _internalGraphView.RemoveManipulator(_contentZoomer);
            }
        }
        /// <summary>
        /// 是否可以缩放
        /// </summary>
        public bool ShowMiniMap
        {
            get => editorInfo.ShowMiniMap;
            set
            {
                if (editorInfo.ShowMiniMap == value)
                    return;
                editorInfo.ShowMiniMap = value;
                if (value)
                    _internalGraphView.Add(_miniMap);
                else
                    _miniMap.RemoveFromHierarchy();
            }
        }
        /// <summary>
        /// 缩放最小值
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomMinScale { get => _contentZoomer.minScale; set => _contentZoomer.minScale = value; }
        /// <summary>
        /// 缩放最大值
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomMaxScale { get => _contentZoomer.maxScale; set => _contentZoomer.maxScale = value; }
        /// <summary>
        /// 单次缩放步长
        /// canZoom=true时候生效
        /// </summary>
        public float ZoomScaleStep { get => _contentZoomer.scaleStep; set => _contentZoomer.scaleStep = value; }
        /// <summary>
        /// 当前图的操作数据
        /// </summary>
        private GraphOperateModel _operateModel = null;
        /// <summary>
        /// 可以连接的端口
        /// </summary>
        private List<Port> _canLinkPorts = new List<Port>();
        /// <summary>
        /// 缩放操作者
        /// </summary>
        private ContentZoomer _contentZoomer = new ContentZoomer();
        /// <summary>
        /// 小地图
        /// </summary>
        private MicroMiniMap _miniMap = null;
        /// <summary>
        /// 创建节点的搜索窗口
        /// </summary>
        private MicroCreateNodeWindow _createNodeWindow = null;
        /// <summary>
        /// 创建唯一节点搜索窗口
        /// </summary>
        private MicroCreateNodeWindow _createUniqueNodeWindow = null;
        /// <summary>
        /// 内部视图
        /// </summary>
        private InternalGraphView _internalGraphView;
        /// <summary>
        /// 子节点
        /// </summary>
        private Dictionary<int, GraphElement> _childElements = new Dictionary<int, GraphElement>();
        /// <summary>
        /// 撤销操作
        /// </summary>
        private MicroGraphViewUndo _undo;
        /// <summary>
        /// 撤销操作
        /// </summary>
        public MicroGraphViewUndo Undo => _undo;

        private List<IMicroGraphKeyEvent> _keyEvents = new List<IMicroGraphKeyEvent>();
        /// <summary>
        /// 所有按键操作
        /// </summary>
        protected List<IMicroGraphKeyEvent> keyEvents => _keyEvents;
        /// <summary>
        /// 是否可以拷贝
        /// </summary>
        protected internal virtual bool CanCopy
        {
            get
            {
                bool isAllEdge = View.selection.All(a => a is Edge);
                return isAllEdge ? false : _internalGraphView.CanCopy;
            }
        }

        private bool _operateNode = false;
        private bool _operateVariable = false;
        /// <summary>
        /// 添加节点回调
        /// </summary>
        public event Action<BaseMicroNode> addNodeCallback;
        /// <summary>
        /// 移除节点回调
        /// </summary>
        public event Action<BaseMicroNode> removeNodeCallback;
        /// <summary>
        /// 添加变量回调
        /// </summary>
        public event Action<BaseMicroVariable> addVariableCallback;
        /// <summary>
        /// 移除变量回调
        /// </summary>
        public event Action<BaseMicroVariable> removeVariableCallback;

        public BaseMicroGraphView()
        {
            Input.imeCompositionMode = IMECompositionMode.On;
            foreach (var item in MicroGraphProvider.AllKeyEvents)
                _keyEvents.Add(Activator.CreateInstance(item) as IMicroGraphKeyEvent);
            _keyEvents.RemoveAll(a => a == null);
            _internalGraphView = new InternalGraphView(this);
            this.AddStyleSheet(STYLE_PATH);
            _miniMap = new MicroMiniMap(this);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.ZoomMaxScale = 2;
            //this.View.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
            this.View.RegisterCallback<KeyDownEvent>(onKeyDownEvent);
            this.View.RegisterCallback<DragPerformEvent>(m_onDragPerformEvent);
            this.View.RegisterCallback<DragUpdatedEvent>(m_onDragUpdatedEvent);
            this.View.viewTransformChanged += onViewTransformChanged;
            this.View.graphViewChanged += onGraphViewChanged;
            this.View.nodeCreationRequest += m_nodeCreateRequest;
            this.View.StretchToParentSize();
        }
    }
    //public
    partial class BaseMicroGraphView
    {
        public void Show()
        {
            this.Style.display = DisplayStyle.Flex;
            _owner.topToolbarView.onGUI -= onTopToolbarGUI;
            _owner.topToolbarView.onGUI += onTopToolbarGUI;
            _owner.bottomToolbarView.onGUI -= onBottomToolbarGUI;
            _owner.bottomToolbarView.onGUI += onBottomToolbarGUI;
            onShow();
        }
        public void Hide()
        {
            this.Style.display = DisplayStyle.None;
            _owner.topToolbarView.onGUI -= onTopToolbarGUI;
            _owner.bottomToolbarView.onGUI -= onBottomToolbarGUI;
            onHide();
        }
        /// <summary>
        /// 保存当前微图
        /// </summary>
        public void Save()
        {
            editorInfo.ModifyTime = DateTime.Now;
            SummaryModel.SetEditorInfo(editorInfo);
            EditorUtility.SetDirty(Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            this.owner.ShowNotification(new GUIContent("保存成功"), NOTIFICATION_TIME);
        }
        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodePosition"></param>
        /// <returns></returns>
        public T AddNode<T>(Vector2 nodePosition) where T : BaseMicroNode
        {
            return (T)AddNode(typeof(T), nodePosition);
        }
        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="nodePosition"></param>
        public BaseMicroNode AddNode(Type nodeType, Vector2 nodePosition)
        {
            if (_operateNode)
                throw new Exception("无法在节点回调中创建节点");
            BaseMicroNode microNode = Activator.CreateInstance(nodeType) as BaseMicroNode;
            NodeCategoryModel nodeCategory = _operateModel.categoryModel.NodeCategories.FirstOrDefault(a => a.NodeClassType == nodeType);
            MicroNodeEditorInfo editorInfo = new MicroNodeEditorInfo();
            editorInfo.Pos = nodePosition;
            editorInfo.Title = nodeCategory.NodeName;
            editorInfo.Target = microNode;
            m_setNodeId(microNode);
            editorInfo.NodeId = microNode.OnlyId;
            AddNode(editorInfo);
            return microNode;
        }
        /// <summary>
        /// 移除一个节点
        /// </summary>
        /// <param name="onlyId">唯一ID</param>
        public void RemoveNode(int onlyId)
        {
            RemoveNode(Target.Nodes.FirstOrDefault(a => a.OnlyId == onlyId));
        }
        /// <summary>
        /// 移除一个节点
        /// </summary>
        /// <param name="node">节点对象</param>
        public void RemoveNode(BaseMicroNode node)
        {
            if (node == null)
                return;
            if (_operateNode)
                throw new Exception("无法在节点回调中删除节点");
            MicroNodeEditorInfo nodeEditor = editorInfo.Nodes.FirstOrDefault(a => a.Target == node);
            RemoveNode(nodeEditor);
            View.DeleteSelection();
        }

        /// <summary>
        /// 添加一个变量
        /// </summary>
        /// <param name="varName">变量名</param>
        /// <param name="canDelete">是否可以被删除</param>
        /// <param name="canRename">是否可以改名</param>
        /// <param name="canDefaultValue">是否可以有默认值</param>
        /// <param name="canAssign">是否可以被赋值</param>
        public BaseMicroVariable AddVariable<T>(string varName, bool canDelete = true, bool canRename = true, bool canDefaultValue = true, bool canAssign = true)
        {
            return AddVariable(varName, typeof(T), canDelete, canRename, canDefaultValue, canAssign);
        }
        /// <summary>
        /// 添加一个变量
        /// </summary>
        /// <param name="varName">变量名</param>
        /// <param name="varType">变量类型</param>
        /// <param name="canDelete">是否可以被删除</param>
        /// <param name="canRename">是否可以改名</param>
        /// <param name="canDefaultValue">是否可以有默认值</param>
        /// <param name="canAssign">是否可以被赋值</param>
        public BaseMicroVariable AddVariable(string varName, Type varType, bool canDelete = true, bool canRename = true, bool canDefaultValue = true, bool canAssign = true)
        {
            if (_operateVariable)
                throw new Exception("无法在变量回调中创建变量");
            VariableCategoryModel varCategory = CategoryModel.GetVarCategory(varType);
            if (varCategory == null)
            {
                owner.ShowNotification(new GUIContent($"当前图不支持此变量:{varType.FullName}类型"), NOTIFICATION_TIME);
                return null;
            }
            if (!MicroGraphUtils.VariableValidity(varName))
            {
                owner.ShowNotification(new GUIContent("变量名不合法"), NOTIFICATION_TIME);
                return null;
            }
            BaseMicroVariable variable = Activator.CreateInstance(varCategory.VarBoxType) as BaseMicroVariable;
            variable.Name = varName;
            MicroVariableEditorInfo editorInfo = new MicroVariableEditorInfo();
            editorInfo.Name = varName;
            editorInfo.Target = variable;
            editorInfo.CanDelete = canDelete;
            editorInfo.CanRename = canRename;
            editorInfo.CanDefaultValue = canDefaultValue;
            editorInfo.CanAssign = canAssign;
            AddVariable(editorInfo);
            return variable;
        }
        /// <summary>
        /// 移除一个变量
        /// </summary>
        /// <param name="varName">变量名</param>
        public void RemoveVariable(string varName)
        {
            RemoveVariable(Target.Variables.FirstOrDefault(a => a.Name == varName));
        }
        /// <summary>
        /// 移除一个变量
        /// </summary>
        /// <param name="variable">变量对象</param>
        public void RemoveVariable(BaseMicroVariable variable)
        {
            if (variable == null)
                return;
            if (_operateVariable)
                throw new Exception("无法在变量回调中删除变量");
            MicroVariableEditorInfo varEditor = editorInfo.Variables.FirstOrDefault(a => a.Target == variable);
            RemoveVariable(varEditor);
            View.DeleteSelection();
        }

        /// <summary>
        /// 添加一个按键事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddKeyEvent<T>() where T : IMicroGraphKeyEvent, new()
        {
            Type type = typeof(T);
            if (_keyEvents.FirstOrDefault(a => a.GetType() == type) != null)
            {
                UnityDebug.LogWarning($"已存在按键：重复添加按键事件:{type}");
                return;
            }
            _keyEvents.Add(new T());
        }
        /// <summary>
        /// 删除一个按键事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveKeyEvent<T>() where T : IMicroGraphKeyEvent, new()
        {
            Type type = typeof(T);
            IMicroGraphKeyEvent keyEvent = _keyEvents.FirstOrDefault(a => a.GetType() == type);
            if (keyEvent == null)
                return;
            _keyEvents.Remove(keyEvent);
        }
        public void AddStyleSheet(string stylePath)
        {
            _internalGraphView.AddStyleSheet(stylePath);
        }
        public void RemoveStyleSheet(string stylePath)
        {
            _internalGraphView.RemoveStyleSheet(stylePath);
        }
        public void AddElement(GraphElement graphElement)
        {
            _internalGraphView.AddElement(graphElement);
        }
        public void RemoveElement(GraphElement graphElement)
        {
            _internalGraphView.RemoveElement(graphElement);
        }
        public void Insert(int index, VisualElement element)
        {
            _internalGraphView.Insert(index, element);
        }
        public void Remove(VisualElement element)
        {
            _internalGraphView.Remove(element);
        }
        public void RemoveAt(int index)
        {
            _internalGraphView.RemoveAt(index);
        }
        public void Clear()
        {
            _internalGraphView.Clear();
        }
        public void AddManipulator(IManipulator manipulator)
        {
            _internalGraphView.AddManipulator(manipulator);
        }
        public void RemoveManipulator(IManipulator manipulator)
        {
            _internalGraphView.AddManipulator(manipulator);
        }
        public T GetElement<T>(int onlyId) where T : GraphElement
        {
            if (_childElements.ContainsKey(onlyId))
            {
                return (T)_childElements[onlyId];
            }
            return null;
        }
    }
    //internal
    partial class BaseMicroGraphView
    {
        internal void Initialize(MicroGraphWindow window)
        {
            _owner = window;
            _operateModel = this._owner.operateModel;
            MicroGraphProvider.InitGraphCategory(this);
            m_initNodeId();
            listener.AddListener(MicroGraphEventIds.VAR_MODIFY, m_onVarModifyCallback);
            _alignmentView = new MicroAlignmentView(this);
            this.View.Add(_alignmentView);
            _controlView = new MicroControlView(this);
            this.View.Add(this._controlView);
            for (int i = Target.Variables.Count - 1; i >= 0; i--)
            {
                BaseMicroVariable variable = Target.Variables[i];
                if (MicroGraphProvider.GetVariableCategory(variable.GetValueType()) == null)
                {
                    Target.Variables.RemoveAt(i);
                    continue;
                }
                MicroVariableEditorInfo info = editorInfo.Variables.FirstOrDefault(a => a.Name == variable.Name);
                info.Target = variable;
            }
            editorInfo.Variables.RemoveAll(v => v.Target == null);
            Target.Nodes.ForEach(n =>
            {
                var info = editorInfo.Nodes.FirstOrDefault(a => a.NodeId == n.OnlyId);
                info.Target = n;
                m_showNodeView(info);
            });
            editorInfo.Nodes.RemoveAll(a => a.Target == null);
            for (int i = editorInfo.VariableNodes.Count - 1; i >= 0; i--)
            {
                MicroVariableNodeEditorInfo varNode = editorInfo.VariableNodes[i];
                var info = editorInfo.Variables.FirstOrDefault(a => a.Name == varNode.Name);
                if (info == null)
                {
                    editorInfo.VariableNodes.RemoveAt(i);
                    continue;
                }
                varNode.EditorInfo = info;
                m_showVarNodeView(varNode);
            }
            editorInfo.Stickys.ForEach(n => m_showStickyView(n));
            editorInfo.Groups.ForEach(n => m_showGroupView(n));
            _childElements.Values.ToList().ForEach(n =>
            {
                if (n is BaseMicroNodeView.InternalNodeView mn)
                    mn.nodeView.DrawLink();
            });
            this._controlView.VariableControlView.Show();
            this.View.UpdateViewTransform(editorInfo.Pos, new Vector2(editorInfo.Scale, editorInfo.Scale));
            _undo = new MicroGraphViewUndo();
            _undo.Initialize(this);
            m_initOther();
            onInit();

            void m_initOther()
            {
                if (ShowGrid)
                    _internalGraphView.m_showGrid();
                else
                    _internalGraphView.m_hideGrid();

                if (CanZoom)
                    _internalGraphView.AddManipulator(_contentZoomer);
                else
                    _internalGraphView.RemoveManipulator(_contentZoomer);

                if (ShowMiniMap)
                    _internalGraphView.Add(_miniMap);
                else
                    _miniMap.RemoveFromHierarchy();
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        internal void Exit()
        {
            _owner.topToolbarView.onGUI -= onTopToolbarGUI;
            _owner.bottomToolbarView.onGUI -= onBottomToolbarGUI;
            _controlView.Exit();
            onExit();
        }
        internal void AddNode(MicroNodeEditorInfo editorInfo)
        {
            this.Target.Nodes.Add(editorInfo.Target);
            this.editorInfo.Nodes.Add(editorInfo);
            BaseMicroNodeView nodeView = m_showNodeView(editorInfo);
            IMicroGraphRecordCommand record = new MicroNodeAddRecord();
            record.Record(this, nodeView);
            _undo?.AddCommand(record);
            try
            {
                _operateNode = true;
                addNodeCallback?.Invoke(editorInfo.Target);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                _operateNode = false;
            }
        }
        internal void RemoveNode(MicroNodeEditorInfo editorInfo)
        {
            this.Target.Nodes.Remove(editorInfo.Target);
            this.editorInfo.Nodes.Remove(editorInfo);
            this.View.AddToSelection(GetElement<Node>(editorInfo.NodeId));
            try
            {
                _operateNode = true;
                removeNodeCallback?.Invoke(editorInfo.Target);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                _operateNode = false;
            }
        }
        internal void AddVariable(MicroVariableEditorInfo editorInfo)
        {
            this.Target.Variables.Add(editorInfo.Target);
            this.editorInfo.Variables.Add(editorInfo);
            MicroVariableItemView itemView = this._controlView.VariableControlView.AddVariableView(editorInfo);
            IMicroGraphRecordCommand record = new MicroVarAddRecord();
            record.Record(this, itemView);
            _undo?.AddCommand(record);
            try
            {
                _operateVariable = true;
                addVariableCallback?.Invoke(editorInfo.Target);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                _operateVariable = false;
            }
        }
        internal void RemoveVariable(MicroVariableEditorInfo editorInfo)
        {
            this.Target.Variables.Remove(editorInfo.Target);
            this.editorInfo.Variables.Remove(editorInfo);
            this._controlView.VariableControlView.RemoveVariableView(editorInfo);
            try
            {
                _operateVariable = true;
                removeVariableCallback?.Invoke(editorInfo.Target);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                _operateVariable = false;
            }
        }
        internal MicroGroupView AddGroupView(MicroGroupEditorInfo group)
        {
            editorInfo.Groups.Add(group);
            MicroGroupView groupView = m_showGroupView(group);
            IMicroGraphRecordCommand record = new MicroGroupAddRecord();
            record.Record(this, groupView);
            _undo?.AddCommand(record);
            return groupView;
        }
        internal MicroStickyNoteView AddStickyNodeView(MicroStickyEditorInfo sticky)
        {
            editorInfo.Stickys.Add(sticky);
            MicroStickyNoteView stickyNote = m_showStickyView(sticky);
            IMicroGraphRecordCommand record = new MicroStickyAddRecord();
            record.Record(this, stickyNote);
            _undo?.AddCommand(record);
            return stickyNote;
        }
        internal void RemoveGroupView(MicroGroupEditorInfo group)
        {
            this.editorInfo.Groups.Remove(group);
            this.View.AddToSelection(GetElement<MicroGroupView>(group.GroupId));
        }
        internal void RemoveStickyNodeView(MicroStickyEditorInfo sticky)
        {
            this.editorInfo.Stickys.Remove(sticky);
            this.View.AddToSelection(GetElement<MicroStickyNoteView>(sticky.NodeId));
        }
        internal MicroVariableNodeView AddVariableNodeView(BaseMicroVariable variable, Vector2 pos)
        {
            MicroVariableNodeEditorInfo varNodeEditor = new MicroVariableNodeEditorInfo();
            varNodeEditor.Name = variable.Name;
            varNodeEditor.Pos = pos;
            varNodeEditor.NodeId = editorInfo.GetUniqueId();
            varNodeEditor.EditorInfo = editorInfo.Variables.FirstOrDefault(a => a.Name == variable.Name);
            return AddVariableNodeView(varNodeEditor);
        }
        internal MicroVariableNodeView AddVariableNodeView(MicroVariableNodeEditorInfo varNodeEditor)
        {
            this.editorInfo.VariableNodes.Add(varNodeEditor);
            MicroVariableNodeView variableNodeView = m_showVarNodeView(varNodeEditor);
            IMicroGraphRecordCommand record = new MicroVarNodeAddRecord();
            record.Record(this, variableNodeView);
            _undo?.AddCommand(record);
            return variableNodeView;
        }
        internal void RemoveVariableNodeView(MicroVariableNodeEditorInfo varNodeEditor)
        {
            this.editorInfo.VariableNodes.Remove(varNodeEditor);
            this.View.AddToSelection(GetElement<Node>(varNodeEditor.NodeId));
        }
        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void AddGraphTemplate()
        {
            if (View.selection.OfType<Node>().Count() < 2)
            {
                owner.ShowNotification(new GUIContent("创建模板失败,请至少选择两个节点"), NOTIFICATION_TIME);
                return;
            }
            string graphClassName = this.CategoryModel.GraphType.FullName;
            MicroGraphConfig graphEditorModel = MicroGraphUtils.EditorConfig.GraphConfigs.FirstOrDefault(a => a.GraphClassName == graphClassName);
            if (graphEditorModel==null)
            {
                graphEditorModel = new MicroGraphConfig();
                graphEditorModel.GraphClassName = graphClassName;
                MicroGraphUtils.EditorConfig.GraphConfigs.Add(graphEditorModel);
            }
            MicroGraphTemplateModel model = new MicroGraphTemplateModel();
            model.Title = "模板";
            model.GraphClassName = this.CategoryModel.GraphType.FullName;
            foreach (var item in View.selection)
            {
                if (item is MicroVariableNodeView.InternalNodeView varNodeView)
                {
                    string varName = varNodeView.nodeView.Target.Name;
                    if (model.Vars.FirstOrDefault(a => a.VarName == varName) == null)
                        MicroGraphProvider.GetRecordTemplateImpl(typeof(MicroVariableEditorInfo))?.Record(model, varNodeView.nodeView.editorInfo.EditorInfo);
                }
                MicroGraphProvider.GetRecordTemplateImpl(item.GetType())?.Record(model, item);
            }
            graphEditorModel.Templates.Add(model);
            MicroGraphUtils.SaveConfig();
            MicroGraphEventListener.OnEventAll(MicroGraphEventIds.GRAPH_TEMPLATE_CHANGED);
            owner.ShowNotification(new GUIContent("创建模板成功"), NOTIFICATION_TIME);
        }
    }
    //protected override
    partial class BaseMicroGraphView
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void onInit() { }
        /// <summary>
        /// 显示
        /// </summary>
        protected virtual void onShow() { }
        /// <summary>
        /// 隐藏
        /// </summary>
        protected virtual void onHide() { }
        /// <summary>
        /// 退出
        /// </summary>
        protected virtual void onExit() { }
        protected virtual void executeDefaultAction(EventBase evt)
        {
            _internalGraphView._ExecuteDefaultAction(evt);
        }
        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void buildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("创建/节点", m_onCreateNodeWindow, DropdownMenuAction.AlwaysEnabled);
            if (CategoryModel.UniqueNodeCategories.Count > 0)
            {
                evt.menu.AppendAction("创建/唯一节点", m_onCreateUniqueNodeWindow, DropdownMenuAction.AlwaysEnabled);
            }

            evt.menu.AppendAction("创建/分组", m_onCreateGroup, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("创建/便笺", m_onCreateSticky, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendSeparator();
            if (View.selection.OfType<Node>().Count() > 1)
            {
                evt.menu.AppendAction("添加模板", (e) => AddGraphTemplate(), DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
            }

            evt.menu.AppendAction("保存", m_onSave, DropdownMenuAction.AlwaysEnabled);

            foreach (var item in CategoryModel.FormatCategories)
            {
                evt.menu.AppendAction("导出/" + item.FormatName, m_onFormatCallback, DropdownMenuAction.AlwaysEnabled, item);
            }
        }
        protected virtual void onTopToolbarGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(editorInfo.Title, "LargeBoldLabel");
            bool value = GUILayout.Toggle(ShowMiniMap, "小地图", "ButtonMid", GUILayout.MaxWidth(48));
            if (value != ShowMiniMap)
            {
                ShowMiniMap = value;
            }
            GUILayout.Space(2);
            value = GUILayout.Toggle(ShowGrid, "网格", "ButtonMid", GUILayout.MaxWidth(48));
            if (value != ShowGrid)
            {
                ShowGrid = value;
            }
            GUILayout.Space(2);
            value = GUILayout.Toggle(CanZoom, "缩放", "ButtonMid", GUILayout.MaxWidth(48));
            if (value != CanZoom)
            {
                CanZoom = value;
            }
            if (CanZoom)
            {
                float scale = EditorGUILayout.Slider(editorInfo.Scale, this.ZoomMinScale, this.ZoomMaxScale, GUILayout.MaxWidth(128));
                if (scale != this.View.scale)
                {
                    Vector3 position = this.View.viewTransform.position;
                    this.View.UpdateViewTransform(position, new Vector3(scale, scale, 1));
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// 绘制下方工具栏
        /// </summary>
        protected virtual void onBottomToolbarGUI()
        {
            //GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //GUILayout.Label("disconnect", "ErrorLabel");
            //GUILayout.EndHorizontal();
        }
        protected virtual void onKeyDownEvent(KeyDownEvent evt)
        {
            foreach (var item in _keyEvents)
            {
                if (item.IsCtrl != evt.ctrlKey || item.IsShift != evt.shiftKey || item.IsAlt != evt.altKey)
                    continue;
                if (item.Code == evt.keyCode)
                {
                    item.Execute(evt, this);
                    evt.StopPropagation();
                    break;
                }
            }
        }
        protected virtual void onViewTransformChanged(GraphView graphView)
        {
            editorInfo.Pos = graphView.viewTransform.position;
            editorInfo.Scale = graphView.scale;
        }
        /// <summary>
        /// 视图变化
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        protected virtual GraphViewChange onGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                bool removeVarNode = false;
                for (int i = graphViewChange.elementsToRemove.Count - 1; i >= 0; i--)
                {
                    GraphElement item = graphViewChange.elementsToRemove[i];
                    switch (item)
                    {
                        case MicroEdgeView microEdgeView:
                            m_removeEdge(microEdgeView, ref i, ref graphViewChange.elementsToRemove);
                            break;
                        case BaseMicroNodeView.InternalNodeView nodeView:
                            m_removeNodeView(nodeView.nodeView, ref i, ref graphViewChange.elementsToRemove);
                            break;
                        case MicroVariableNodeView.InternalNodeView variableView:
                            m_removeVarNodeView(variableView.nodeView, ref i, ref graphViewChange.elementsToRemove);
                            removeVarNode = true;
                            break;
                        case MicroVariableItemView variableItemView:
                            m_removeVarItemView(variableItemView, ref i, ref graphViewChange.elementsToRemove);
                            break;
                        case MicroGroupView groupView:
                            m_removeGroup(groupView, ref i, ref graphViewChange.elementsToRemove);
                            break;
                        case MicroStickyNoteView stickyNoteView:
                            m_removeStickyNoteView(stickyNoteView, ref i, ref graphViewChange.elementsToRemove);
                            break;
                        default:
                            break;
                    }
                }
                if (removeVarNode)
                    listener.OnEvent(MicroGraphEventIds.VAR_NODE_MODIFY);
            }

            #region Move
            if (graphViewChange.movedElements != null)
            {
                IMicroGraphRecordCommand record;
                foreach (var item in graphViewChange.movedElements)
                {
                    switch (item)
                    {
                        case BaseMicroNodeView.InternalNodeView nodeView:
                            record = new MicroNodeMoveRecord();
                            record.Record(this, nodeView.nodeView);
                            _undo?.AddCommand(record);
                            break;
                        case MicroVariableNodeView.InternalNodeView variableView:
                            record = new MicroVarNodeMoveRecord();
                            record.Record(this, variableView.nodeView);
                            _undo?.AddCommand(record);
                            break;
                        case MicroGroupView nodeView:
                            record = new MicroGroupMoveRecord();
                            record.Record(this, nodeView);
                            _undo?.AddCommand(record);
                            break;
                        case MicroStickyNoteView stickyNote:
                            record = new MicroStickyMoveRecord();
                            record.Record(this, stickyNote);
                            _undo?.AddCommand(record);
                            break;
                    }
                }
            }
            #endregion
            return graphViewChange;
        }

        /// <summary>
        /// 获取可以用节点类型<BaseMicroNode>
        /// <para>如果返回空，则可用所有节点类型</para>
        /// <para>如果一个节点类型同时出现在可用和不可用中，则该图不可以用该类型节点</para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual List<Type> getUsableNodeTypes() { return null; }

        /// <summary>
        /// 获取不可用节点类型<BaseMicroNode>（优先级最高）
        /// <para>如果返回空，则可用所有节点类型</para>
        /// <para>如果一个节点类型同时出现在可用和不可用中，则该图不可以用该节点类型</para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual List<Type> getUnusableNodeTypes() { return null; }
        /// <summary>
        /// 获取只能创建一个的节点类型<BaseMicroNode>
        /// <para>如果返回空，则所有节点均可被创建多次</para>
        /// <para>如果一个节点类型同时出现在唯一和不可用中，则该图不可以用该节点类型</para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual List<Type> getUniqueNodeTypes() { return null; }

        /// <summary>
        /// 获取可以用变量类型(真实类型，例如int,bool等)（优先级最高）
        /// <para>如果返回空，则可用所有变量类型</para>
        /// <para>如果一个变量类型同时出现在可用和不可用中，则该图不可以用该变量类型</para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual List<Type> getUsableVarableTypes() { return null; }

        /// <summary>
        /// 获取不可用变量类型(真实类型，例如int,bool等)
        /// <para>如果返回空，则可用所有变量类型</para>
        /// <para>如果一个变量类型同时出现在可用和不可用中，则该图不可以用该变量类型</para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual List<Type> getUnusableVarableTypes() { return null; }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        protected internal virtual void onUpdate() { }
    }

    //private
    partial class BaseMicroGraphView
    {
        private List<Port> getCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            _canLinkPorts.Clear();
            if (startPort.direction == Direction.Input)
            {
                goto End;
            }
            if (startPort is MicroPort.InternalPort sourcePort)
            {
                foreach (var port in View.ports)
                {
                    if (port.direction == Direction.Output)
                    {
                        continue;
                    }
                    if (sourcePort.node == port.node)
                    {
                        continue;
                    }
                    if (sourcePort.connections.FirstOrDefault(a => a.input == port) != null)
                    {
                        continue;
                    }
                    var tarPort = port as MicroPort.InternalPort;
                    if (tarPort == null)
                    {
                        continue;
                    }
                    bool isResult = sourcePort.microPort.CanConnectPort(tarPort.microPort);
                    if (isResult)
                        isResult = tarPort.microPort.CanConnectPort(sourcePort.microPort);
                    if (isResult)
                        _canLinkPorts.Add(tarPort);
                }
            }
        End: return _canLinkPorts;
        }
        private BaseMicroNodeView m_showNodeView(MicroNodeEditorInfo editorInfo)
        {
            Type nodeType = editorInfo.Target.GetType();
            NodeCategoryModel nodeCategory = _operateModel.categoryModel.GetNodeCategory(nodeType);
            BaseMicroNodeView nodeView = Activator.CreateInstance(MicroGraphProvider.GetNodeViewType(nodeCategory)) as BaseMicroNodeView;
            this.AddElement(nodeView);
            nodeView.Initialize(this, editorInfo, nodeCategory);
            m_addChildElement(editorInfo.NodeId, nodeView);
            return nodeView;
        }
        private MicroVariableNodeView m_showVarNodeView(MicroVariableNodeEditorInfo varNodeEditor)
        {
            MicroVariableNodeView variableNodeView = new MicroVariableNodeView();
            variableNodeView.Initialize(this, varNodeEditor);
            this.AddElement(variableNodeView);
            m_addChildElement(varNodeEditor.NodeId, variableNodeView);
            return variableNodeView;
        }
        private MicroGroupView m_showGroupView(MicroGroupEditorInfo group)
        {
            MicroGroupView groupView = new MicroGroupView();
            groupView.Initialize(this, group);
            this.AddElement(groupView);
            m_addChildElement(group.GroupId, groupView);
            return groupView;
        }
        private MicroStickyNoteView m_showStickyView(MicroStickyEditorInfo sticky)
        {
            MicroStickyNoteView stickyView = new MicroStickyNoteView();
            stickyView.Initialize(this, sticky);
            this.AddElement(stickyView);
            m_addChildElement(sticky.NodeId, stickyView);
            return stickyView;
        }
        private void m_addChildElement(int onlyId, GraphElement element)
        {
            if (_childElements.TryGetValue(onlyId, out GraphElement value))
            {
                throw new ArgumentException($"唯一ID重复：{onlyId},已存在：{value.GetType().Name}, 新添加：{element.GetType().Name}");
            }
            _childElements.Add(onlyId, element);
        }
        private void m_onCreateGroup(DropdownMenuAction obj)
        {
            Vector2 screenPos = owner.GetScreenPosition(obj.eventInfo.mousePosition);
            //经过计算得出节点的位置
            var windowMousePosition = owner.rootVisualElement.ChangeCoordinatesTo(owner.rootVisualElement.parent, screenPos - owner.position.position);
            var groupPos = this.View.contentViewContainer.WorldToLocal(windowMousePosition);
            MicroGroupEditorInfo group = new MicroGroupEditorInfo();
            group.GroupId = editorInfo.GetUniqueId();
            group.Pos = groupPos;
            group.Title = "默认分组";
            group.GroupColor = UnityEngine.Random.ColorHSV(0, 1, 0.58f, 0.58f, 0.8f, 0.8f);
            var groupView = AddGroupView(group);
            groupView.schedule.Execute(() =>
            {
                foreach (var item in View.selection)
                {
                    if (item is GraphElement element)
                    {
                        groupView.AddElement(element);
                    }
                }
            }).StartingIn(2);
        }
        private void m_onCreateSticky(DropdownMenuAction obj)
        {
            Vector2 screenPos = owner.GetScreenPosition(obj.eventInfo.mousePosition);
            //经过计算得出节点的位置
            var windowMousePosition = owner.rootVisualElement.ChangeCoordinatesTo(owner.rootVisualElement.parent, screenPos - owner.position.position);
            var stickyPos = this.View.contentViewContainer.WorldToLocal(windowMousePosition);
            MicroStickyEditorInfo sticky = new MicroStickyEditorInfo();
            sticky.NodeId = editorInfo.GetUniqueId();
            sticky.Pos = stickyPos;
            sticky.Size = new Vector2(140, 120);
            sticky.Theme = 0;
            sticky.FontStyle = 0;
            sticky.FontSize = 12;
            var stickyView = AddStickyNodeView(sticky);
        }
        private void m_onCreateNodeWindow(DropdownMenuAction obj)
        {
            if (_createNodeWindow == null)
            {
                _createNodeWindow = ScriptableObject.CreateInstance<MicroCreateNodeWindow>();
                _createNodeWindow.onSelectHandler += m_onCreateNodeSelectEntry;
                _createNodeWindow.Initialize(this, _operateModel.categoryModel);
            }

            Vector2 screenPos = owner.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), _createNodeWindow);
        }
        private void m_onCreateUniqueNodeWindow(DropdownMenuAction obj)
        {
            if (_createUniqueNodeWindow == null)
            {
                _createUniqueNodeWindow = ScriptableObject.CreateInstance<MicroCreateNodeWindow>();
                _createUniqueNodeWindow.onSelectHandler += m_onCreateNodeSelectEntry;
                _createUniqueNodeWindow.Initialize(this, _operateModel.categoryModel, true);
            }

            Vector2 screenPos = owner.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), _createUniqueNodeWindow);
        }
        private bool m_onCreateNodeSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            //经过计算得出节点的位置
            var windowMousePosition = this.View.ChangeCoordinatesTo(this, context.screenMousePosition - owner.position.position);
            var nodePosition = this.View.contentViewContainer.WorldToLocal(windowMousePosition);
            if (entry.userData is NodeCategoryModel nodeCategory)
            {
                if (CategoryModel.UniqueNodeCategories.Contains(nodeCategory))
                {
                    BaseMicroNode node = Target.Nodes.FirstOrDefault(a => a.GetType() == nodeCategory.NodeClassType);
                    if (node != null)
                    {
                        Node nodeVew = this.GetElement<Node>(node.OnlyId);
                        this.View.AddToSelection(nodeVew);
                        this.View.FrameSelection();
                        this.owner.ShowNotification(new GUIContent("唯一节点不允许重复创建"), NOTIFICATION_TIME);
                        return false;
                    }
                }
                AddNode(nodeCategory.NodeClassType, nodePosition);
            }

            return true;
        }
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="obj"></param>
        private void m_onFormatCallback(DropdownMenuAction obj)
        {
            FormatCategoryModel model = (FormatCategoryModel)obj.userData;
            string path = EditorUtility.SaveFilePanel("格式化微图", Application.dataPath, model.FormatName, model.Extension);
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return;
            }
            try
            {
                model.ToFormat(this.Target, path);
            }
            catch (Exception ex)
            {
                UnityDebug.LogError(ex.Message);
            }
        }
        private void m_onSave(DropdownMenuAction action)
        {
            this.Save();
        }
        /// <summary>
        /// 设置唯一Id
        /// </summary>
        /// <param name="node"></param>
        private void m_setNodeId(BaseMicroNode node)
        {
            var field = typeof(BaseMicroNode).GetField("_onlyId", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(node, editorInfo.GetUniqueId());
            }
            else
            {
                UnityDebug.LogError("节点没有找到字段");
            }
        }
        private void m_initNodeId()
        {
            int id = 0;
            if (Target.Nodes.Count > 0)
            {
                id = Mathf.Max(Target.Nodes.Max(a => a.OnlyId), id);
            }
            if (editorInfo.VariableNodes.Count > 0)
            {
                id = Mathf.Max(editorInfo.VariableNodes.Max(a => a.NodeId), id);
            }
            if (editorInfo.Groups.Count > 0)
            {
                id = Mathf.Max(editorInfo.Groups.Max(a => a.GroupId), id);
            }
            if (editorInfo.Stickys.Count > 0)
            {
                id = Mathf.Max(editorInfo.Stickys.Max(a => a.NodeId), id);
            }
            editorInfo.SetUniqueId(id);
        }
        /// <summary>
        /// 拖拽更新函数
        /// </summary>
        /// <param name="evt"></param>       
        private void m_onDragUpdatedEvent(DragUpdatedEvent evt)
        {
            List<ISelectable> dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;
            if (dragData != null)
                foreach (var item in dragData)
                {
                    if (item is MicroVariableItemView)
                    {
                        dragging = true;
                        break;
                    }
                    if (item is MicroTemplateElement)
                    {
                        dragging = true;
                        break;
                    }
                }
            if (dragging)
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }
        /// <summary>
        /// 如果有拖拽对象 拖拽松开操作
        /// </summary>
        /// <param name="evt"></param>
        private void m_onDragPerformEvent(DragPerformEvent evt)
        {
            var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(View.contentViewContainer, evt.localMousePosition);
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (dragData != null)
            {
                List<MicroVariableItemView> exposedFieldViews = dragData.OfType<MicroVariableItemView>().ToList();
                foreach (MicroVariableItemView varFieldView in exposedFieldViews)
                {
                    if (varFieldView.owner != this)
                    {
                        owner.ShowNotification(new GUIContent("无法跨图拖拽变量"), NOTIFICATION_TIME);
                        continue;
                    }
                    AddVariableNodeView(varFieldView.editorInfo.Target, mousePos);
                }

                List<MicroTemplateElement> templateList = dragData.OfType<MicroTemplateElement>().ToList();
                foreach (MicroTemplateElement template in templateList)
                {
                    if (template.owner != this)
                    {
                        owner.ShowNotification(new GUIContent("无法跨图使用模板"), NOTIFICATION_TIME);
                        continue;
                    }
                    m_restoreTemplate(template, mousePos);
                }
            }
        }
        private void m_nodeCreateRequest(NodeCreationContext context)
        {
            if (_createNodeWindow == null)
            {
                _createNodeWindow = ScriptableObject.CreateInstance<MicroCreateNodeWindow>();
                _createNodeWindow.onSelectHandler += m_onCreateNodeSelectEntry;
                _createNodeWindow.Initialize(this, _operateModel.categoryModel);
            }

            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _createNodeWindow);
        }
        /// <summary>
        /// 还原模板
        /// </summary>
        /// <param name="template"></param>
        private void m_restoreTemplate(MicroTemplateElement template, Vector2 mousePos)
        {
            MicroGraphTemplateModel model = template.templateModel;
            bool hasNewVar = false;
            bool needCreateVar = false;
            foreach (var item in model.Vars)
            {
                MicroVariableEditorInfo variable = editorInfo.Variables.FirstOrDefault(a => a.Name == item.VarName);
                if (variable == null)
                    hasNewVar = true;
                else if (variable.Target.GetValueType().FullName != item.VarClassName)
                {
                    string[] strs = variable.Target.GetValueType().FullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    string targetName = strs[strs.Length - 1];
                    strs = item.VarClassName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    string templateName = strs[strs.Length - 1];
                    owner.ShowNotification(new GUIContent($"变量:{item.VarName}已存在, 但类型不一致, 无法使用改模板\n当前类型:{targetName}, 模板类型:{templateName}"), NOTIFICATION_TIME);
                    return;
                }
            }
            if (hasNewVar)
                needCreateVar = EditorUtility.DisplayDialog("提示", "是否创建该模板需要的变量", "是", "否");
            MicroTemplateOperateData operateData = new MicroTemplateOperateData();
            operateData.model = model;
            operateData.mousePos = mousePos;
            operateData.view = this;
            Rect calcRect = new Rect();
            m_calculateCenter();
            operateData.centerPos = calcRect.center;
            if (needCreateVar)
            {
                foreach (MicroVarSerializeModel item in model.Vars)
                {
                    MicroVariableEditorInfo variable = editorInfo.Variables.FirstOrDefault(a => a.Name == item.VarName);
                    if (variable != null)
                        continue;
                    MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
                }
            }
            foreach (MicroNodeSerializeModel item in model.Nodes)
                MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
            foreach (MicroVarNodeSerializeModel item in model.VarNodes)
                MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
            foreach (MicroStickySerializeModel item in model.Stickys)
                MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
            foreach (MicroGroupSerializeModel item in model.Groups)
                MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
            foreach (MicroEdgeSerializeModel item in model.Edges)
                MicroGraphProvider.GetRestoreTemplateImpl(item.GetType())?.Restore(operateData, item);
            owner.ShowNotification(new GUIContent("使用模板成功"), NOTIFICATION_TIME);
            void m_calculateCenter()
            {
                calcRect.xMax = float.MinValue;
                calcRect.yMax = float.MinValue;
                calcRect.xMin = float.MaxValue;
                calcRect.yMin = float.MaxValue;
                foreach (var item in model.Nodes)
                    m_comparePos(item.Pos);
                foreach (var item in model.VarNodes)
                    m_comparePos(item.Pos);
                foreach (var item in model.Stickys)
                    m_comparePos(item.Pos);
            }
            void m_comparePos(Vector2 pos)
            {
                Vector2 min = pos;
                Vector2 max = pos;
                if (calcRect.min.x > min.x)
                    calcRect.xMin = min.x;
                if (calcRect.min.y > min.y)
                    calcRect.yMin = min.y;

                if (calcRect.max.x < max.x)
                    calcRect.xMax = max.x;
                if (calcRect.max.y < max.y)
                    calcRect.yMax = max.y;
            }
        }

        /// <summary>
        /// 删除NodeView
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="index"></param>
        /// <param name="elementsToRemove"></param>
        private void m_removeNodeView(BaseMicroNodeView nodeView, ref int index, ref List<GraphElement> elementsToRemove)
        {
            this.Target.Nodes.Remove(nodeView.Target);
            this.editorInfo.Nodes.Remove(nodeView.editorInfo);
            this._childElements.Remove(nodeView.editorInfo.NodeId);
            foreach (MicroPort element in nodeView.microPorts)
            {
                foreach (var item in element.view.connections)
                {
                    if (elementsToRemove.Contains(item))
                        continue;
                    elementsToRemove.Insert(0, item);
                    index++;
                }
            }
            IMicroGraphRecordCommand record = new MicroNodeDeleteRecord();
            record.Record(this, nodeView);
            _undo?.AddCommand(record);
        }
        private void m_removeVarNodeView(MicroVariableNodeView varNodeView, ref int index, ref List<GraphElement> elementsToRemove)
        {
            this.editorInfo.VariableNodes.Remove(varNodeView.editorInfo);
            this._childElements.Remove(varNodeView.editorInfo.NodeId);
            varNodeView.OnDestory();
            IMicroGraphRecordCommand record = new MicroVarNodeDeleteRecord();
            record.Record(this, varNodeView);
            _undo?.AddCommand(record);
        }
        private void m_removeVarItemView(MicroVariableItemView varItemView, ref int index, ref List<GraphElement> elementsToRemove)
        {
            if (!varItemView.editorInfo.CanDelete)
            {
                owner.ShowNotification(new GUIContent("该变量不允许删除"), NOTIFICATION_TIME);
                elementsToRemove.Remove(varItemView);
                return;
            }

            this.Target.Variables.Remove(varItemView.editorInfo.Target);
            this.editorInfo.Variables.Remove(varItemView.editorInfo);
            BlackboardRow blackboardRow = varItemView.GetFirstAncestorOfType<BlackboardRow>();
            if (blackboardRow != null)
                elementsToRemove.Add(blackboardRow);
            var varNodeList = this.editorInfo.VariableNodes.Where(a => a.Name == varItemView.editorInfo.Target.Name);
            foreach (var varNode in varNodeList)
            {
                if (_childElements.TryGetValue(varNode.NodeId, out GraphElement node))
                {
                    MicroVariableNodeView.InternalNodeView nodeView = (MicroVariableNodeView.InternalNodeView)node;
                    if (nodeView != null)
                    {
                        foreach (var item in nodeView.nodeView.Input.view.connections)
                        {
                            if (elementsToRemove.Contains(item))
                                continue;
                            elementsToRemove.Insert(0, item);
                            index++;
                        }
                        foreach (var item in nodeView.nodeView.OutPut.view.connections)
                        {
                            if (elementsToRemove.Contains(item))
                                continue;
                            elementsToRemove.Insert(0, item);
                            index++;
                        }
                    }
                    elementsToRemove.Insert(0, node);
                    index++;
                }
            }
            varItemView.OnDestory();
            IMicroGraphRecordCommand record = new MicroVarDeleteRecord();
            record.Record(this, varItemView);
            _undo?.AddCommand(record);
        }
        private void m_removeEdge(MicroEdgeView microEdgeView, ref int index, ref List<GraphElement> elementsToRemove)
        {
            MicroPort.InternalPort input = microEdgeView.input as MicroPort.InternalPort;
            MicroPort.InternalPort output = microEdgeView.output as MicroPort.InternalPort;
            input.microPort.Disonnect(output);
            output.microPort.Disonnect(input);
            IMicroGraphRecordCommand record = new MicroEdgeDeleteRecord();
            record.Record(this, microEdgeView);
            _undo?.AddCommand(record);
        }
        private void m_removeGroup(MicroGroupView groupView, ref int index, ref List<GraphElement> elementsToRemove)
        {
            this.editorInfo.Groups.Remove(groupView.editorInfo);
            this._childElements.Remove(groupView.editorInfo.GroupId);
            IMicroGraphRecordCommand record = new MicroGroupDeleteRecord();
            record.Record(this, groupView);
            _undo?.AddCommand(record);
        }
        private void m_removeStickyNoteView(MicroStickyNoteView stickyNoteView, ref int i, ref List<GraphElement> elementsToRemove)
        {
            this.editorInfo.Stickys.Remove(stickyNoteView.editorInfo);
            this._childElements.Remove(stickyNoteView.editorInfo.NodeId);
            stickyNoteView.OnDestory();
            IMicroGraphRecordCommand record = new MicroStickyDeleteRecord();
            record.Record(this, stickyNoteView);
            _undo?.AddCommand(record);
        }
        private bool m_onVarModifyCallback(object args)
        {
            if (args is not VarModifyEventArgs varModify)
                return true;
            foreach (var node in Target.Nodes)
            {
                foreach (var item in node.VariableEdges)
                {
                    if (item.varName == varModify.oldVarName)
                    {
                        item.varName = varModify.var.Name;
                    }
                }
            }
            return true;
        }
    }

    //内部Class
    partial class BaseMicroGraphView
    {
        internal class InternalGraphView : GraphView
        {
            internal readonly BaseMicroGraphView graphView;

            public bool CanCopy => selection.Any((ISelectable s) => s is Node || s is Group || s is Placemat || s is MicroStickyNoteView || s is StickyNote || s is MicroVariableItemView);

            internal InternalGraphView(BaseMicroGraphView baseGraphView)
            {
                this.AddToClassList("internalGraphView");
                graphView = baseGraphView;
                this.name = baseGraphView.GetType().Name;
                this.focusable = true;

                //会移除默认的快捷键
                //this.isReframable = false;
                //MethodInfo method = typeof(GraphView).GetMethod("OnExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                //var o = (EventCallback<ExecuteCommandEvent>)Delegate.CreateDelegate(typeof(EventCallback<ExecuteCommandEvent>), this, method);
                //UnregisterCallback<ExecuteCommandEvent>(o);
            }
            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            {
                return graphView.getCompatiblePorts(startPort, nodeAdapter);
            }
            public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                graphView.buildContextualMenu(evt);
            }
            protected override void ExecuteDefaultAction(EventBase evt)
            {
                graphView.executeDefaultAction(evt);
            }
            internal void _ExecuteDefaultAction(EventBase evt)
            {
                base.ExecuteDefaultAction(evt);
            }
            public override void AddToSelection(ISelectable selectable)
            {
                base.AddToSelection(selectable);
                try
                {
                    graphView.onSelectChanged?.Invoke(this.selection);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
            public override void RemoveFromSelection(ISelectable selectable)
            {
                base.RemoveFromSelection(selectable);
                try
                {
                    graphView.onSelectChanged?.Invoke(this.selection);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
            public override EventPropagation DeleteSelection()
            {
                var result = base.DeleteSelection();
                try
                {
                    graphView.onSelectChanged?.Invoke(this.selection);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                return result;
            }
            public override void ClearSelection()
            {
                base.ClearSelection();
                try
                {
                    graphView.onSelectChanged?.Invoke(this.selection);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            /// <summary>
            /// 显示背景网格
            /// </summary>
            internal void m_showGrid()
            {
                //添加网格背景
                GridBackground gridBackground = new InternalGraphGrid();
                gridBackground.name = "___GridBackground";
                Insert(0, gridBackground);
            }
            /// <summary>
            /// 隐藏Grid
            /// </summary>
            internal void m_hideGrid()
            {
                GridBackground gridBackground = this.Q<GridBackground>("___GridBackground");
                if (gridBackground != null)
                {
                    gridBackground.RemoveFromHierarchy();
                }
            }
            private class InternalGraphGrid : GridBackground { }
        }

        public static implicit operator GraphView(BaseMicroGraphView graphView)
        {
            return graphView.View;
        }
    }
}
