using MicroGraph.Runtime;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图格式化
    /// </summary>
    public interface IMicroGraphFormat
    {
        /// <summary>
        /// 后缀名
        /// txt，json等等
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="graph">逻辑图</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        bool ToFormat(BaseMicroGraph graph, string path);
    }
}
