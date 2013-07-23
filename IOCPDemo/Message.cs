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
        Hello,
        Goodbye,
    }

    enum MessageDirection
    {
        FromServer = 1,
        FromClient = 2,
    }

    [ProtoContract]
    internal class BaseMessage
    {
        public MessageType Type;
    }

    [ProtoContract]
    internal class HelloMessage : BaseMessage
    {
        [ProtoMember(1)]
        public MessageType Type = MessageType.Hello;
        [ProtoMember(2)]
        public Int32 ID { get; set; }
        [ProtoMember(3)]
        public MessageDirection Direction { get; set; }
        [ProtoMember(4)]
        public String Message { get; set; }
    }
    
    [ProtoContract]
    internal class GoodbyeMessage : BaseMessage
    {
        [ProtoMember(1)]
        public MessageType Type = MessageType.Goodbye;
        [ProtoMember(2)]
        public Int32 ID { get; set; }
        [ProtoMember(3)]
        public MessageDirection Direction { get; set; }
        [ProtoMember(4)]
        public String Message { get; set; }
    }

    // Protobuf 的封装
    internal class MessageSerializer
    {
        public Byte[] Serialize(BaseMessage message)
        {
            MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, message);
            return stream.GetBuffer();
        }

        public dynamic Deserialize(Byte[] buffer, Int32 index, Int32 count)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(buffer, index, count);
            BaseMessage message = ProtoBuf.Serializer.Deserialize<HelloMessage>(stream);
            return message;
        }

        public dynamic ConvertMessage(BaseMessage message)
        {
            switch (message.Type)
            { 
                case MessageType.Hello:
                    return (HelloMessage)message;
                case MessageType.Goodbye:
                    return (GoodbyeMessage)message;
                default:
                    throw new ArgumentException("Invalid message type");
            }
        }
    }

}
