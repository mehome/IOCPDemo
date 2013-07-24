// Do not modify this file, it is automatically generated. 
// 
// Date: 07/24/2013 18:35:39

using System.Linq;

#region IOCPDemo.HelloMessage - HelloMessage_Serializer(7bb446a1-4835-3be4-979b-dd1513a35829)

namespace IOCPDemo
{
    public class HelloMessage_Serializer : ProtoSharp.IProtoSerializer<IOCPDemo.HelloMessage>
    {
        public static readonly HelloMessage_Serializer Instance = new HelloMessage_Serializer();

        #region Meta data

        public static readonly System.Guid MetaId_Static = new System.Guid("7bb446a1-4835-3be4-979b-dd1513a35829");

        public static readonly byte[] MetaData_Static = new byte[]
        {
            0x08, 0x02, 0x12, 0x15, 0x49, 0x4F, 0x43, 0x50, 0x44, 0x65, 0x6D, 0x6F, 0x2E, 0x48, 0x65, 0x6C, 
            0x6C, 0x6F, 0x4D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x1A, 0x10, 0xA1, 0x46, 0xB4, 0x7B, 0x35, 
            0x48, 0xE4, 0x3B, 0x97, 0x9B, 0xDD, 0x15, 0x13, 0xA3, 0x58, 0x29, 0x2A, 0x0C, 0x08, 0x02, 0x12, 
            0x04, 0x54, 0x79, 0x70, 0x65, 0x18, 0x8C, 0x80, 0x12, 0x2A, 0x0A, 0x08, 0x04, 0x12, 0x02, 0x49, 
            0x44, 0x18, 0x8C, 0x80, 0x12, 0x2A, 0x11, 0x08, 0x06, 0x12, 0x09, 0x44, 0x69, 0x72, 0x65, 0x63, 
            0x74, 0x69, 0x6F, 0x6E, 0x18, 0x8C, 0x80, 0x12, 0x2A, 0x0F, 0x08, 0x08, 0x12, 0x07, 0x4D, 0x65, 
            0x73, 0x73, 0x61, 0x67, 0x65, 0x18, 0x9C, 0xC0, 0x24, 0x2A, 0x11, 0x08, 0x0A, 0x12, 0x09, 0x53, 
            0x65, 0x73, 0x73, 0x69, 0x6F, 0x6E, 0x49, 0x44, 0x18, 0x8C, 0x80, 0x12
        };

        public static readonly ProtoSharp.Inspection.IProtoMetaProvider[] ReferenceMetaProviders_Static = null;

        public System.Guid MetaId
        {
            get
            {
                return MetaId_Static;
            }
        }

        public byte[] MetaData
        {
            get
            {
                return MetaData_Static;
            }
        }

        public ProtoSharp.Inspection.IProtoMetaProvider[] ReferenceMetaProviders
        {
            get
            {
                return ReferenceMetaProviders_Static;
            }
        }

        #endregion // Meta data

        #region Serialize

        public static byte[] Serialize_Static(IOCPDemo.HelloMessage message)
        {
            if (message == null)
            {
                return ProtoSharp.ProtoZeroLengthByteArray.Value;
            }

            ProtoSharp.ProtoWriter writer = new ProtoSharp.ProtoWriter();
            writer.WriteInt32(1, message.Type, ProtoSharp.ProtoFieldEncoding.VarintZigZag);
            writer.WriteInt32(2, message.ID, ProtoSharp.ProtoFieldEncoding.VarintZigZag);
            writer.WriteInt32(3, message.Direction, ProtoSharp.ProtoFieldEncoding.VarintZigZag);
            writer.WriteString(4, message.Message, ProtoSharp.ProtoFieldEncoding.Utf8);
            writer.WriteInt32(5, message.SessionID, ProtoSharp.ProtoFieldEncoding.VarintZigZag);

            return writer.ToBytesBuffer();
        }

        public static byte[] SerializeObject_Static(object message)
        {
            return Serialize_Static((IOCPDemo.HelloMessage)message);
        }

        public byte[] Serialize(IOCPDemo.HelloMessage message)
        {
            return Serialize_Static(message);
        }

        byte[] ProtoSharp.IProtoSerializer.Serialize(object message)
        {
            return Serialize_Static((IOCPDemo.HelloMessage)message);
        }

        #endregion // Serialize

        #region Deserialize

        internal static void DeserializeFields_Static(IOCPDemo.HelloMessage message, ProtoSharp.ProtoReader reader)
        {

            foreach (ProtoSharp.ProtoFieldReader field in reader)
            {
                switch (field.Tag)
                {
                    case 1:  // int Type
                        message.Type = field.ToInt32(ProtoSharp.ProtoFieldEncoding.VarintZigZag);
                        break;

                    case 2:  // int ID
                        message.ID = field.ToInt32(ProtoSharp.ProtoFieldEncoding.VarintZigZag);
                        break;

                    case 3:  // int Direction
                        message.Direction = field.ToInt32(ProtoSharp.ProtoFieldEncoding.VarintZigZag);
                        break;

                    case 4:  // string Message
                        message.Message = field.ToString(ProtoSharp.ProtoFieldEncoding.Utf8);
                        break;

                    case 5:  // int SessionID
                        message.SessionID = field.ToInt32(ProtoSharp.ProtoFieldEncoding.VarintZigZag);
                        break;

                    default:
                        break;
                }
            }
        }

        public static IOCPDemo.HelloMessage Deserialize_Static(byte[] buffer, int start, int length)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return new IOCPDemo.HelloMessage();
            }

            if (start < 0 || start >= buffer.Length)
            {
                throw new System.ArgumentOutOfRangeException("start");
            }

            if (length < 0 || start + length > buffer.Length)
            {
                throw new System.ArgumentOutOfRangeException("length");
            }

            IOCPDemo.HelloMessage message = new IOCPDemo.HelloMessage();

            ProtoSharp.ProtoReader reader = new ProtoSharp.ProtoReader(buffer, start, length);

            DeserializeFields_Static(message, reader);

            return message;
        }

        public static object DeserializeObject_Static(byte[] buffer, int start, int length)
        {
            return Deserialize_Static(buffer, start, length);
        }

        public IOCPDemo.HelloMessage Deserialize(byte[] buffer, int start, int length)
        {
            return Deserialize_Static(buffer, start, length);
        }

        public IOCPDemo.HelloMessage Deserialize(byte[] buffer)
        {
            if (buffer == null)
            {
                return Deserialize_Static(null, 0, 0);
            }
            else
            {
                return Deserialize_Static(buffer, 0, buffer.Length);
            }
        }

        object ProtoSharp.IProtoSerializer.Deserialize(byte[] buffer, int start, int length)
        {
            return Deserialize_Static(buffer, start, length);
        }

        object ProtoSharp.IProtoSerializer.Deserialize(byte[] buffer)
        {
            if (buffer == null)
            {
                return Deserialize_Static(null, 0, 0);
            }
            else
            {
                return Deserialize_Static(buffer, 0, buffer.Length);
            }
        }

        #endregion // Deserialize
    }
}

#endregion // IOCPDemo.HelloMessage - HelloMessage_Serializer(7bb446a1-4835-3be4-979b-dd1513a35829)

// Module

namespace ProtoSharp.GeneratedCodes
{

    public class _messages_SerializerModule : ProtoSharp.IProtoSerializerModule
    {
        public void Initialize()
        {

            #region ProtoSharp initialization

            ProtoSharp.ProtoSerializer<IOCPDemo.HelloMessage>.InitializeInstance(IOCPDemo.HelloMessage_Serializer.Instance);

            #endregion // ProtoSharp initialization
        }
    }
}
