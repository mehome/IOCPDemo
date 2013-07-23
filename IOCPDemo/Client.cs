using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace IOCPDemo
{

    /// <summary>
    /// Implements the connection logic for the socket client.
    /// </summary>
    class Client : IDisposable
    {
        public Int32 index;

        /// <summary>
        /// Constants for socket operations.
        /// </summary>
        private const Int32 ReceiveOperation = 1, SendOperation = 0;

        /// <summary>
        /// The socket used to send/receive messages.
        /// </summary>
        private Socket clientSocket;

        /// <summary>
        /// Flag for connected socket.
        /// </summary>
        private Boolean connected = false;

        /// <summary>
        /// Listener endpoint.
        /// </summary>
        private IPEndPoint hostEndPoint;

        /// <summary>
        /// Signals a connection.
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false); 

        /// <summary>
        /// Signals the send/receive operation.
        /// </summary>
        private static AutoResetEvent[] autoSendReceiveEvents = new AutoResetEvent[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        private MessageSerializer serializer;

        /// <summary>
        /// Create an uninitialized client instance.  
        /// To start the send/receive processing
        /// call the Connect method followed by SendReceive method.
        /// </summary>
        /// <param name="hostName">Name of the host where the listener is running.</param>
        /// <param name="port">Number of the TCP port from the listener.</param>
        internal Client(Int32 index, String hostName, Int32 port)
        {
            // Get host related information.
            IPHostEntry host = Dns.GetHostEntry(hostName);

            // Addres of the host.
            IPAddress[] addressList = host.AddressList;

            // Instantiates the endpoint and socket.
            this.hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.index = index;
            this.serializer = new MessageSerializer();
        }

        /// <summary>
        /// Connect to the host.
        /// </summary>
        /// <returns>True if connection has succeded, else false.</returns>
        internal void Connect()
        {
            Console.WriteLine("Client #{0} connect", index);
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

            connectArgs.UserToken = this.clientSocket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            clientSocket.ConnectAsync(connectArgs);
            autoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((Int32)errorCode);
                //Console.WriteLine("SocketException: {0}", errorCode);
            }
        }

        /// <summary>
        /// Disconnect from the host.
        /// </summary>
        internal void Disconnect()
        {
            Console.WriteLine("Client #{0} disconnected", index);
            clientSocket.Disconnect(false);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("Client #{0} OnConnect", index);
            // Signals the end of connection.
            autoConnectEvent.Set();

            // Set the flag for socket connected.
            this.connected = (e.SocketError == SocketError.Success);
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of receive.
            autoSendReceiveEvents[SendOperation].Set();
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of send.
            autoSendReceiveEvents[ReceiveOperation].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving.
                    Socket s = e.UserToken as Socket;

                    byte[] receiveBuffer = new byte[255];
                    e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    s.ReceiveAsync(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        /// <summary>
        /// Close socket in case of failure and throws a SockeException according to the SocketError.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the failed operation.</param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                // close the socket associated with the client
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // throws if client process has already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
            }

            // Throw the SocketException
            throw new SocketException((Int32)e.SocketError);
        }

        /// <summary>
        /// Exchange a message with the host.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>Message sent by the host.</returns>
        internal String SendReceive(String message)
        {
            Console.WriteLine("Client #{0}, send receive: {1}, connected: {2}", index, message, connected);
            if (this.connected)
            {
                // Use google buf
                HelloMessage helloMessage = new HelloMessage
                {
                    ID = this.GetMessageID(),
                    Direction = MessageDirection.FromClient,
                    Message = message
                };
                MemoryStream stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize(stream, helloMessage);
                Byte[] msgBuffer = this.serializer.Serialize(helloMessage);
            
                // Create a buffer to send.
                //Byte[] msgBuffer = Encoding.ASCII.GetBytes(message);
                Byte[] prefixBuffer = BitConverter.GetBytes((Int16)Buffer.ByteLength(msgBuffer));
                Int32 msgLength = 2 + msgBuffer.Length;
                Byte[] sendBuffer = new Byte[msgLength];

                Buffer.BlockCopy(prefixBuffer, 0, sendBuffer, 0, 2);
                Buffer.BlockCopy(msgBuffer, 0, sendBuffer, 2, msgBuffer.Length);
                //Buffer.BlockCopy(prefixBuffer, 0, sendBuffer, msgLength, 2);
                //Buffer.BlockCopy(msgBuffer, 0, sendBuffer, msgLength + 2, msgBuffer.Length);

                // Prepare arguments for send/receive operation.
                SocketAsyncEventArgs completeArgs = new SocketAsyncEventArgs();
                completeArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                completeArgs.UserToken = this.clientSocket;
                completeArgs.RemoteEndPoint = this.hostEndPoint;
                completeArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                // Start sending asyncronally.
                clientSocket.SendAsync(completeArgs);

                // Wait for the send/receive completed.
                AutoResetEvent.WaitAll(autoSendReceiveEvents);

                // Return data from SocketAsyncEventArgs buffer.
                return Encoding.ASCII.GetString(completeArgs.Buffer, completeArgs.Offset, completeArgs.BytesTransferred);
            }
            else
            {
                throw new SocketException((Int32)SocketError.NotConnected);
            }
        }

        private Int32 GetMessageID()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return (Int32)t.TotalSeconds;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the instance of SocketClient.
        /// </summary>
        public void Dispose()
        {
            autoConnectEvent.Close();
            autoSendReceiveEvents[SendOperation].Close();
            autoSendReceiveEvents[ReceiveOperation].Close();
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }

        #endregion
    }

}
