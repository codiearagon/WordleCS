using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WordleServer
{
    class Server
    {
        private const string ADDRESS = "127.0.0.1";
        private const int PORT = 11020;

        private static Socket serverSock = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static List<Room> rooms = new List<Room>(); 

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

            Player newPlayer = new Player(client);

            string message;
            while(true)
            {
                message = newPlayer.ReceiveString();
                HandleClientMessage(newPlayer, message);
            }


            client.Close();
        }

        private static void HandleClientMessage(Player player, string message)
        {
            string[] parts = message.Split(';');

            // first element is always the action, and following are arguments
            switch(parts[0])
            {
                case "set_username":
                    player.SetName(parts[1]);
                    Console.WriteLine("Set name: {0}", player.playerName);
                    break;
                case "create_room":
                    CreateRoom(player, parts[1]);
                    break;
                case "join_room":
                    JoinRoom(player, parts[1]);
                    break;
            }
        }

        private static void CreateRoom(Player host, string roomName)
        {
            foreach (Room room in rooms)
            {
                if (room.roomName == roomName)
                {
                    host.SendMessage("Room name already exists");
                    return;
                }
            }

            Room newRoom = new Room(host, roomName);
            rooms.Add(newRoom);
        }

        private static void JoinRoom(Player player, string roomName)
        {
            foreach (Room room in rooms)
            {
                if (room.roomName == roomName)
                {
                    room.AddPlayer(player);
                    player.SendMessage("Successfully joined room.");
                    return;
                }
            }
        }
    }
}
