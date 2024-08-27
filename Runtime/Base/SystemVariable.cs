using System;
using UnityEngine;

namespace MicroGraph.Runtime
{
    [Serializable]
    public class MicroIntVariable : BaseMicroVariable<int> { }

    [Serializable]
    public class MicroStringVariable : BaseMicroVariable<string> { }

    [Serializable]
    public class MicroFloatVariable : BaseMicroVariable<float>
    {
        public override string GetDisplayName() => "Float";
    }

    [Serializable]
    public class MicroBoolVariable : BaseMicroVariable<bool> { }

    [Serializable]
    public class MicroColorVariable : BaseMicroVariable<Color> { }

    [Serializable]
    public class MicroVector2Variable : BaseMicroVariable<Vector2> { }

    [Serializable]
    public class MicroVector3Variable : BaseMicroVariable<Vector3> { }

}
