using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图拓展
    /// </summary>
    public static class MicroGraphExtensions
    {
        /// <summary>
        /// 逻辑图唯一ID的字段
        /// </summary>
        readonly static FieldInfo GRAPH_ONLY_ID = typeof(BaseMicroGraph).GetField("_onlyId", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static Type MICRO_NODE_TYPE = typeof(BaseMicroNode);
        private readonly static Type MICRO_VARIABLE_TYPE = typeof(BaseMicroVariable);
        private readonly static Type LIST_TYPE = typeof(List<>);
        /// <summary>
        /// 重置唯一ID
        /// </summary>
        /// <param name="graph"></param>
        internal static void ResetGUID(this BaseMicroGraph graph)
        {
            if (GRAPH_ONLY_ID != null)
            {
                GRAPH_ONLY_ID.SetValue(graph, Guid.NewGuid().ToString());
            }
            else
            {
                Debug.LogError("没有找到字段");
            }
        }

        /// <summary>
        /// 根据窗体获取位置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="localPos">本地位置</param>
        internal static Vector2 GetScreenPosition(this UnityEditor.EditorWindow window, Vector2 localPos)
        {
            return window.position.position + localPos;
        }

        /// <summary>
        /// 获取字段显示名
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static string GetFieldDisplayName(this FieldInfo fieldInfo)
        {
            NodeFieldNameAttribute attr = fieldInfo.GetCustomAttribute<NodeFieldNameAttribute>();
            return attr == null ? fieldInfo.Name : attr.name;
        }
        /// <summary>
        /// 当前字段是否是输入
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool FieldIsInput(this FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<NodeInputAttribute>() != null;
        }
        /// <summary>
        /// 当前字段是否是输出
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool FieldIsOutput(this FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<NodeOutputAttribute>() != null;
        }
        /// <summary>
        /// 当前字段是否是输入
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static PortDirEnum FieldPortDir(this FieldInfo fieldInfo)
        {
            bool isInput = fieldInfo.FieldIsInput();
            bool isOutput = fieldInfo.FieldIsOutput();
            PortDirEnum dir = PortDirEnum.None;
            dir = dir | (isInput ? PortDirEnum.In : PortDirEnum.None);
            dir = dir | (isOutput ? PortDirEnum.Out : PortDirEnum.None);
            return dir;
        }
        /// <summary>
        /// 获取一个类型的端口类型
        /// </summary>
        /// <param name="orginType">原始类型，可以是List</param>  
        /// <returns>
        /// 返回一个包含三个元素的元组：
        /// <para>type: 当前字段的类型，去掉List</para>
        /// <para>isList: 当前字段是否是List</para>
        /// <para>portType: 当前端口类型</para>
        /// </returns>
        internal static (Type type, MicroPortType portType) GetTypePortInfo(this Type orginType)
        {
            Type type;
            MicroPortType result;
            if (orginType.IsGenericType || orginType.IsArray)
            {
                ////这里判断下是List<T> 还是Dic<K,V>
                //if (orginType.GetGenericTypeDefinition() == LIST_TYPE)
                //{
                //    isList = true;
                //    type = orginType.GenericTypeArguments[0];
                //    //是集合
                //    if (type.IsSubclassOf(MICRO_NODE_TYPE))
                //    {
                //        //List<BaseMicroNode>
                //        result = MicroPortType.ListRefPort;
                //    }
                //    else if (type.IsSubclassOf(MICRO_VARIABLE_TYPE))
                //    {
                //        //List<BaseMicroVariable>
                //        result = MicroPortType.ListVarRefPort;
                //    }
                //    else
                //    {
                //        //其他所有
                //        result = MicroPortType.ListVarPort;
                //    }
                //}
                //else
                //{
                throw new Exception("默认不支持泛型和数组，请自行拓展");
                //}
            }
            else
            {
                type = orginType;
                //不是集合
                if (type.IsSubclassOf(MICRO_NODE_TYPE))
                {
                    //BaseMicroNode
                    //result = MicroPortType.RefPort;
                    throw new Exception("默认不支持BaseMicroNode类型，请自行拓展");
                }
                else if (type.IsSubclassOf(MICRO_VARIABLE_TYPE))
                {
                    //BaseMicroVariable
                    result = MicroPortType.VarPort;
                }
                else
                {
                    //其他所有
                    result = MicroPortType.VarPort;
                }
            }
            return (type, result);
        }

        ///// <summary>
        ///// 获取一个字段的端口类型
        ///// </summary>
        ///// <param name="fieldInfo"></param>  
        ///// <returns>
        ///// 返回一个包含三个元素的元组：
        ///// <para>type: 当前字段的类型，去掉List</para>
        ///// <para>isList: 当前字段是否是List</para>
        ///// <para>portType: 当前端口类型</para>
        ///// </returns>
        //public static (Type type, bool isList, MicroPortType portType) GetFieldInfoPortInfo(this FieldInfo fieldInfo)
        //{
        //    bool isList;
        //    Type type;
        //    MicroPortType result;
        //    if (fieldInfo.FieldType.IsGenericType)
        //    {
        //        //这里判断下是List<T> 还是Dic<K,V>
        //        if (fieldInfo.FieldType.GetGenericTypeDefinition() == LIST_TYPE)
        //        {
        //            isList = true;
        //            type = fieldInfo.FieldType.GenericTypeArguments[0];
        //            //是集合
        //            if (type.IsSubclassOf(MICRO_NODE_TYPE))
        //            {
        //                //List<BaseMicroNode>
        //                result = MicroPortType.ListRefPort;
        //            }
        //            else if (type.IsSubclassOf(MICRO_VARIABLE_TYPE))
        //            {
        //                //List<BaseMicroVariable>
        //                result = MicroPortType.ListVarRefPort;
        //            }
        //            else
        //            {
        //                //其他所有
        //                result = MicroPortType.ListVarPort;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("目前只支持List<T>");
        //        }
        //    }
        //    else
        //    {
        //        isList = false;
        //        type = fieldInfo.FieldType;
        //        //不是集合
        //        if (type.IsSubclassOf(MICRO_NODE_TYPE))
        //        {
        //            //BaseMicroNode
        //            result = MicroPortType.RefPort;
        //        }
        //        else if (type.IsSubclassOf(MICRO_VARIABLE_TYPE))
        //        {
        //            //BaseMicroVariable
        //            result = MicroPortType.VarRefPort;
        //        }
        //        else
        //        {
        //            //其他所有
        //            result = MicroPortType.VarPort;
        //        }
        //    }
        //    return (type, isList, result);
        //}
    }
}
