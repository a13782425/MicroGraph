using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 图的种类信息
    /// </summary>
    public class GraphCategoryModel
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; internal set; }
        /// <summary>
        /// 图名
        /// </summary>
        public string GraphName { get; internal set; }
        /// <summary>
        /// 图的颜色
        /// </summary>
        public Color GraphColor { get; internal set; }
        /// <summary>
        /// 微图类型
        /// </summary>
        public Type GraphType { get; internal set; }
        /// <summary>
        /// 视图类型
        /// </summary>
        public Type ViewType { get; internal set; }
        /// <summary>
        /// 是否初始化过了
        /// </summary>
        public bool IsInit { get; internal set; }

        private List<NodeCategoryModel> _nodes = new List<NodeCategoryModel>();
        /// <summary>
        /// 当前微图所对应的节点
        /// </summary>
        public List<NodeCategoryModel> NodeCategories => _nodes;

        private List<NodeCategoryModel> _uniqueNodes = new List<NodeCategoryModel>();
        /// <summary>
        /// 当前微图唯一节点
        /// </summary>
        public List<NodeCategoryModel> UniqueNodeCategories => _uniqueNodes;

        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <returns></returns>
        public NodeCategoryModel GetNodeCategory(Type nodeType) => GetNodeCategory(nodeType.FullName);
        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="nodeTypeName">节点类全名</param>
        /// <returns></returns>
        public NodeCategoryModel GetNodeCategory(string nodeTypeName) => NodeCategories.FirstOrDefault(a => a.NodeClassType.FullName == nodeTypeName);

        /// <summary>
        /// 是否是唯一节点
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <returns></returns>
        public bool IsUniqueNode(Type nodeType) => IsUniqueNode(nodeType.FullName);
        /// <summary>
        /// 是否是唯一节点
        /// </summary>
        /// <param name="nodeTypeName">节点类全名</param>
        /// <returns></returns>
        public bool IsUniqueNode(string nodeTypeName) => UniqueNodeCategories.FirstOrDefault(a => a.NodeClassType.FullName == nodeTypeName) != null;


        private List<VariableCategoryModel> _variables = new List<VariableCategoryModel>();
        /// <summary>
        /// 适用于当前微图的变量
        /// </summary>
        public List<VariableCategoryModel> VariableCategories => _variables;

        private List<FormatCategoryModel> _formats = new List<FormatCategoryModel>();
        /// <summary>
        /// 当前微图适用的格式化
        /// </summary>
        public List<FormatCategoryModel> FormatCategories => _formats;

        /// <summary>
        /// 通过变量类型获取它的信息
        /// </summary>
        /// <param name="varType"></param>
        /// <returns></returns>
        public VariableCategoryModel GetVarCategory(Type varType) => _variables.FirstOrDefault(a => a.VarType == varType || a.VarBoxType == varType);


    }
    /// <summary>
    /// 图节点分类信息缓存
    /// </summary>
    public sealed class NodeCategoryModel
    {
        /// <summary>
        /// 节点名
        /// </summary>
        public string NodeName { get; internal set; }
        /// <summary>
        /// 节点层级
        /// </summary>
        public string[] NodeLayers { get; internal set; }
        /// <summary>
        /// 节点全名
        /// </summary>
        public string NodeFullName { get; internal set; }
        /// <summary>
        /// 节点描述
        /// </summary>
        public string NodeDescribe { get; internal set; }
        /// <summary>
        /// 节点类型
        /// </summary>
        public Type NodeClassType { get; internal set; }
        /// <summary>
        /// 视图类型
        /// </summary>
        public Type ViewClassType { get; internal set; }
        /// <summary>
        /// 节点端口类型
        /// </summary>
        public PortDirEnum PortDir { get; internal set; }
        /// <summary>
        /// 节点端口显示方式
        /// </summary>
        public bool IsHorizontal { get; internal set; }
        /// <summary>
        /// 最小宽度
        /// </summary>
        public int MinWidth { get; internal set; }
        /// <summary>
        /// 节点启用状态
        /// </summary>
        public MicroNodeEnableState EnableState { get; internal set; }
        /// <summary>
        /// 标题颜色
        /// </summary>
        public NodeTitleColorType NodeTitleColor { get; internal set; }
        /// <summary>
        /// 节点类型
        /// </summary>
        public MicroNodeType NodeType { get; internal set; }



        private bool _isReady = false;
        /// <summary>
        /// 字段信息
        /// key:字段名 
        /// </summary>
        private Dictionary<string, FieldInfo> _fieldInfos = new Dictionary<string, FieldInfo>();
        /// <summary>
        /// 获取所有字段信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, FieldInfo> GetNodeFieldInfos()
        {
            if (_isReady)
            {
                return _fieldInfos;
            }
            _isReady = true;
            FieldInfo[] fields = NodeClassType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var item in fields)
            {
                if (item.IsPublic)
                {
                    if (item.GetCustomAttribute<NonSerializedAttribute>() != null)
                        continue;
                }
                else
                {
                    if (item.GetCustomAttribute<SerializeField>() == null && item.GetCustomAttribute<SerializeReference>() == null)
                        continue;
                }
                _fieldInfos.Add(item.Name, item);
            }
            return _fieldInfos;
        }

        public FieldInfo GetNodeFieldInfo(string fieldName)
        {
            if (GetNodeFieldInfos().TryGetValue(fieldName, out FieldInfo fieldInfo))
                return fieldInfo;
            return NodeClassType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }

    /// <summary>
    /// 变量缓存
    /// </summary>
    public sealed class VariableCategoryModel
    {
        /// <summary>
        /// 变量名
        /// </summary>
        public string VarName { get; internal set; }
        /// <summary>
        /// 变量真实类型
        /// </summary>
        public Type VarType { get; internal set; }
        /// <summary>
        /// 变量包装类型
        /// </summary>
        public Type VarBoxType { get; internal set; }
        /// <summary>
        /// 变量视图类型
        /// </summary>
        public Type VarViewType { get; internal set; }
    }

    /// <summary>
    /// 图格式化缓存
    /// </summary>
    public sealed class FormatCategoryModel
    {
        /// <summary>
        /// 格式化方法
        /// </summary>
        public MethodInfo Method { get; internal set; }
        /// <summary>
        /// 格式化名
        /// </summary>
        public string FormatName { get; internal set; }
        /// <summary>
        /// 格式化后缀
        /// </summary>
        public string Extension { get; internal set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="graph">微图</param>
        /// <param name="path">路径</param>
        /// <returns>是否导出成功</returns>
        public bool ToFormat(BaseMicroGraph graph, string path)
        {
            if (Method == null)
                return false;
            object res = Method.Invoke(null, new object[] { graph, path });
            return (bool)res;
        }
    }
}
