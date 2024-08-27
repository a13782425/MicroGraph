using System;

namespace MicroGraph.Editor
{
    [Flags]
    public enum MicroPortType
    {
        /// <summary>
        /// 占位符
        /// 如果端口的类型是它会报错
        /// </summary>
        None = 0,
        /// <summary>
        /// 节点NodeView默认端口(In,out)
        /// </summary>
        BaseNodePort = 1,
        /// <summary>
        /// 变量NodeView节点默认端口(In,out)
        /// </summary>
        BaseVarPort = 1 << 1,

        /// <summary>
        /// BaseMicroNode引用端口
        /// 该端口只能连接BasePort
        /// 只能是输出
        /// </summary>
        NodePort = 1 << 2,
        ///// <summary>
        ///// List<BaseMicroNode>引用端口
        ///// 该端口只能连接BasePort
        ///// 只能是输出
        ///// </summary>
        //ListRefPort = 1 << 3,

        /// <summary>
        /// 变量端口
        /// 非BaseMicroNode和BaseMicroVariable，只是普通的Var
        /// </summary>
        VarPort = 1 << 4,
        ///// <summary>
        ///// BaseMicroVariable引用端口
        ///// </summary>
        //VarRefPort = 1 << 5,
        ///// <summary>
        ///// List<BaseMicroVariable>引用端口
        ///// </summary>
        //ListVarRefPort = 1 << 6,
        ///// <summary>
        ///// List<非BaseMicroNode和BaseMicroVariable>引用端口，只是普通的Var
        ///// </summary>
        //ListVarPort = 1 << 7,

    }
}
