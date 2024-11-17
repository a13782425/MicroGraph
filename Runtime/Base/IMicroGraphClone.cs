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
