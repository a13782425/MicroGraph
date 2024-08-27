using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MicroGraph.MonoExample.Runtime
{
    [MicroNode("移动")]
    public partial class TransformMoveNode : BaseMonoNode
    {
        [NodeInput]
        [NodeFieldName("Tran")]
        public Transform tran;
        [NodeInput]
        [NodeFieldName("方向")]
        public Vector3 dir;
        [NodeInput]
        [NodeFieldName("速度")]
        public float speed;

        public override bool OnExecute()
        {
            if (tran != null)
                tran.position += dir * speed * Time.deltaTime * Time.timeScale;
            return base.OnExecute();
        }
    }
}
