using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图信息提供者
    /// </summary>
    public static partial class MicroGraphProvider
    {
        /// <summary>
        /// 类型空集合
        /// </summary>
        private readonly static List<Type> EMPTY_TYPE_LIST = new List<Type>();
        private readonly static Assembly CUR_ASSEMBLY = typeof(BaseMicroNodeView).Assembly;
        private readonly static Type DEFAULT_NODE_VIEW_TYPE = typeof(BaseMicroNodeView);
        private static List<GraphSummaryModel> _graphSummaryList = new List<GraphSummaryModel>();
        /// <summary>
        /// 逻辑图对象的简介和编辑器信息缓存
        /// </summary>
        public static List<GraphSummaryModel> GraphSummaryList => _graphSummaryList;

        private static List<GraphCategoryModel> _graphCategoryList = new List<GraphCategoryModel>();
        /// <summary>
        /// 图分类信息缓存
        /// </summary>
        public static List<GraphCategoryModel> GraphCategoryList => _graphCategoryList;

        /// <summary>
        /// 缓存所有节点
        /// <para>key 节点类型</para>
        /// <para>value: 节点信息</para>
        /// </summary>
        private static Dictionary<Type, NodeCategoryModel> _allNodeCategoryMapping = new Dictionary<Type, NodeCategoryModel>();
        /// <summary>
        /// 所有节点
        /// <para>key 节点类型</para>
        /// <para>value: 节点信息</para>
        /// </summary>
        public static Dictionary<Type, NodeCategoryModel> NodeCategoryMapping => _allNodeCategoryMapping;
        /// <summary>
        /// 缓存所有变量
        /// <para>Key：变量真实类型</para>
        /// </summary>
        private static Dictionary<Type, VariableCategoryModel> _allVarCategoryMapping = new Dictionary<Type, VariableCategoryModel>();

        /// <summary>
        /// 节点字段元素显示类型映射
        /// </summary>
        private static Dictionary<Type, Type> _nodeElementMapping = new Dictionary<Type, Type>();

        /// <summary>
        /// 粘贴复制的类型映射
        /// <para>Type：IMicroGraphCopyPaste的Type</para>
        /// </summary>
        private static Dictionary<Type, Type> _copyPasteMapping = new Dictionary<Type, Type>();

        /// <summary>
        /// 记录模板映射
        /// </summary>
        private static Dictionary<Type, IMicroGraphTemplate> _recordTemplateMapping = new Dictionary<Type, IMicroGraphTemplate>();
        /// <summary>
        /// 还原模板映射
        /// </summary>
        private static Dictionary<Type, IMicroGraphTemplate> _restoreTemplateMapping = new Dictionary<Type, IMicroGraphTemplate>();

        /// <summary>
        /// 所有按键信息
        /// </summary>
        private static List<Type> _allKeyEvents = new List<Type>();

        /// <summary>
        /// 所有按键信息
        /// </summary>
        public static List<Type> AllKeyEvents => _allKeyEvents;

        /// <summary>
        /// 获取节点字段元素显示类型
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static Type GetNodeElementType(Type type)
        {
            _nodeElementMapping.TryGetValue(type, out Type elementValue);

            if (type.IsEnum && elementValue == null)
                _nodeElementMapping.TryGetValue(typeof(Enum), out elementValue);
            return elementValue;
        }

        /// <summary>
        /// 获取图的简介和编辑器信息
        /// </summary>
        /// <param name="logic"></param>
        /// <returns></returns>
        public static GraphSummaryModel GetGraphSummary(BaseMicroGraph logic) => GetGraphSummary(logic.OnlyId);
        /// <summary>
        /// 获取图的简介和编辑器信息
        /// </summary>
        /// <param name="onlyId"></param>
        /// <returns></returns>
        public static GraphSummaryModel GetGraphSummary(string onlyId) => GraphSummaryList.FirstOrDefault(a => a.OnlyId == onlyId);
        /// <summary>
        /// 获得对应逻辑图的分类信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static GraphCategoryModel GetGraphCategory(GraphSummaryModel info) => GetGraphCategory(info.GraphClassName);
        /// <summary>
        /// 获得对应图的分类信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static GraphCategoryModel GetGraphCategory(BaseMicroGraph info) => GetGraphCategory(info.GetType().FullName);
        /// <summary>
        /// 获得对应图的分类信息
        /// </summary>
        /// <param name="fullClassName"></param>
        /// <returns></returns>
        public static GraphCategoryModel GetGraphCategory(string fullClassName) => GraphCategoryList.FirstOrDefault(a => a.GraphType.FullName == fullClassName);
        /// <summary>
        /// 获取变量对应的信息
        /// </summary>
        /// <param name="varType"></param>
        /// <returns></returns>
        public static VariableCategoryModel GetVariableCategory(Type varType) => _allVarCategoryMapping.ContainsKey(varType) ? _allVarCategoryMapping[varType] : null;
        /// <summary>
        /// 获取节点的视图类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Type GetNodeViewType(NodeCategoryModel model)
        {
            Type viewType = model.ViewClassType;
            if (viewType != DEFAULT_NODE_VIEW_TYPE)
                return viewType;
            Type nodeType = model.NodeClassType.BaseType;
            while (nodeType != null)
            {
                if (!_allNodeCategoryMapping.TryGetValue(nodeType, out var temp))
                    break;
                viewType = temp.ViewClassType;
                if (viewType != DEFAULT_NODE_VIEW_TYPE)
                    break;
                nodeType = nodeType.BaseType;
            }
            return viewType;
        }
        /// <summary>
        /// 获取粘贴复制的实现类
        /// </summary>
        /// <param name="type">需要粘贴复制的对象类型</param>
        /// <returns></returns>
        public static IMicroGraphCopyPaste GetCopyPasteImpl(Type type)
        {
            Type copyType = null;
            Type temp = type;
            while (temp != null)
            {
                if (_copyPasteMapping.TryGetValue(temp, out copyType))
                    break;
                temp = temp.BaseType;
            }
            if (copyType != null)
            {
                return Activator.CreateInstance(copyType) as IMicroGraphCopyPaste;
            }
            return null;
        }
        /// <summary>
        /// 获取记录模板实现类
        /// </summary>
        /// <returns></returns>
        internal static IMicroGraphTemplate GetRecordTemplateImpl(Type type)
        {
            IMicroGraphTemplate template = null;
            if (_recordTemplateMapping.TryGetValue(type, out template))
                return template;
            return null;
        }
        /// <summary>
        /// 获取还原模板实现类
        /// </summary>
        /// <returns></returns>
        internal static IMicroGraphTemplate GetRestoreTemplateImpl(Type type)
        {
            IMicroGraphTemplate template = null;
            if (_restoreTemplateMapping.TryGetValue(type, out template))
                return template;
            return null;
        }
        /// <summary>
        /// 添加一个新的图简介
        /// </summary>
        /// <returns></returns>
        [Obsolete("此方法不能调用", true)]
        internal static bool AddSummaryModel(string path, BaseMicroGraph graph)
        {
            graph.ResetGUID();
            MicroGraphEditorInfo editorInfo = graph.editorInfo;
            editorInfo.Title = graph.name;
            editorInfo.CreateTime = DateTime.Now;
            editorInfo.ModifyTime = DateTime.Now;
            GraphSummaryModel graphCache = new GraphSummaryModel();
            graphCache.GraphClassName = graph.GetType().FullName;
            graphCache.AssetPath = path;
            graphCache.OnlyId = graph.OnlyId;
            graphCache.MicroName = graph.name;
            graphCache.SetEditorInfo(editorInfo);
            GraphSummaryList.Add(graphCache);
            return true;
        }
        /// <summary>
        /// 在简介中移动图
        /// </summary>
        /// <param name="path"></param>
        /// <param name="graph"></param>
        [Obsolete("此方法不能调用", true)]
        internal static bool MoveSummaryModel(string path, BaseMicroGraph graph)
        {
            var catalog = GraphSummaryList.FirstOrDefault(a => a.OnlyId == graph.OnlyId);
            if (catalog != null)
                catalog.AssetPath = path;
            return true;
        }
        /// <summary>
        /// 在简介中删除逻辑图
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [Obsolete("此方法不能调用", true)]
        internal static bool DeleteSummaryModel(string path)
        {
            var catalog = GraphSummaryList.FirstOrDefault(a => a.AssetPath == path);
            if (catalog != null)
                GraphSummaryList.Remove(catalog);
            return true;
        }

        /// <summary>
        /// 初始化微图分类
        /// </summary>

        internal static void InitGraphCategory(BaseMicroGraphView graphView)
        {
            InitGraphCategory(graphView, GetGraphCategory(graphView.Target));
        }

        internal static void InitGraphCategory(BaseMicroGraphView graphView, GraphCategoryModel graphCategory)
        {
            if (graphCategory == null || graphCategory.IsInit || graphCategory.NodeCategories.Count > 0)
                return;
            graphCategory.IsInit = true;
            List<Type> usableNodeTypes = graphView.getUsableNodeTypes() ?? EMPTY_TYPE_LIST;
            List<Type> unusableNodeTypes = graphView.getUnusableNodeTypes() ?? EMPTY_TYPE_LIST;
            List<Type> uniqueNodeTypes = graphView.getUniqueNodeTypes() ?? EMPTY_TYPE_LIST;
            List<Type> usableVarTypes = graphView.getUsableVarableTypes() ?? EMPTY_TYPE_LIST;
            List<Type> unusableVarTypes = graphView.getUnusableVarableTypes() ?? EMPTY_TYPE_LIST;
            List<Type> tempList = new List<Type>();

            #region 筛选节点类型

            //Assembly curAssembly = graphCategory.GraphType.Assembly;
            //AssemblyName curAssemblyName = curAssembly.GetName();
            // AssemblyName[] assemblies = graphCategory.GraphType.Assembly.GetReferencedAssemblies();
            bool isReference(Type type)
            {
                return true;
                //if (type.Assembly == curAssembly)
                //    return true;
                //AssemblyName[] assemblies = type.Assembly.GetReferencedAssemblies();
                //return assemblies.Contains(curAssemblyName);
            }

            if (usableNodeTypes.Count > 0)
            {
                tempList.AddRange(usableNodeTypes.Except(unusableNodeTypes));
                foreach (var type in tempList)
                {
                    if (_allNodeCategoryMapping.TryGetValue(type, out NodeCategoryModel nodeCategory))
                        graphCategory.NodeCategories.Add(nodeCategory);
                }
            }
            else if (unusableNodeTypes.Count > 0)
            {
                tempList.AddRange(_allNodeCategoryMapping.Keys.Except(unusableNodeTypes));
                foreach (var type in tempList)
                {
                    if (_allNodeCategoryMapping.TryGetValue(type, out NodeCategoryModel nodeCategory) && isReference(type))
                        graphCategory.NodeCategories.Add(nodeCategory);
                }
            }
            else
            {
                foreach (var item in _allNodeCategoryMapping.Values)
                {
                    if (isReference(item.NodeClassType))
                        graphCategory.NodeCategories.Add(item);
                }
            }
            if (_allNodeCategoryMapping.TryGetValue(typeof(MicroPackageNode),out NodeCategoryModel packageCategoryModel))
            {
                if (!graphCategory.NodeCategories.Contains(packageCategoryModel))
                {
                    graphCategory.NodeCategories.Add(packageCategoryModel);
                }
            }
            if (uniqueNodeTypes.Count > 0)
            {
                tempList.Clear();
                tempList.AddRange(uniqueNodeTypes.Except(unusableNodeTypes));
                foreach (var type in tempList)
                {
                    if (_allNodeCategoryMapping.TryGetValue(type, out NodeCategoryModel nodeCategory))
                        graphCategory.UniqueNodeCategories.Add(nodeCategory);
                }
            }
            #endregion

            #region 筛选变量类型

            tempList.Clear();
            if (usableVarTypes.Count > 0)
            {
                foreach (var item in usableVarTypes)
                {
                    VariableCategoryModel variableCategory = _allVarCategoryMapping.Values.FirstOrDefault(a => a.VarBoxType == item || a.VarType == item);
                    if (unusableVarTypes.Contains(variableCategory.VarType) || unusableVarTypes.Contains(variableCategory.VarBoxType))
                        continue;
                    graphCategory.VariableCategories.Add(variableCategory);
                }
            }
            else if (unusableVarTypes.Count > 0)
            {
                foreach (var item in _allVarCategoryMapping)
                {
                    if (unusableVarTypes.Contains(item.Value.VarType) || unusableVarTypes.Contains(item.Value.VarBoxType))
                        continue;
                    if (isReference(item.Value.VarBoxType))
                        graphCategory.VariableCategories.Add(item.Value);
                }
            }
            else
            {
                foreach (var item in _allVarCategoryMapping.Values)
                {
                    if (isReference(item.VarType))
                        graphCategory.VariableCategories.Add(item);
                }
            }

            #endregion

            graphCategory.NodeCategories.Sort((entry1, entry2) =>
            {
                for (var i = 0; i < entry1.NodeLayers.Length; i++)
                {
                    if (i >= entry2.NodeLayers.Length)
                        return 1;
                    var value = entry1.NodeLayers[i].CompareTo(entry2.NodeLayers[i]);
                    if (value != 0)
                    {
                        // Make sure that leaves go before nodes
                        if (entry1.NodeLayers.Length != entry2.NodeLayers.Length && (i == entry1.NodeLayers.Length - 1 || i == entry2.NodeLayers.Length - 1))
                            return entry1.NodeLayers.Length < entry2.NodeLayers.Length ? -1 : 1;
                        return value;
                    }
                }
                return 0;
            });
            graphCategory.UniqueNodeCategories.Sort((entry1, entry2) =>
            {
                for (var i = 0; i < entry1.NodeLayers.Length; i++)
                {
                    if (i >= entry2.NodeLayers.Length)
                        return 1;
                    var value = entry1.NodeLayers[i].CompareTo(entry2.NodeLayers[i]);
                    if (value != 0)
                    {
                        // Make sure that leaves go before nodes
                        if (entry1.NodeLayers.Length != entry2.NodeLayers.Length && (i == entry1.NodeLayers.Length - 1 || i == entry2.NodeLayers.Length - 1))
                            return entry1.NodeLayers.Length < entry2.NodeLayers.Length ? -1 : 1;
                        return value;
                    }
                }
                return 0;
            });
        }
        /// <summary>
        /// 刷新图简介
        /// </summary>
        internal static void RefreshGraphSummary()
        {
            foreach (var item in GraphSummaryList)
            {
                item.IsRefresh = false;
            }
            //GraphSummaryList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BaseMicroGraph");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseMicroGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(assetPath);
                if (logicGraph == null)
                    continue;
                string onlyId = logicGraph.OnlyId;
                GraphSummaryModel matched = GraphSummaryList.FirstOrDefault(a => a.AssetPath == assetPath && a.OnlyId == onlyId && !a.IsRefresh);
                if (matched != null)
                {
                    //全匹配匹配到了，表示是老的
                    matched.IsRefresh = true;
                    goto End;//释放资源
                }
                matched = GraphSummaryList.FirstOrDefault(a => a.AssetPath != assetPath && a.OnlyId == onlyId);
                if (matched != null)
                {
                    //匹配到了onlyId,没有匹配到路径，表示是移动的或者是复制的新的
                    BaseMicroGraph originGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(matched.AssetPath);
                    if (originGraph != null)
                    {
                        //因为在原本地方找到微图，表示现在是复制出来的
                        create(logicGraph, assetPath, true);
                        MicroGraphUtils.UnloadObject(originGraph);
                        goto End;//释放资源
                    }
                    //原始地方没有微图，onlyId又一致，表示是移动的
                    matched.AssetPath = assetPath;
                    matched.IsRefresh = true;
                    goto End;//释放资源
                }
                //全新微图
                create(logicGraph, assetPath, false);
            End: MicroGraphUtils.UnloadObject(logicGraph);
            }
            for (int i = GraphSummaryList.Count - 1; i >= 0; i--)
            {
                if (!GraphSummaryList[i].IsRefresh)
                {
                    //被删除的
                    GraphSummaryList.RemoveAt(i);
                }
            }
            void create(BaseMicroGraph graph, string assetPath, bool refreshId)
            {
                MicroGraphEditorInfo editorInfo = graph.editorInfo;
                GraphSummaryModel graphCache = new GraphSummaryModel();
                if (refreshId || string.IsNullOrWhiteSpace(graph.OnlyId))
                {
                    graph.ResetGUID();
                    editorInfo.Title = graph.name;
                    editorInfo.CreateTime = DateTime.Now;
                    editorInfo.ModifyTime = DateTime.Now;
                }
                graphCache.GraphClassName = graph.GetType().FullName;
                graphCache.AssetPath = assetPath;
                graphCache.OnlyId = graph.OnlyId;
                graphCache.SetEditorInfo(editorInfo);
                graphCache.IsRefresh = true;
                GraphSummaryList.Add(graphCache);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            }
        }
    }

    partial class MicroGraphProvider
    {
        static MicroGraphProvider()
        {
            s_nodeFieldMapping();
            s_buildGraphCache();
            s_buildNode();
            s_buildVariable();
            s_buildGraphSummary();
            s_buildGraphKeyEvent();
            s_buildGraphOperate();
            s_buildGraphFormat();
            s_buildGraphTemplate();
        }
    }

    partial class MicroGraphProvider
    {

        /// <summary>
        /// 节点元素类型映射
        /// </summary>
        private static void s_nodeFieldMapping()
        {
            TypeCache.TypeCollection nodeElements = TypeCache.GetTypesDerivedFrom<INodeFieldElement>();
            foreach (var nodeElement in nodeElements)
            {
                if (nodeElement.IsAbstract || nodeElement.IsInterface)
                    continue;
                MicroGraphEditorAttribute graphAttr = nodeElement.GetCustomAttribute<MicroGraphEditorAttribute>();
                if (graphAttr == null)
                    continue;
                if (_nodeElementMapping.ContainsKey(graphAttr.type))
                {
                    var tempType = _nodeElementMapping[graphAttr.type];
                    if (nodeElement.Assembly != CUR_ASSEMBLY)
                    {
                        _nodeElementMapping[graphAttr.type] = nodeElement;
                    }
                }
                else
                    _nodeElementMapping.Add(graphAttr.type, nodeElement);
            }
        }
        /// <summary>
        /// 生成图类型信息缓存
        /// </summary>
        private static void s_buildGraphCache()
        {
            TypeCache.TypeCollection graphViewTypes = TypeCache.GetTypesDerivedFrom<BaseMicroGraphView>();
            TypeCache.TypeCollection graphTypes = TypeCache.GetTypesDerivedFrom<BaseMicroGraph>();
            //循环查询微图
            foreach (var item in graphTypes)
            {
                if (item.IsAbstract)
                    continue;
                GraphCategoryModel graphData = new GraphCategoryModel();
                graphData.GraphColor = MicroGraphUtils.GetColor(item);
                graphData.GraphType = item;
                graphData.ViewType = typeof(BaseMicroGraphView);
                graphData.GraphName = item.Name;
                graphData.Index = GraphCategoryList.Count;
                MicroGraphAttribute graphAttr = item.GetCustomAttribute<MicroGraphAttribute>();
                if (graphAttr != null)
                {
                    Color color = graphData.GraphColor;
                    if (!string.IsNullOrWhiteSpace(graphAttr.Color))
                    {
                        if (!ColorUtility.TryParseHtmlString(graphAttr.Color, out color))
                        {
                            color = graphData.GraphColor;
                        }
                    }
                    graphData.GraphColor = color;
                    graphData.GraphName = graphAttr.GraphName;
                }
                GraphCategoryList.Add(graphData);
            }
            //循环查询微图视图
            foreach (var item in graphViewTypes)
            {
                //如果当前类型是微图
                MicroGraphEditorAttribute customAttr = item.GetCustomAttribute<MicroGraphEditorAttribute>();
                if (customAttr != null)
                {
                    if (!customAttr.type.IsSubclassOf(typeof(BaseMicroGraph)) || customAttr.type.IsAbstract)
                    {
                        continue;
                    }
                    GraphCategoryModel graphData = GraphCategoryList.FirstOrDefault(a => a.GraphType == customAttr.type);
                    if (graphData == null)
                        continue;
                    graphData.ViewType = item;
                }
            }
            GraphCategoryList.Sort((a, b) =>
            {
                if (a.GraphType.FullName.GetHashCode() < b.GraphType.FullName.GetHashCode())
                    return -1;
                else if (a.GraphType.FullName.GetHashCode() > b.GraphType.FullName.GetHashCode())
                    return 1;
                return 0;
            });
        }
        /// <summary>
        /// 生成导出节点缓存
        /// </summary>
        private static void s_buildNode()
        {
            TypeCache.TypeCollection nodeViewTypes = TypeCache.GetTypesDerivedFrom<BaseMicroNodeView>();
            TypeCache.TypeCollection nodeTypes = TypeCache.GetTypesDerivedFrom<BaseMicroNode>();

            foreach (Type nodeType in nodeTypes)
            {
                if (nodeType.IsAbstract)
                    continue;

                MicroNodeAttribute nodeAttr = nodeType.GetCustomAttribute<MicroNodeAttribute>();
                NodeCategoryModel nodeCategory = new NodeCategoryModel();
                nodeCategory.NodeClassType = nodeType;
                nodeCategory.ViewClassType = typeof(BaseMicroNodeView);
                nodeCategory.PortDir = PortDirEnum.All;
                nodeCategory.IsHorizontal = true;
                nodeCategory.MinWidth = -1;
                nodeCategory.EnableState = MicroNodeEnableState.Enabled;
                nodeCategory.NodeTitleColor = NodeTitleColorType.Default;
                if (nodeAttr != null)
                {
                    string[] strs = nodeAttr.NodeName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    nodeCategory.NodeLayers = strs;
                    nodeCategory.NodeDescribe = nodeAttr.Describe;
                    nodeCategory.NodeFullName = nodeAttr.NodeName;
                    nodeCategory.MinWidth = nodeAttr.MinWidth;
                    nodeCategory.PortDir = nodeAttr.PortDir;
                    nodeCategory.IsHorizontal = nodeAttr.IsHorizontal;
                    nodeCategory.EnableState = nodeAttr.EnableState;
                    nodeCategory.NodeType = nodeAttr.NodeType;
                    nodeCategory.NodeTitleColor = nodeAttr.NodeTitleColor;
                }
                else
                {
                    nodeCategory.NodeFullName = nodeType.FullName;
                    nodeCategory.NodeLayers = nodeType.FullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                }
                nodeCategory.NodeName = nodeCategory.NodeLayers[nodeCategory.NodeLayers.Length - 1];
                _allNodeCategoryMapping.Add(nodeType, nodeCategory);
            }
            //循环查询节点视图
            foreach (Type viewType in nodeViewTypes)
            {
                //如果当前类型是节点
                MicroGraphEditorAttribute customAttr = viewType.GetCustomAttribute<MicroGraphEditorAttribute>();
                if (customAttr != null)
                {
                    if (!customAttr.type.IsSubclassOf(typeof(BaseMicroNode)) || customAttr.type.IsAbstract)
                    {
                        continue;
                    }
                    if (_allNodeCategoryMapping.TryGetValue(customAttr.type, out NodeCategoryModel nodeCategory))
                    {
                        nodeCategory.ViewClassType = viewType;
                    }
                }
            }
        }
        /// <summary>
        /// 生成变量信息
        /// </summary>
        private static void s_buildVariable()
        {
            TypeCache.TypeCollection varTypes = TypeCache.GetTypesDerivedFrom<BaseMicroVariable>();
            foreach (Type varType in varTypes)
            {
                if (varType.IsAbstract)
                    continue;
                VariableCategoryModel variableCategory = new VariableCategoryModel();
                BaseMicroVariable tempVar = (BaseMicroVariable)Activator.CreateInstance(varType);
                variableCategory.VarName = tempVar.GetDisplayName();
                variableCategory.VarType = tempVar.GetValueType();
                variableCategory.VarBoxType = varType;
                tempVar = null;
                _allVarCategoryMapping.Add(variableCategory.VarType, variableCategory);
            }
            TypeCache.TypeCollection varViewTypes = TypeCache.GetTypesDerivedFrom<IVariableElement>();
            foreach (var item in varViewTypes)
            {
                if (item.IsAbstract || !item.IsClass)
                    continue;
                MicroGraphEditorAttribute editorAttribute = item.GetCustomAttribute<MicroGraphEditorAttribute>();
                if (editorAttribute == null)
                    continue;
                var tempType = editorAttribute.type;
                VariableCategoryModel model = _allVarCategoryMapping.Values.FirstOrDefault(a => a.VarType == tempType || a.VarBoxType == tempType);
                if (model == null)
                {
                    UnityDebug.LogWarning("变量视图没有找到对应的变量包装类");
                    continue;
                }
                if (model.VarViewType == null)
                {
                    model.VarViewType = item;
                    continue;
                }
                if (model.VarViewType.Assembly == CUR_ASSEMBLY)
                {
                    model.VarViewType = tempType;
                }
                else
                {
                    if (tempType.Assembly != CUR_ASSEMBLY)
                    {
                        model.VarViewType = tempType;
                    }
                }
            }
        }
        /// <summary>
        /// 生成图的简介信息缓存
        /// </summary>
        private static void s_buildGraphSummary()
        {
            HashSet<string> hashKey = new HashSet<string>();
            GraphSummaryList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BaseMicroGraph");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseMicroGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(assetPath);
                if (logicGraph == null)
                    continue;
                if (hashKey.Contains(logicGraph.OnlyId))
                {
                    logicGraph.ResetGUID();
                }
                MicroGraphEditorInfo editorInfo = logicGraph.editorInfo;
                GraphSummaryModel graphCache = new GraphSummaryModel();
                graphCache.GraphClassName = logicGraph.GetType().FullName;
                graphCache.AssetPath = assetPath;
                graphCache.OnlyId = logicGraph.OnlyId;
                graphCache.SetEditorInfo(editorInfo);

                hashKey.Add(logicGraph.OnlyId);
                GraphSummaryList.Add(graphCache);
            }
        }
        /// <summary>
        /// 收集键盘按下操作
        /// </summary>
        private static void s_buildGraphKeyEvent()
        {
            TypeCache.TypeCollection keyEventTypes = TypeCache.GetTypesDerivedFrom<IMicroGraphKeyEvent>();
            foreach (Type type in keyEventTypes)
            {
                if (type.IsAbstract)
                    continue;
                if (!type.IsClass)
                    continue;
                _allKeyEvents.Add(type);
            }
        }
        /// <summary>
        /// 收集操作相关
        /// </summary>
        private static void s_buildGraphOperate()
        {
            #region 粘贴复制
            Assembly assembly = typeof(IMicroGraphCopyPaste).Assembly;
            TypeCache.TypeCollection copyPasteTypes = TypeCache.GetTypesDerivedFrom<IMicroGraphCopyPaste>();
            foreach (Type type in copyPasteTypes)
            {
                if (type.Assembly != assembly)
                    continue;
                MicroGraphEditorAttribute attr = type.GetCustomAttribute<MicroGraphEditorAttribute>();
                if (attr == null)
                    continue;
                if (type.IsAbstract)
                    continue;
                if (!type.IsClass)
                    continue;
                if (_copyPasteMapping.ContainsKey(attr.type))
                    _copyPasteMapping[attr.type] = type;
                else
                    _copyPasteMapping.Add(attr.type, type);
            }
            #endregion
        }
        /// <summary>
        /// 收集格式化相关
        /// </summary>
        private static void s_buildGraphFormat()
        {
            TypeCache.MethodCollection methods = TypeCache.GetMethodsWithAttribute<MicroGraphFormatAttribute>();
            foreach (var item in methods)
            {
                var formatAttrs = item.GetCustomAttributes<MicroGraphFormatAttribute>();
                if (formatAttrs == null || formatAttrs.Count() == 0)
                    continue;
                foreach (var attr in formatAttrs)
                {
                    if (attr.GraphType == null)
                        continue;
                    string formatName = string.IsNullOrWhiteSpace(attr.FormatName) ? item.Name : attr.FormatName;
                    string extensionName = attr.Extension;
                    if (!attr.GraphType.IsSubclassOf(typeof(BaseMicroGraph)))
                    {
                        UnityDebug.LogWarning($"方法:{item.Name}的特性无效，特性类型参数需要继承自BaseMicroGraph");
                        continue;
                    }
                    GraphCategoryModel category = GetGraphCategory(attr.GraphType.FullName);
                    if (category == null)
                        continue;
                    category.FormatCategories.Add(new FormatCategoryModel() { Extension = extensionName, FormatName = formatName, Method = item });
                }
            }
        }

        /// <summary>
        /// 收集模板
        /// </summary>
        private static void s_buildGraphTemplate()
        {
            IMicroGraphTemplate template = new MicroNodeTemplateImpl();
            _recordTemplateMapping.Add(typeof(BaseMicroNodeView.InternalNodeView), template);
            _restoreTemplateMapping.Add(typeof(MicroNodeSerializeModel), template);

            template = new MicroVarTemplateImpl();
            _recordTemplateMapping.Add(typeof(MicroVariableEditorInfo), template);
            _restoreTemplateMapping.Add(typeof(MicroVarSerializeModel), template);

            template = new MicroVarNodeTemplateImpl();
            _recordTemplateMapping.Add(typeof(MicroVariableNodeView.InternalNodeView), template);
            _restoreTemplateMapping.Add(typeof(MicroVarNodeSerializeModel), template);

            template = new MicroStickyNodeTemplateImpl();
            _recordTemplateMapping.Add(typeof(MicroStickyNoteView), template);
            _restoreTemplateMapping.Add(typeof(MicroStickySerializeModel), template);

            template = new MicroEdgeTemplateImpl();
            _recordTemplateMapping.Add(typeof(MicroEdgeView), template);
            _restoreTemplateMapping.Add(typeof(MicroEdgeSerializeModel), template);

            template = new MicroGroupTemplateImpl();
            _recordTemplateMapping.Add(typeof(MicroGroupView), template);
            _restoreTemplateMapping.Add(typeof(MicroGroupSerializeModel), template);

        }
    }
}
