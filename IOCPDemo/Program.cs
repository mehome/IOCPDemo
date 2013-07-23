using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Collections;
using System.Threading;
using System.Timers;

namespace IOCPDemo
{
    class Program
    {

        private static int port = 9091;

        private static int maxClients = 1;

        private static Server server;

        private static List<Client> clients;

        private static Int32 clientId;

        static void Main(string[] args)
        {
            clients = new List<Client>();

            StartServer();

            Console.WriteLine("Press any key to create client...");
            Console.ReadKey();

            for (int i = 0; i < maxClients; i++)
            {
                ThreadPool.QueueUserWorkItem(o => StartClientAndSendMessage("hello"));
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void StartServer()
        {
            Console.WriteLine("StartServer");
            server = new Server(maxClients, 1024);
            server.Start(port);
        }

        private static Client StartClient()
        {
            clientId = Interlocked.Increment(ref clientId);

            Console.WriteLine("StartClient: {0}", clientId);
            Client client = new Client(clientId, "localhost", port);
            client.Connect();
            clients.Add(client);
            Console.WriteLine("Add client #{0} to list. size: {1}", client.index, clients.Count);
            return client;
        }

        private static void SendMessageToAllClients(Object sender, EventArgs e)
        {
            Console.WriteLine("SendMessageToAllClients. size: {0}", clients.Count);
            foreach (Client c in clients)
            {
                Console.WriteLine("Send message to client #{0}", c.index);
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
            t.Elapsed += new ElapsedEventHandler((o, e) => client.SendReceive(message));

            // Set the Interval to 2 seconds (2000 milliseconds).
            t.Interval = 5000;
            t.Enabled = true;
            t.Start();
        }
    }
}
