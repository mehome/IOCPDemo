using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace IOCPDemo
{


    class Server
    {

        /// <summary>
        /// 监听Socket，用于接受客户端的连接请求
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// 用于服务器执行的互斥同步对象
        /// </summary>
        private static Mutex mutex = new Mutex();

        /// <summary>
        /// 用于每个I/O Socket操作的缓冲区大小
        /// </summary>
        private Int32 bufferSize;

        /// <summary>
        /// 服务器上连接的客户端总数
        /// </summary>
        private Int32 numConnectedSockets;

        /// <summary>
        /// 服务器能接受的最大连接数量
        /// </summary>
        private Int32 numConnections;

        /// <summary>
        /// 完成端口上进行投递所用的连接对象池
        /// </summary>
        private SocketAsyncEventArgsPool eventArgsPool;


        /// <summary>
        /// 构造函数，建立一个未初始化的服务器实例
        /// </summary>
        /// <param name="numConnections">服务器的最大连接数据</param>
        /// <param name="bufferSize"></param>
        public Server(Int32 numConnections = 1024, Int32 bufferSize = 8192)
        {
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.bufferSize = bufferSize;

            eventArgsPool = new SocketAsyncEventArgsPool(numConnections);

            // 为IoContextPool预分配SocketAsyncEventArgs对象
            for (Int32 i = 0; i < this.numConnections; i++)
            {
                SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                MessageUserToken msgUserToken = new MessageUserToken();
                msgUserToken.MessageReceived += new EventHandler<MessageEventArgs>(OnMessageReceived);
                eventArg.UserToken = msgUserToken;

                eventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                eventArg.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);
                Console.WriteLine("Server:initEventArg: {0}", eventArg.GetHashCode());
                // 将预分配的对象加入SocketAsyncEventArgs对象池中
                eventArgsPool.Push(eventArg);
            }
        }

        /// <summary>
        /// 启动服务，开始监听
        /// </summary>
        /// <param name="port">Port where the server will listen for connection requests.</param>
        public void Start(Int32 port)
        {
            Console.WriteLine("Server start: {0}", port);
            // 获得主机相关信息
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            Console.WriteLine("IP Addresses: {0}. Machine name: {1}", addressList, Environment.MachineName);
            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

            // 创建监听socket
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.ReceiveBufferSize = bufferSize;
            listenSocket.SendBufferSize = bufferSize;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Console.WriteLine("Server start. ipv6: {0}", localEndPoint);

                // 配置监听socket为 dual-mode (IPv4 & IPv6) 
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                Console.WriteLine("Server start. ipv4: {0}", localEndPoint);
                listenSocket.Bind(localEndPoint);
            }

            // 开始监听
            this.listenSocket.Listen(this.numConnections);

            // 在监听Socket上投递一个接受请求。
            this.StartAccept(null);

            // Blocks the current thread to receive incoming messages.
            mutex.WaitOne();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            listenSocket.Close();
            
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// 当Socket上的发送或接收请求被完成时，调用此函数
        /// </summary>
        /// <param name="sender">激发事件的对象</param>
        /// <param name="e">与发送或接收完成操作相关联的SocketAsyncEventArg对象</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("ONIOCompleted");
            // Determine which type of operation just completed and call the associated handler.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            dynamic message = e.Message;
            Console.WriteLine("OnMessageReceived. id: {0}, msg: {1}", message.ID, message.Message);
        }

        /// <summary>
        ///接收完成时处理函数
        /// </summary>
        /// <param name="e">与接收完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Console.WriteLine("ProcessReceive");
            // 检查远程主机是否关闭连接
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Socket s = e.AcceptSocket;
                    //判断所有需接收的数据是否已经完成
                    //Console.WriteLine("ProcessReceive:Available: {0}", s.Available);
                    Console.WriteLine("ProcessReceive:e: {0}", e.GetHashCode());
                    if (s.Available == 0)
                    {
                        // 设置发送数据
                        // 不返回其他的
                        //Array.Copy(e.Buffer, 0, e.Buffer, e.BytesTransferred, e.BytesTransferred);
                        //e.SetBuffer(e.Offset, e.BytesTransferred * 2);
                        //Console.WriteLine("ProcessReceive:SendBuffer: {0}", Encoding.ASCII.GetString(e.Buffer));
                        //if (!s.SendAsync(e))        //投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                        //{
                        //    // 同步发送时处理发送完成事件
                        //    this.ProcessSend(e);
                        //}
                        MessageUserToken messageUserToken = (MessageUserToken)e.UserToken;
                        messageUserToken.AppendBuffer(e);
                    }

                    //为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                    if (!s.ReceiveAsync(e))
                    {
                        // 同步接收时处理接收完成事件
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    this.ProcessError(e);
                }
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }

        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            Console.WriteLine("ProcessSend:e: {0}", e.GetHashCode());
            if (e.SocketError == SocketError.Success)
            {
                Socket s = e.AcceptSocket;

                //接收时根据接收的字节数收缩了缓冲区的大小，因此投递接收请求时，恢复缓冲区大小
                e.SetBuffer(0, bufferSize);
                if (!s.ReceiveAsync(e))     //投递接收请求
                {
                    // 同步接收时处理接收完成事件
                    this.ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        /// <summary>
        /// 处理socket错误
        /// </summary>
        /// <param name="e"></param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            IPEndPoint localEndPoint = s.LocalEndPoint as IPEndPoint;

            this.CloseClientSocket(s, e);

            Console.WriteLine("Socket error {0} on endpoint {1} during {2}.", (Int32)e.SocketError, localEndPoint, e.LastOperation);
        }

        /// <summary>
        /// 关闭socket连接
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            this.CloseClientSocket(s, e);
        }

        private void CloseClientSocket(Socket s, SocketAsyncEventArgs e)
        {
            Console.WriteLine("CloseClientSocket");
            Interlocked.Decrement(ref this.numConnectedSockets);

            // SocketAsyncEventArg 对象被释放，压入可重用队列。
            e.AcceptSocket = null;
            ((MessageUserToken)e.UserToken).Reset();
            eventArgsPool.Push(e);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);
            
            try
            {
                s.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                s.Close();
            }
        }

        /// <summary>
        /// accept 操作完成时回调函数
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// 监听Socket接受处理
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Console.WriteLine("ProcessAccept");
            Socket s = e.AcceptSocket;
            if (s.Connected)
            {
                try
                {
                    SocketAsyncEventArgs eventArg = this.eventArgsPool.Pop();
                    Console.WriteLine("ProcessAccept:eventArg: {0}", eventArg.GetHashCode());
                    if (eventArg != null)
                    {
                        // 从接受的客户端连接中取数据配置ioContext
                        eventArg.AcceptSocket = e.AcceptSocket;

                        Interlocked.Increment(ref this.numConnectedSockets);
                        Console.WriteLine("Client connection accepted. There are {0} clients connected to the server", this.numConnectedSockets);

                        if (!s.ReceiveAsync(eventArg))
                        {
                            this.ProcessReceive(eventArg);
                        }
                    }
                    else
                    {
                        //已经达到最大客户连接数量，在这接受连接，发送“连接已经达到最大数”，然后断开连接
                        s.Send(Encoding.Default.GetBytes("连接已经达到最大数!"));
                        Console.WriteLine("Max client connections");
                        s.Close();
                    }
                }
                catch (SocketException ex)
                {
                    Socket token = e.AcceptSocket;
                    Console.WriteLine("Error when processing data received from {0}: {1}", token.RemoteEndPoint, ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex);
                }
                // 投递下一个接受请求
                this.StartAccept(e);
            }
        }

        /// <summary>
        /// 从客户端开始接受一个连接操作
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                Console.WriteLine("StartAccept:newEventArg: {0}", acceptEventArg.GetHashCode());

            }
            else
            {
                Console.WriteLine("StartAccept:eventArg: {0}", acceptEventArg.GetHashCode());

                // 重用前进行对象清理
                acceptEventArg.AcceptSocket = null;
                //Console.WriteLine("Need to clean AcceptSocket?");
            }

            if (!this.listenSocket.AcceptAsync(acceptEventArg))
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

    }
}
