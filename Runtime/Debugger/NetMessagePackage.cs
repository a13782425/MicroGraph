#if MICRO_GRAPH_DEBUG
using System;
using System.Collections.Generic;

namespace MicroGraph.Runtime
{
    /// <summary>
    /// 网络消息类型
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// 缺省
        /// </summary>
        None = 0,
        /// <summary>
        /// 微图数据
        /// </summary>
        GraphData = 2,
        /// <summary>
        /// 蓝图改运行时名字
        /// </summary>
        GraphRename = 10,
        /// <summary>
        /// 蓝图删除
        /// </summary>
        GraphDelete = 11,
        /// <summary>
        /// 心跳
        /// </summary>
        Heartbeat = 100
    }
    /// <summary>
    /// 网络数据
    /// </summary>
    public interface INetData
    {
        MessageType Type { get; }

        byte[] Format();
        void Parse(byte[] bytes);
    }
    /// <summary>
    /// 协议包
    /// </summary>
    [Serializable]
    public class NetMessagePackage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; private set; }

        /// <summary>
        /// 协议长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 网络传输真实的数据
        /// </summary>
        private byte[] _content;
        /// <summary>
        /// 网络传输缓存
        /// </summary>
        private byte[] _data;
        /// <summary>
        /// 对象
        /// </summary>
        private object obj;

        private const int MessageTypeLength = 1;
        private const int ContentLengthLength = 4;
        private const int HeaderLength = MessageTypeLength + ContentLengthLength;
        private static List<byte> _cacheBytes = new List<byte>();

        /// <summary>
        /// 不允许直接创建
        /// </summary>
        private NetMessagePackage()
        {
            _content = null;
            _data = null;
        }

        /// <summary>
        /// 创建发送消息包
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// 用于创建发送消息
        /// </remarks>
        public static NetMessagePackage CreateSendMessagePackage<T>(T data) where T : INetData
        {
            NetMessagePackage package = new NetMessagePackage();
            package.AddMessage(data.Type, data);
            return package;
        }
        /// <summary>
        /// 创建发送消息包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// 用于创建接收消息，需提前确认数据完整性
        /// </remarks>
        public static NetMessagePackage CreateSendMessagePackage(MessageType type, byte[] data)
        {
            NetMessagePackage package = new NetMessagePackage();
            package.AddMessage(type, data);
            return package;
        }
        /// <summary>
        /// 创建接收消息包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// 用于创建接收消息，需提前确认数据完整性
        /// </remarks>
        public static NetMessagePackage CreateReceiveMessagePackage(byte[] data)
        {
            var package = new NetMessagePackage
            {
                _data = data
            };

            package.AnalyzeNetworkData();

            return package;
        }

        /// <summary>
        /// 创建接收消息包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// 用于创建接收消息，需提前确认数据完整性
        /// </remarks>
        public static NetMessagePackage CreateReceiveMessagePackage(ref List<byte> data)
        {
            var package = new NetMessagePackage();

            package.AnalyzeNetworkData(ref data);

            return package;
        }

        /// <summary>
        /// 检查数据流中的数据完整性
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static bool CheckDataComplete(byte[] datas)
        {
            if (datas is null // 空引用
                || datas.Length == 0 // 空数据
                || datas.Length <= HeaderLength // 数据未达到可解析长度
                )
            {
                return false;
            }

            var index = MessageTypeLength;
            var contentLengthBuffer = datas[index..(index + ContentLengthLength)];
            var contentLength = 0;

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(contentLengthBuffer);
            }

            contentLength = BitConverter.ToInt32(contentLengthBuffer);

            return datas.Length - HeaderLength >= contentLength;
        }
        /// <summary>
        /// 检查数据流中的数据完整性
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CheckDataComplete(List<byte> datas)
        {
            if (datas is null // 空引用
                || datas.Count == 0 // 空数据
                || datas.Count <= HeaderLength // 数据未达到可解析长度
                )
            {
                return false;
            }

            var index = MessageTypeLength;
            var contentLengthBuffer = datas.ToArray()[..(index + ContentLengthLength)];
            var contentLength = 0;

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(contentLengthBuffer);
            }

            contentLength = BitConverter.ToInt32(contentLengthBuffer);

            return datas.Count - HeaderLength >= contentLength;
        }
        /// <summary>
        /// 获取消息包中的内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetMessageContent<T>() where T : INetData, new()
        {
            try
            {
                if (obj == null)
                {
                    T t = new T();
                    t.Parse(_content);
                    obj = t;
                }

                return (T)obj;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        /// <summary>
        /// 获取消息包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public byte[] GetMessageData()
        {
            return _data;
        }
        /// <summary>
        /// 解析网络流数据
        /// </summary>
        private void AnalyzeNetworkData(ref List<byte> data)
        {
            Type = (MessageType)data[0];
            byte[] bytes = data.ToArray();
            var index = MessageTypeLength;
            var buffer = bytes[index..(index + ContentLengthLength)];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            Length = BitConverter.ToInt32(buffer);

            index += ContentLengthLength;

            buffer = bytes[index..(index + Length)];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            _content = buffer;

            index += Length;
            _data = bytes[..index];
            if (index == bytes.Length)
            {
                data.Clear();
                return;
            }
            buffer = bytes[index..];
            data.Clear();
            data.AddRange(buffer);
        }
        /// <summary>
        /// 解析网络流数据
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        private void AnalyzeNetworkData()
        {
            if (_data is null || _data.Length == 0)
            {
                throw new ArgumentNullException($"{nameof(_data)} is empty");
            }

            Type = (MessageType)_data[0];

            var index = MessageTypeLength;
            var buffer = _data[index..(index + ContentLengthLength)];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            Length = BitConverter.ToInt32(buffer);

            index += ContentLengthLength;

            buffer = _data[index..(index + Length)];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            _content = buffer;

            // TODO: 如果有剩余的未使用字节，还需要对剩余部分进行缓存
        }
        /// <summary>
        /// 添加一个message
        /// </summary>
        private void AddMessage(MessageType messageType, INetData data)
        {
            AddMessage(messageType, data.Format());
        }
        /// <summary>
        /// 添加一个message
        /// </summary>
        private void AddMessage(MessageType messageType, byte[] content)
        {
            _cacheBytes.Clear();
            _cacheBytes.Add((byte)messageType);
            Type = messageType;
            Length = (int)content.Length;
            byte[] buffer = BitConverter.GetBytes(Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            _cacheBytes.AddRange(buffer);
            _content = content;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_content);
            }
            _cacheBytes.AddRange(_content);

            _data = _cacheBytes.ToArray();

        }
    }
}
#endif