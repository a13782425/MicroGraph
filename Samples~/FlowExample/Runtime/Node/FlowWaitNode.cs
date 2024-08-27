using MicroGraph.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.FlowExample.Runtime
{
    [MicroNode("等待", NodeType = MicroNodeType.Wait)]
    public partial class FlowWaitNode : BaseFlowNode
    {
        [NodeInput]
        [NodeFieldName("等待时间")]
        public float waitTime;
        [NodeInput]
        [NodeFieldName("时间缩放影响")]
        public bool useUnscaled;

        private float _tempTime = 0;
        public override bool OnExecute()
        {
            _tempTime = 0;
            return true;
        }
        public override bool OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            _tempTime += useUnscaled ? unscaledDeltaTime : deltaTime;
            if (_tempTime > waitTime)
            {
                this.State = NodeState.Success;
            }
            return base.OnUpdate(deltaTime, unscaledDeltaTime);
        }
    }
}