using System;
using System.Diagnostics;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图编辑器特性
    /// 用于指向类型关联
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MicroGraphEditorAttribute : Attribute
    {
        public Type type { get; private set; }

        public MicroGraphEditorAttribute(Type type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// 微图排序特性
    /// 用于排序微图
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MicroGraphOrderAttribute : Attribute
    {
        public int Order { get; private set; }

        public MicroGraphOrderAttribute(int order)
        {
            this.Order = order;
        }
    }

    /// <summary>
    /// 标记微图格式化方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class MicroGraphFormatAttribute : Attribute
    {
        /// <summary>
        /// 微图类型
        /// </summary>
        public readonly Type GraphType;
        /// <summary>
        /// 格式化名
        /// </summary>
        public readonly string FormatName;
        /// <summary>
        /// 格式化后缀
        /// </summary>
        public readonly string Extension;

        public MicroGraphFormatAttribute(Type graphType, string extension) : this(graphType, "", extension) { }
        public MicroGraphFormatAttribute(Type graphType, string formatName, string extension)
        {
            this.GraphType = graphType;
            this.FormatName = formatName;
            this.Extension = extension;
        }
    }
}
