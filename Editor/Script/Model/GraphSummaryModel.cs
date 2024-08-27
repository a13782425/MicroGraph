using MicroGraph.Runtime;
using System;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 单个微图对象的简介信息
    /// </summary>
    public class GraphSummaryModel
    {
        public string OnlyId;
        /// <summary>
        /// 微图名
        /// </summary>
        public string MicroName;
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe;
        /// <summary>
        /// 图类型名全称,含命名空间
        /// </summary>
        public string GraphClassName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime;

        /// <summary>
        /// 是否已经刷新过
        /// </summary>
        internal bool IsRefresh;
        internal void SetEditorInfo(MicroGraphEditorInfo editorInfo)
        {
            this.MicroName = editorInfo.Title;
            this.CreateTime = editorInfo.CreateTime;
            this.ModifyTime = editorInfo.ModifyTime;
            this.Describe = string.IsNullOrWhiteSpace(editorInfo.Describe) ? "这里是描述" : editorInfo.Describe;
        }

    }
}
