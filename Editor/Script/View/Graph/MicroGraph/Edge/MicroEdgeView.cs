using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    internal class MicroEdgeView : Edge
    {
        private const string STYLE_PATH = "Uss/MicroGraph/MicroEdgeView";

        public MicroEdgeView() : base()
        {
            this.AddStyleSheet(STYLE_PATH);
        }
    }
}
