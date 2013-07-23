using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace IOCPDemo
{
    // 消息的封装
    internal class MessageEventArgs : EventArgs
    {
        public readonly String message;
    }

    // 组装 Buffer，解析 Message，并以 MessageReceived 事件将 Message 抛出
    internal class MessageUserToken
    {
        // 当接受并解析 Message 成功时抛出
        internal event EventHandler<MessageEventArgs> MessageReceived;

        // Only for testing
        //internal readonly Int32 tokenId;

        private Byte[] bufferReceived;

        internal MessageUserToken()
        {
            bufferReceived = new Byte[] {};
            Console.WriteLine("MessageUserToken:bufferReceived: {0}", bufferReceived);
        }

        // 处理 Buffer，当 Message 可以被解析时，抛出事件
        public void AppendBuffer(SocketAsyncEventArgs e)
        {
            Console.WriteLine("MessageUserToken:AppendBuffer: {0}", e.BytesTransferred);
            //The BytesTransferred property tells us how many bytes 
            //we need to process.
            Int32 remainingBytesToProcess = e.BytesTransferred;

            Int32 newBufferLength = e.Buffer.Length + bufferReceived.Length;
            Byte[] newBuffer = new Byte[newBufferLength];
            // Concat buffer
            Buffer.BlockCopy(bufferReceived, 0, newBuffer, 0, bufferReceived.Length);
            Buffer.BlockCopy(e.Buffer, 0, newBuffer, bufferReceived.Length, e.Buffer.Length);
            bufferReceived = newBuffer;
            Console.WriteLine("MessageUserToken:AppendBuffer:bufferReceived: {0}", Buffer.ByteLength(bufferReceived));

            // Read prefix
        }

        public void Reset()
        {
            Console.WriteLine("MessageUserToken:Reset:");
        }
    }
}
