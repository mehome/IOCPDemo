using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;
using System.ServiceModel.Channels;

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

    [ProtoContract]
    internal class HelloMessage
    {
        [ProtoMember(1)]
        public Int32 Type = (Int32)MessageType.Hello;
        [ProtoMember(2)]
        public Int32 ID { get; set; }
        [ProtoMember(3)]
        public Int32 Direction { get; set; }
        [ProtoMember(4)]
        public String Message { get; set; }
        [ProtoMember(5)]
        public Int32 SessionID { get; set; }
    }

    // Protobuf 的封装
    internal class MessageSerializer
    {

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
            Byte[] data;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                data = stream.ToArray();
            }
            return data;
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
            T msg;
            using (var stream = new MemoryStream(buffer))
            {
                msg = Serializer.Deserialize<T>(stream);
            }
            return msg;
        }

        // 解析某一个类型的消息
        public T Deserialize<T>(Byte[] buffer, Int32 index, Int32 count)
        {
            T msg;
            using (var stream = new MemoryStream(buffer, index, count))
            {
                msg = Serializer.Deserialize<T>(stream);
            }
            return msg;
        }

    }

}
