using MicroGraph.MonoExample.Runtime;
using MicroGraph.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.MonoExample.Runtime
{
    [MicroNode("打印日志")]
    public partial class FlowDebugNode : BaseMonoNode
    {
        [NodeFieldName("内容")]
        [NodeInput]
        public string msg;
        [NodeFieldName("日志类型")]
        public LogType logType = LogType.Log;
        public override bool OnExecute()
        {
            string str = msg ?? "";
            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(str);
                    break;
                case LogType.Assert:
                    Debug.LogError(str);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(str);
                    break;
                case LogType.Log:
                    Debug.Log(str);
                    break;
                case LogType.Exception:
                    Debug.LogError(str);
                    break;
                default:
                    break;
            }
            return base.OnExecute();
        }
    }

}