using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    [Serializable]
    internal class MicroVariableEditorInfo
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public bool CanRename = true;
        [SerializeField]
        public bool CanDelete = true;
        /// <summary>
        /// 可以有默认值
        /// </summary>
        [SerializeField]
        public bool CanDefaultValue = true;
        /// <summary>
        /// 可以被赋值
        /// </summary>
        [SerializeField]
        public bool CanAssign = true;
        /// <summary>
        /// 注释
        /// </summary>
        [SerializeField]
        public string Comment = "";

        [NonSerialized]
        [HideInInspector]
        public BaseMicroVariable Target;
    }
}
