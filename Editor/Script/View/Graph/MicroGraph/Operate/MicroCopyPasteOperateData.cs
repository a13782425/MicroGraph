using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 复制的数据
    /// </summary>
    public class MicroCopyPasteOperateData
    {
        public Vector2 mousePos = Vector2.zero;
        public Vector2 centerPos = Vector2.zero;

        public BaseMicroGraphView view = default;

        /// <summary>
        /// 旧Id映射新Id
        /// </summary>
        public Dictionary<int, int> oldMappingNewIdDic = new Dictionary<int, int>();

        public List<IMicroGraphCopyPaste> edges = new List<IMicroGraphCopyPaste>();
        public List<IMicroGraphCopyPaste> elements = new List<IMicroGraphCopyPaste>();
        public List<IMicroGraphCopyPaste> groups = new List<IMicroGraphCopyPaste>();
        public List<IMicroGraphCopyPaste> variables = new List<IMicroGraphCopyPaste>();
    }
}
