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
        public readonly HelloMessage Message;

        internal MessageEventArgs(HelloMessage message)
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

        private Int32 receivePrefixLength = 2;
        internal Int32 lengthOfCurrentIncomingMessage = -1;
        private Int32 maxMessageLength = 8192;

        private MessageSerializer serializer;

        internal MessageUserToken()
        {
            bufferReceived = new Byte[] {};
            serializer = new MessageSerializer();
            //Console.WriteLine("MessageUserToken:bufferReceived: {0}", bufferReceived);
        }

        // 处理 Buffer，当 Message 可以被解析时，抛出事件
        public void ProcessBuffer(SocketAsyncEventArgs e)
        {
            //Console.WriteLine("[Server] MessageUserToken:AppendBuffer: BytesTransferred: {0}, Buffer.Length: {1}", e.BytesTransferred, e.Buffer.Length);
            // 获得当前接收到的 Buffer 长度
            Int32 remainingBytesToProcess = e.BytesTransferred;

            Int32 newBufferLength = bufferReceived.Length + remainingBytesToProcess;
            //Console.WriteLine("[Server] MessageUserToken:AppendBuffer: bufferReceived: {0}, newBufferLength: {1}", bufferReceived.Length, newBufferLength);

            Byte[] newBuffer = new Byte[newBufferLength];
            // Concat buffer
            Buffer.BlockCopy(bufferReceived, 0, newBuffer, 0, bufferReceived.Length);
            Buffer.BlockCopy(e.Buffer, 0, newBuffer, bufferReceived.Length, remainingBytesToProcess);
            bufferReceived = newBuffer;
            newBuffer = null;
            //Console.WriteLine("[Server] MessageUserToken:AppendBuffer: new bufferReceived: {0}", bufferReceived.Length);
            //if (bufferReceived.Length < 40)
            //{
            //    Console.WriteLine("[Server] bufferReceived: {0}", MessageSerializer.ByteArrayToHex(bufferReceived));
            //}
  
            Int32 offset = 0;
            // 如果接收到的 Buffer 长度可以独到 prefix 的话
            while (bufferReceived.Length - offset > receivePrefixLength)
            {
                //Console.WriteLine("[Server] Parse buffer from: {0}, to: {1}", offset, bufferReceived.Length);
                // 如果还不知道消息长度的话
                if (lengthOfCurrentIncomingMessage < 0)
                {
                    lengthOfCurrentIncomingMessage = (Int32) BitConverter.ToInt16(bufferReceived, offset);
                }
                //Console.WriteLine("[Server] Got message length: {0}", lengthOfCurrentIncomingMessage);
                if (lengthOfCurrentIncomingMessage > maxMessageLength)
                {
                    lengthOfCurrentIncomingMessage = -1;
                    bufferReceived = new Byte[] {};
                    Console.WriteLine("[Server] Prefix length to large. May be a bad message. Drop it!");
                    return;
                }

                // 如果消息还没有接受完成的话
                if (bufferReceived.Length - offset - 2 < lengthOfCurrentIncomingMessage)
                {
                    //Console.WriteLine("[Server] Message is not complete: {0} vs {1}", (bufferReceived.Length - 2), lengthOfCurrentIncomingMessage);
                    break;
                }
                HandleMessage(e, bufferReceived, offset + 2, lengthOfCurrentIncomingMessage);
                offset += (2 + lengthOfCurrentIncomingMessage);
                lengthOfCurrentIncomingMessage = -1;
                //Console.WriteLine("[Server] Update offset to: {0}", offset);
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
            //Console.WriteLine("[Server] bufferReceived: {0}", bufferReceived.Length);
            if (bufferReceived.Length > 0 && bufferReceived.Length < 40)
            {
                //Console.WriteLine("[Server] bufferReceived: {0}", MessageSerializer.ByteArrayToHex(bufferReceived));
            }
            newBuffer = null;
        }

        public void Reset()
        {
            //Console.WriteLine("MessageUserToken:Reset:");
            bufferReceived = new Byte[] { };
            lengthOfCurrentIncomingMessage = -1;

        }

        private void HandleMessage(SocketAsyncEventArgs socket, Byte[] buffer, Int32 index, Int32 count)
        {
            HelloMessage message = (HelloMessage)serializer.Deserialize(buffer, index, count);
            MessageEventArgs e = new MessageEventArgs(message);
            EventHandler<MessageEventArgs> handler = Volatile.Read(ref MessageReceived);
            if (handler != null)
            {
                handler(socket, e);
            }
        }

    }
}