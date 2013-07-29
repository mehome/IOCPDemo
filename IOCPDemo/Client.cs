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

    // Implements the connection logic for the socket client.
    class Client : IDisposable
    {
        public Int32 index;

        // Constants for socket operations.
        private const Int32 ReceiveOperation = 1, SendOperation = 0;

        // The socket used to send/receive messages.
        private Socket clientSocket;

        // Flag for connected socket.
        private Boolean connected = false;

        // Listener endpoint.
        private IPEndPoint hostEndPoint;

        // Signals a connection.
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false); 

        // Signals the send/receive operation.
        private static AutoResetEvent[] autoSendReceiveEvents = new AutoResetEvent[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        private MessageSerializer serializer;

        private MessageUserToken messageUserToken;

        // Create an uninitialized client instance.  
        // To start the send/receive processing
        // call the Connect method followed by SendReceive method.
        internal Client(Int32 index, String hostName, Int32 port)
        {
            // Get host related information.
            IPHostEntry host = Dns.GetHostEntry(hostName);

            // Addres of the host.
            IPAddress[] addressList = host.AddressList;

            // Instantiates the endpoint and socket.
            this.hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);
            //this.hostEndPoint = new IPEndPoint(0, port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.index = index;
            this.serializer = new MessageSerializer();
            this.messageUserToken = new MessageUserToken();
            this.messageUserToken.MessageReceived += new EventHandler<MessageEventArgs>(OnMessageReceived);
        }

        // Connect to the host.
        internal void Connect()
        {
            Console.WriteLine("[Client] Client #{0} starts connecting", index);
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

            connectArgs.UserToken = this.clientSocket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            clientSocket.ConnectAsync(connectArgs);
            autoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
            {
                //throw new SocketException((Int32)errorCode);
                Console.WriteLine("SocketException: {0}", errorCode);
            }
        }

        // Disconnect from the host.
        internal void Disconnect()
        {
            Console.WriteLine("[Client] Client #{0} disconnected", index);
            clientSocket.Disconnect(false);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of connection.
            autoConnectEvent.Set();

            // Set the flag for socket connected.
            this.connected = (e.SocketError == SocketError.Success);
            Console.WriteLine("[Client] Client #{0} is connected: {1}", index, this.connected);
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of receive.
            autoSendReceiveEvents[SendOperation].Set();
            //Console.WriteLine("[Client] OnReceive: {0}", e.GetHashCode());
            messageUserToken.ProcessBuffer(e);
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {

             HelloMessage msg = (HelloMessage)e.Message;
             Console.WriteLine("[Client] OnMessageReceived: received server msg: '{0}'.", msg.Message);
             SendReceive("hello again");
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            //Console.WriteLine("[Client] OnSend: {0}", e.GetHashCode());
            // Signals the end of send.
            autoSendReceiveEvents[ReceiveOperation].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving.
                    Socket s = e.UserToken as Socket;

                    SocketAsyncEventArgs receiveArg = new SocketAsyncEventArgs();
                    receiveArg.AcceptSocket = s;
                    Byte[] receiveBuffer = new Byte[255];
                    receiveArg.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    receiveArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    s.ReceiveAsync(receiveArg);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        // Close socket in case of failure and throws a SockeException according to the SocketError.
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

        // Exchange a message with the host.
        internal String SendReceive(String message)
        {
            Console.WriteLine("[Client] Client #{0}, send receive: {1}, connected: {2}", index, message, connected);
            if (this.connected)
            {
                // Use google buf
                HelloMessage helloMessage = new HelloMessage
                {
                    Message = message,
                    ClientID = this.index,
                };

                Byte[] sendBuffer = this.serializer.SerializeWithPrefix(helloMessage);

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
                return message;
            }
            else
            {
                Console.WriteLine("Socket not connected");
                //throw new SocketException((Int32)SocketError.NotConnected);
                return "";
            }
        }

        // Use Current timestamp as message id
        private Int32 GetMessageID()
        {
            return 1;
            //TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            //return (Int32)t.TotalSeconds;
        }

        #region IDisposable Members

        // Disposes the instance of SocketClient.
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
