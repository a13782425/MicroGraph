using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MicroGraph.Editor
{
    internal sealed class MicroTemplateOperateData
    {
        public Vector2 mousePos = Vector2.zero;
        public Vector2 centerPos = Vector2.zero;
        /// <summary>
        /// 旧Id映射新Id
        /// </summary>
        public Dictionary<int, int> oldMappingNewIdDic = new Dictionary<int, int>();

        public BaseMicroGraphView view = default;

        public MicroGraphTemplateModel model = default;
    }
}
