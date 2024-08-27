using System.Collections.Generic;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图的配置文件
    /// </summary>
    public interface IMicroGraphConfig
    {

        List<IMicroGraphFormat> GetMicroGraphFormats();
    }
}
