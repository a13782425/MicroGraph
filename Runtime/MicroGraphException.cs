using System;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 微图通用异常
    /// </summary>
    public class MicroGraphException : Exception
    {
        public MicroGraphException() : base() { }
        public MicroGraphException(string message) : base(message) { }
    }
    /// <summary>
    /// 微图空错误异常
    /// </summary>
    public class MicroGraphNullException : MicroGraphException
    {
        public MicroGraphNullException() : base("Object is null.") { }
        public MicroGraphNullException(string message) : base(message) { }
    }

}
