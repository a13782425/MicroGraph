using System;
using System.Collections.Generic;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图操作类
    /// </summary>
    internal static class MicroGraphOperate
    {
        /// <summary>
        /// Group视图优先级
        /// </summary>
        public const int GROUP_VIEW_RECORD_PRIORITY = 1;
        /// <summary>
        /// edge视图优先级
        /// </summary>
        public const int EDGE_VIEW_RECORD_PRIORITY = 2;
        /// <summary>
        /// node视图优先级
        /// </summary>
        public const int NODE_VIEW_RECORD_PRIORITY = 3;
        /// <summary>
        /// VariableItemView视图优先级
        /// </summary>
        public const int VAR_VIEW_RECORD_PRIORITY = 4;


        private static Dictionary<Type, MicroCopyPasteOperateData> copyDatas = new Dictionary<Type, MicroCopyPasteOperateData>();

        /// <summary>
        /// 拷贝数据
        /// key：微图类型 value：当前拷贝的数据
        /// </summary>
        internal static Dictionary<Type, MicroCopyPasteOperateData> CopyDatas => copyDatas;

    }

    /// <summary>
    /// 粘贴复制操作
    /// </summary>
    public interface IMicroGraphCopyPaste
    {
        void Copy(BaseMicroGraphView graphView, object target);
        bool Paste(MicroCopyPasteOperateData copyData);
    }

    /// <summary>
    /// 粘贴复制操作
    /// </summary>
    internal interface IMicroGraphTemplate
    {
        void Record(MicroGraphTemplateModel model, object target);
        void Restore(MicroTemplateOperateData operateData, object data);
    }

    /// <summary>
    /// 记录
    /// 用做撤销和重做
    /// </summary>
    public interface IMicroGraphRecordCommand
    {
        /// <summary>
        /// 撤销的优先级
        /// 优先级越大越先执行
        /// </summary>
        int Priority => 0;
        void Record(BaseMicroGraphView graphView, object target);
        bool Undo(BaseMicroGraphView graphView);
        bool Redo(BaseMicroGraphView graphView);
    }

}
