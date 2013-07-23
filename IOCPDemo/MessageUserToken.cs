using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using ProtoBuf;

namespace IOCPDemo
{
    // 消息的封装
    internal class MessageEventArgs : EventArgs
    {
        public readonly BaseMessage Message;

        internal MessageEventArgs(BaseMessage message)
        {
            this.Message = message;
        }
    }

    // 组装 Buffer，解析 Message，并以 MessageReceived 事件将 Message 抛出
    internal class MessageUserToken
    {
        // 当接受并解析 Message 成功时抛出
        internal event EventHandler<MessageEventArgs> MessageReceived;

        // Only for testing
        //internal readonly Int32 tokenId;
        private Byte[] bufferReceived;

        //private Int32 receivedPrefixBytesDoneCount = 0;
        //private Byte[] byteArrayForPrefix;
        private Int32 receivePrefixLength = 2;
        internal Int32 receiveMessageOffset = 0;
        internal Int32 receivedMessageBytesDoneCount = 0;
        internal Int32 receivePrefixBytesDoneThisOp = 0;
        internal Int32 lengthOfCurrentIncomingMessage = -1;

        private MessageSerializer serializer;

        internal MessageUserToken()
        {
            bufferReceived = new Byte[] {};
            serializer = new MessageSerializer();
            Console.WriteLine("MessageUserToken:bufferReceived: {0}", bufferReceived);
        }

        // 处理 Buffer，当 Message 可以被解析时，抛出事件
        public void AppendBuffer(SocketAsyncEventArgs e)
        {
            Console.WriteLine("MessageUserToken:AppendBuffer: BytesTransferred: {0}, Buffer.Length: {1}", e.BytesTransferred, e.Buffer.Length);
            // 获得当前接收到的 Buffer 长度
            Int32 remainingBytesToProcess = e.BytesTransferred;

            Int32 newBufferLength = bufferReceived.Length + remainingBytesToProcess;
            Console.WriteLine("MessageUserToken:AppendBuffer: bufferReceived: {0}, newBufferLength: {1}", bufferReceived.Length, newBufferLength);

            Byte[] newBuffer = new Byte[newBufferLength];
            // Concat buffer
            Buffer.BlockCopy(bufferReceived, 0, newBuffer, 0, bufferReceived.Length);
            Buffer.BlockCopy(e.Buffer, 0, newBuffer, bufferReceived.Length, remainingBytesToProcess);
            bufferReceived = newBuffer;
            newBuffer = null;
            Console.WriteLine("MessageUserToken:AppendBuffer: new bufferReceived: {0}", bufferReceived.Length);
            if (bufferReceived.Length < 40)
            {
                Console.WriteLine("bufferReceived: {0}", ByteArrayToHex(bufferReceived));
            }
  
            Int32 offset = 0;
            // 如果接收到的 Buffer 长度可以独到 prefix 的话
            while (bufferReceived.Length - offset > receivePrefixLength)
            {
                Console.WriteLine("Parse buffer from: {0}, to: {1}", offset, bufferReceived.Length);
                // 如果还不知道消息长度的话
                if (lengthOfCurrentIncomingMessage < 0)
                {
                    lengthOfCurrentIncomingMessage = (Int32) BitConverter.ToInt16(bufferReceived, offset);
                }
                Console.WriteLine("Got prefix length: {0}", lengthOfCurrentIncomingMessage);
                // 如果消息还没有接受完成的话
                if (bufferReceived.Length - offset - 2 < lengthOfCurrentIncomingMessage)
                {
                    Console.WriteLine("Message is not complete: {0} vs {1}", (bufferReceived.Length - 2), lengthOfCurrentIncomingMessage);
                    break;
                }
                // TODO 这里可以不用新建 Byte[]
                HandleMessage(bufferReceived, offset + 2, lengthOfCurrentIncomingMessage);
                offset += (2 + lengthOfCurrentIncomingMessage);
                lengthOfCurrentIncomingMessage = -1;
                Console.WriteLine("Update offset to: {0}", offset);
            }
            // 截取已经处理过的 Buffer
            if (offset > 0)
            {
                if (offset == bufferReceived.Length)
                {
                    bufferReceived = new Byte[] {};
                } else {
                    newBuffer = new Byte[bufferReceived.Length - offset];
                    Buffer.BlockCopy(bufferReceived, offset, newBuffer, 0, (bufferReceived.Length - offset));
                    bufferReceived = newBuffer;
                }
            }
            Console.WriteLine("bufferReceived: {0}", bufferReceived.Length);
            if (bufferReceived.Length > 0 && bufferReceived.Length < 40)
            {
                Console.WriteLine("bufferReceived: {0}", ByteArrayToHex(bufferReceived));
            }
            newBuffer = null;
        }

        public void Reset()
        {
            Console.WriteLine("MessageUserToken:Reset:");
        }

        private void HandleMessage(Byte[] buffer, Int32 index, Int32 count)
        {

            BaseMessage message = (BaseMessage)serializer.Deserialize(buffer, index, count);
            MessageEventArgs e = new MessageEventArgs(message);
            EventHandler<MessageEventArgs> temp = Volatile.Read(ref MessageReceived);
            if (temp != null)
            {
                temp(this, e);
            }
        }

        private String ByteArrayToHex(Byte[] buffer)
        {
            String result = "{";
            foreach (byte b in buffer)
            {
                result += " 0x" + Convert.ToString(b, 16).ToUpper().PadLeft(2, '0');
            }
            result += " }";
            return result;
        }
    }
}
