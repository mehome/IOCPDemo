using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

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

        public Byte[] Serialize(HelloMessage message)
        {
            //MemoryStream stream = new MemoryStream();
            //ProtoBuf.Serializer.Serialize(stream, message);
            //return stream.GetBuffer();
            Byte[] data;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, message);
                data = ms.ToArray();
            }
            return data;
        }

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

        // 解析其他类型的消息
        public HelloMessage Deserialize(Byte[] buffer)
        {

            HelloMessage msg;
            using (var ms = new MemoryStream(buffer))
            {
                //Console.WriteLine("array: {0}", MessageSerializer.ByteArrayToHex(ms.ToArray()));
                msg = Serializer.Deserialize<HelloMessage>(ms);

            }
            return msg;
        }

        // 解析其他类型的消息
        public HelloMessage Deserialize(Byte[] buffer, Int32 index, Int32 count)
        {

            HelloMessage msg;
            using (var ms = new MemoryStream(buffer, index, count))
            {
                //Console.WriteLine("before pos: {0}", ms.Position);
                //ms.Write(buffer, 0, buffer.Length);
                //Console.WriteLine("array: {0}", MessageSerializer.ByteArrayToHex(ms.ToArray()));
                //ms.Write(buffer, 0, Buffer.ByteLength(buffer));
                msg = Serializer.Deserialize<HelloMessage>(ms);
                //Console.WriteLine("after pos: {0}", ms.Position);
            }
            return msg;
        }

    }

}
