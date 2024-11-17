using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroGraph.Runtime
{
    [MicroNode("微图/包节点", NodeType = MicroNodeType.Wait, EnableState = MicroNodeEnableState.Exclude)]
    [Serializable]
    public sealed partial class MicroPackageNode : BaseMicroNode
    {
        public int PackageId;
        [NodeInput]
        [NodeFieldName("显示日志")]
        public bool showLog = true;
        private MicroPackageInfo _packageInfo;
        private Dictionary<int, RuntimeAtom> atoms = new Dictionary<int, RuntimeAtom>();
        private Queue<RuntimeAtom> _cacheQueue = new Queue<RuntimeAtom>();
        private RuntimeAtom _runtimeAtom;
        private int _startNodeId;
        private int _endNodeId;
        public override bool OnExecute()
        {
            if (atoms.ContainsKey(RuntimVersion))
                return true;
            if (_cacheQueue.Count > 0)
            {
                RuntimeAtom temp = _cacheQueue.Dequeue();
                atoms.Add(RuntimVersion, temp);
                temp.Play();
                return true;
            }
            if (_packageInfo == null)
            {
                _packageInfo = microGraph.Packages.FirstOrDefault(a => a.PackageId == PackageId);
                if (_packageInfo == null)
                {
                    MicroGraphLogger.LogWarning($"节点包:{PackageId},没有找到");
                    return base.OnExecute();
                }
                if (_packageInfo.StartNodes.Count == 0)
                {
                    MicroGraphLogger.LogWarning($"节点包:{PackageId},没有进入节点");
                    return base.OnExecute();
                }
                if (_packageInfo.EndNodes.Count == 0)
                {
                    if (showLog)
                        MicroGraphLogger.LogWarning($"节点包:{PackageId},没有退出节点");
                    _endNodeId = -1;
                }
                else
                {
                    _endNodeId = _packageInfo.EndNodes[0];
                }
                _startNodeId = _packageInfo.StartNodes[0];

            }

            var atom = new RuntimeAtom(microGraph, _startNodeId);
            atom.EndNodeId = _endNodeId;
            atoms.Add(RuntimVersion, atom);
            atom.Play();
            return true;
        }

        public override bool OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (atoms.TryGetValue(RuntimVersion, out _runtimeAtom))
            {
                _runtimeAtom.Update(deltaTime, unscaledDeltaTime);
                if (_runtimeAtom.RuntimeState == MicroGraphRuntimeState.Complete)
                {
                    State = NodeState.Success;
                }
                else
                {
                    if (State != NodeState.Running)
                        State = NodeState.Running;
                }
            }
            return true;
        }
        public override bool OnExit()
        {
            _runtimeAtom?.Exit();
            return base.OnExit();
        }
        public override List<int> GetChild()
        {
            if (atoms.TryGetValue(RuntimVersion, out _runtimeAtom))
            {
                _cacheQueue.Enqueue(_runtimeAtom);
                atoms.Remove(RuntimVersion);

                if (_runtimeAtom.EndMode == MicroGraphRuntimeEndMode.Auto)
                {
#if UNITY_EDITOR
                    if (showLog)
                    {
                        var groupData = microGraph.editorInfo.Groups.FirstOrDefault(a => a.GroupId == PackageId);
                        if (groupData != null)
                            MicroGraphLogger.LogWarning($"微图:{microGraph.name}中包没有执行到结束节点，包Id: {PackageId}, 包名: {groupData.Title}");
                        else
                            MicroGraphLogger.LogWarning($"微图:{microGraph.name}中包没有执行到结束节点，包Id: {PackageId}, 包名: null");
                    }
#endif
                    return EMPTY_CHILD_LIST;
                }
                return base.GetChild();
            }
            return base.GetChild();
        }
    }
}
