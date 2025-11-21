using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WordleServer
{
    class Server
    {
        private const int PORT = 11020;
        private const string ADDRESS = "127.0.0.1";

        private static Socket serverSock = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            CreateServer();
        }

        private static void CreateServer()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ADDRESS), PORT);

            Console.WriteLine("Creating server...");
            serverSock.Bind(localEndPoint);

            serverSock.Listen(10);
            Console.WriteLine("Listening to {0}:{1}", ADDRESS, PORT);

            while(true)
            {
                Socket clientSock = serverSock.Accept();
                Thread clientThread = new Thread(() => HandleClient(clientSock));
                clientThread.Start();
            }
        }

        private static void HandleClient(Socket client)
        {
            IPEndPoint clientEndPoint = (IPEndPoint)client.RemoteEndPoint;

            Console.WriteLine("Accepted client: {0}:{1}", clientEndPoint?.Address.ToString(), clientEndPoint?.Port);
        }
    }
}