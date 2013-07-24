using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceModel.Channels;
using ProtoSharp;

namespace IOCPDemo
{

    enum MessageType
    {
        Hello = 1,
        Goodbye = 2,
    }

    enum MessageDirection
    {
        FromServer = 1,
        FromClient = 2,
    }

    [ProtoMessage]
    public class HelloMessage
    {
        [ProtoField(1)]
        public Int32 Type = (Int32)MessageType.Hello;
        [ProtoField(2)]
        public Int32 ID { get; set; }
        [ProtoField(3)]
        public Int32 Direction { get; set; }
        [ProtoField(4)]
        public String Message { get; set; }
        [ProtoField(5)]
        public Int32 SessionID { get; set; }
    }

    // Protobuf 的封装
    internal class MessageSerializer
    {

        internal MessageSerializer()
        {

        }

        public static String ByteArrayToHex(Byte[] buffer)
        {
            String result = "{";
            foreach (byte b in buffer)
            {
                result += " 0x" + Convert.ToString(b, 16).ToUpper().PadLeft(2, '0');
            }
            result += " }";
            return result;
        }

        // 串行化消息
        public Byte[] Serialize(HelloMessage message)
        {
            return ProtoSerializer<HelloMessage>.Serialize(message);
        }

        // 串行化消息，并加入消息长度的前缀
        public Byte[] SerializeWithPrefix(HelloMessage message)
        {
            Byte[] messageBuffer = Serialize(message);

            Byte[] prefixBuffer = BitConverter.GetBytes((Int16)Buffer.ByteLength(messageBuffer));
            Int32 msgLength = 2 + messageBuffer.Length;
            Byte[] resultBuffer = new Byte[msgLength];

            Buffer.BlockCopy(prefixBuffer, 0, resultBuffer, 0, 2);
            Buffer.BlockCopy(messageBuffer, 0, resultBuffer, 2, messageBuffer.Length);
            return resultBuffer;
        }

        // 解析某一个类型的消息
        public T Deserialize<T>(Byte[] buffer)
        {
            return ProtoSerializer<T>.Deserialize(buffer);
        }

        // 解析某一个类型的消息
        public T Deserialize<T>(Byte[] buffer, Int32 index, Int32 count)
        {
            return ProtoSerializer<T>.Deserialize(buffer, index, count);
        }

    }

}
