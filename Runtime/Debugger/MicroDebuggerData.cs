#if MICRO_GRAPH_DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroGraph.Runtime
{
    internal static class MicroDebuggerHelper
    {
        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        internal static void WriteString(this BinaryWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(0);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }
        /// <summary>
        /// 读字符串
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static string ReadStr(this BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length == 0)
            {
                return null;
            }
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        internal static byte[] GetNetBytes(this INetData data)
        {
            NetMessagePackage package = NetMessagePackage.CreateSendMessagePackage(data);
            return package.GetMessageData();
        }
    }


    /// <summary>
    /// 变量网络数据
    /// </summary>
    [Serializable]
    public class DebuggerVarData
    {
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId = "";
        /// <summary>
        /// 运行时名称
        /// </summary>
        public string runtimeName = "";
        /// <summary>
        /// 变量名
        /// </summary>
        public string varName = "";
        /// <summary>
        /// 变量数据
        /// 暂时只为简单的字符串
        /// </summary>
        public string data = "";

        public void Format(BinaryWriter binaryReader)
        {
            binaryReader.WriteString(varName);
            binaryReader.WriteString(data);
        }
        public void Parse(BinaryReader binaryReader)
        {
            varName = binaryReader.ReadStr();
            data = binaryReader.ReadStr();
        }
        private static Queue<DebuggerVarData> _queue = new Queue<DebuggerVarData>();
        public static DebuggerVarData Create()
        {
            if (_queue.Count > 0)
            {
                return _queue.Dequeue();
            }
            return new DebuggerVarData();
        }
        public static void Release(DebuggerVarData data)
        {
            _queue.Enqueue(data);
        }
    }
    /// <summary>
    /// 节点网络数据
    /// </summary>
    [Serializable]
    public class DebuggerNodeData
    {
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId = "";
        /// <summary>
        /// 运行时名称
        /// </summary>
        public string runtimeName = "";
        /// <summary>
        /// 节点Id
        /// </summary>
        public int nodeId = 0;
        /// <summary>
        /// 节点状态
        /// </summary>
        public byte nodeState = 0;

        public void Parse(BinaryReader binaryReader)
        {
            nodeId = binaryReader.ReadInt32();
            nodeState = binaryReader.ReadByte();
        }
        public void Format(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(nodeId);
            binaryWriter.Write(nodeState);
        }
        private static Queue<DebuggerNodeData> _queue = new Queue<DebuggerNodeData>();
        public static DebuggerNodeData Create()
        {
            if (_queue.Count > 0)
            {
                return _queue.Dequeue();
            }
            return new DebuggerNodeData();
        }
        public static void Release(DebuggerNodeData data)
        {
            _queue.Enqueue(data);
        }
    }

    [Serializable]
    public class DebuggerGraphData : INetData
    {
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId;
        /// <summary>
        /// 运行时名称
        /// </summary>
        public string runtimeName;
        /// <summary>
        /// 是否是开始
        /// </summary>
        public bool start = false;
        public MessageType Type => MessageType.GraphData;
        public List<DebuggerNodeData> nodeDatas = new List<DebuggerNodeData>();
        public List<DebuggerVarData> varDatas = new List<DebuggerVarData>();

        public byte[] Format()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.WriteString(microGraphId);
            writer.WriteString(runtimeName);
            writer.Write(start);
            writer.Write(nodeDatas.Count);
            foreach (var nodeData in nodeDatas)
            {
                nodeData.Format(writer);
            }
            writer.Write(varDatas.Count);
            foreach (var varData in varDatas)
            {
                varData.Format(writer);
            }
            return stream.ToArray();
        }

        public void Parse(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            microGraphId = binaryReader.ReadStr();
            runtimeName = binaryReader.ReadStr();
            start = binaryReader.ReadBoolean();
            int nodeCount = binaryReader.ReadInt32();
            for (int i = 0; i < nodeCount; i++)
            {
                DebuggerNodeData nodeData = new DebuggerNodeData();
                nodeData.Parse(binaryReader);
                nodeData.microGraphId = microGraphId;
                nodeData.runtimeName = runtimeName;
                nodeDatas.Add(nodeData);
            }
            int varCount = binaryReader.ReadInt32();
            for (int i = 0; i < varCount; i++)
            {
                DebuggerVarData varData = new DebuggerVarData();
                varData.Parse(binaryReader);
                varData.microGraphId = microGraphId;
                varData.runtimeName = runtimeName;
                varDatas.Add(varData);
            }
        }

        public void AddNodeData(DebuggerNodeData nodeData)
        {
            //if (!start)
            //    return;
            DebuggerNodeData node = nodeDatas.FirstOrDefault(a => a.nodeId == nodeData.nodeId);
            if (node == null)
            {
                node = DebuggerNodeData.Create();
                nodeDatas.Add(node);
            }
            node.nodeState = nodeData.nodeState;
            node.nodeId = nodeData.nodeId;

        }
        public void AddVarData(DebuggerVarData varData)
        {
            //if (!start)
            //    return;
            DebuggerVarData node = varDatas.FirstOrDefault(a => a.varName == varData.varName);
            if (node == null)
            {
                node = DebuggerVarData.Create();
                varDatas.Add(node);
            }
            node.varName = varData.varName;
            node.data = varData.data;
        }
        private static Queue<DebuggerGraphData> _queue = new Queue<DebuggerGraphData>();
        public static DebuggerGraphData Create()
        {
            if (_queue.Count > 0)
            {
                return _queue.Dequeue();
            }
            return new DebuggerGraphData();
        }

        public static void Release(DebuggerGraphData data)
        {
            foreach (var nodeData in data.nodeDatas)
            {
                DebuggerNodeData.Release(nodeData);
            }
            foreach (var varData in data.varDatas)
            {
                DebuggerVarData.Release(varData);
            }
            data.nodeDatas.Clear();
            data.varDatas.Clear();
            _queue.Enqueue(data);
        }

    }
    [Serializable]
    public class DebuggerGraphRenameData : INetData
    {
        public MessageType Type => MessageType.GraphRename;
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId = "";
        /// <summary>
        /// 运行时老名称
        /// </summary>
        public string oldName = "";
        /// <summary>
        /// 运行时新名称
        /// </summary>
        public string newName = "";
        public byte[] Format()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.WriteString(microGraphId);
            writer.WriteString(oldName);
            writer.WriteString(newName);
            return stream.ToArray();
        }

        public void Parse(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            microGraphId = binaryReader.ReadStr();
            oldName = binaryReader.ReadStr();
            newName = binaryReader.ReadStr();
        }
    }
    [Serializable]
    public class DebuggerGraphDeleteData : INetData
    {
        public MessageType Type => MessageType.GraphDelete;
        /// <summary>
        /// 微图Id
        /// </summary>
        public string microGraphId = "";
        /// <summary>
        /// 运行时老名称
        /// </summary>
        public string runtimeName = "";
        public byte[] Format()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.WriteString(microGraphId);
            writer.WriteString(runtimeName);
            return stream.ToArray();
        }

        public void Parse(byte[] bytes)
        {
            using MemoryStream memoryStream = new MemoryStream(bytes);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            microGraphId = binaryReader.ReadStr();
            runtimeName = binaryReader.ReadStr();
        }
    }
}

#endif