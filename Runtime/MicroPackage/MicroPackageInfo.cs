using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图包
    /// </summary>
    [Serializable]
    public sealed partial class MicroPackageInfo : IMicroGraphClone
    {
        /// <summary>
        /// 微图包ID
        /// </summary>
        [SerializeField]
        public int PackageId;
        /// <summary>
        /// 进入节点列表
        /// </summary>
        [SerializeField]
        public List<int> StartNodes = new List<int>();
        /// <summary>
        /// 退出节点列表
        /// </summary>
        [SerializeField]
        public List<int> EndNodes = new List<int>();

    }
}
