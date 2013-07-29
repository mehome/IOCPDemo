using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using ProtoSharp;
using ProtoSharp.CodeGeneration;
using System.Reflection;

namespace IOCPDemo
{
    class Program
    {
        private static int port = 9092;

        private static int maxClients = 1;

        private static Server server;

        private static List<Client> clients;

        private static Int32 clientId;

        static void Main(string[] args)
        {
            //TestProtoBuf();
            InitializeProtoSerializers();

            clients = new List<Client>();

            //StartServer();
            
            Console.WriteLine("Press any key to create client...");
            Console.ReadKey();

            for (int i = 0; i < maxClients; i++)
            {
                //ThreadPool.QueueUserWorkItem(o => StartClientAndSendMessage("hello hello hello hello hello hello hello hello hello"));
                StartClientAndSendMessage("hello");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void InitializeProtoSerializers()
        {
#if !MONO_MODE
            ProtoGeneratorSettings settings = new ProtoGeneratorSettings();
            {
                settings.InputAssemblyFileNames.Add(Assembly.GetExecutingAssembly().Location);
                settings.TargetAssembly = "messages.dll";
                settings.CSharpFileName = "..\\..\\GeneratedSerializers.cs";
                settings.ProtoFileName = "messages.proto";
#if DEBUG
                settings.DebugCompilation = true;
#endif
                settings.GenerateMetaData = true;
                settings.MetaDataFileName = "messages.metadata";
                settings.LoadTargetAutomatically = true;
            }

            ProtoGenerator.Process(settings);
#endif
            ProtoModuleLoader.InitializeSerializers();
        }

        /*
        private static void TestProtoBuf()
        {
            MessageSerializer serializer = new MessageSerializer();
            HelloMessage msg = new HelloMessage
            {
                Type = 1,
                ID = 2,
                Direction = 3,
                Message = "4"
            };
            Console.WriteLine("[Client] serialize: {0}, {1}, {2}, {3}", msg.Type, msg.ID, msg.Message, msg.Direction);
            Byte[] rawBuffer = serializer.Serialize(msg);
            Console.WriteLine("[Client] buffer: {0}", MessageSerializer.ByteArrayToHex(rawBuffer));
            HelloMessage bmsg = serializer.Deserialize<HelloMessage>(rawBuffer);
            Console.WriteLine("[Client] deserialize: {0}, {1}, {2}, {3}", bmsg.Type, bmsg.ID, bmsg.Message, bmsg.Direction);
        }
        */

        private static void StartServer()
        {
            Console.WriteLine("[Server] StartServer");
            server = new Server(maxClients, 32);
            server.Start(port);
        }

        private static Client StartClient()
        {
            lock (clients)
            {
                clientId = Interlocked.Increment(ref clientId);

                Console.WriteLine("[Client] StartClient: {0}", clientId);
                Client client = new Client(clientId, "localhost", port);
                client.Connect();
                clients.Add(client);
                Console.WriteLine("[Client] Add client #{0} to list. size: {1}", client.index, clients.Count);
                return client;
            }
        }

        private static void SendMessageToAllClients(Object sender, EventArgs e)
        {
            Console.WriteLine("[Client] SendMessageToAllClients. size: {0}", clients.Count);
            foreach (Client c in clients)
            {
                Console.WriteLine("[Client] Send message to client #{0}", c.index);
                c.SendReceive("hello");
            }
        }

        private static void StartClientAndSendMessage(String message)
        {
            Client client = StartClient();
            message += " from client #" + client.index.ToString();
            // Create a timer with a ten second interval.
            System.Timers.Timer t = new System.Timers.Timer(2000);

            // Hook up the Elapsed event for the timer.
            t.Elapsed += new ElapsedEventHandler((o, e) => 
                {
                    client.SendReceive(message);
                    t.Stop();
                }
            );

            // Set the Interval to 2 seconds (2000 milliseconds).
            //t.Interval = 10000;
            t.Enabled = true;
            t.Start();
        }
    }
}
