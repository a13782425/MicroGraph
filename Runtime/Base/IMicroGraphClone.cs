using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 克隆
    /// </summary>
    public interface IMicroGraphClone
    {
        IMicroGraphClone DeepCopy(IMicroGraphClone target);
        IMicroGraphClone DeepClone();
    }
}
