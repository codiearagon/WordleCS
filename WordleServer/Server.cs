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

        private static int userIdIncrement = 0;

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

            Player newPlayer = new Player(client, userIdIncrement);
            userIdIncrement++;

            try
            {
                string message;
                while (true)
                {
                    message = newPlayer.ReceiveString();
                    if (message == null)
                    {
                        newPlayer.LeaveRoom();
                        Console.WriteLine("Client disconnected cleanly.");
                        break;
                    }

                    HandleClientMessage(newPlayer, message);
                }
            }
            catch (SocketException)
            {
                newPlayer.LeaveRoom();
                Console.WriteLine("Client disconnected unexpectedly.");
            }
            finally
            {
                client.Close();
            }
        
        }

        private static void HandleClientMessage(Player player, string message)
        {
            string[] parts = message.Split(';');

            // first element is always the action, and following are arguments
            switch(parts[0])
            {
                case "set_username":
                    player.SetName(parts[1]);
                    break;
                case "create_room":
                    CreateRoom(player, parts[1]);
                    break;
                case "join_room":
                    JoinRoom(player, parts[1]);
                    break;
                case "leave_room":
                    LeaveRoom(player);
                    break;
                case "get_room_data":
                    GetRoomData(player);
                    break;
            }
        }

        private static void CreateRoom(Player host, string roomName)
        {
            foreach (Room room in rooms)
            {
                if (room.roomName == roomName)
                {
                    host.SendMessage("status;Room name already exists");
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
                    player.SendMessage("status;Successfully joined room.");
                    room.BroadcastMessage("room_changed");
                    return;
                }
            }
        }

        private static void LeaveRoom(Player player)
        {
            player.room.RemovePlayer(player);
            player.SendMessage("status;Successfully joined room.");
            player.room.BroadcastMessage("room_changed");
        }

        private static void GetRoomData(Player sender)
        {
            string message = "get_room_data;";
            foreach(Player player in sender.room.players) 
            {
                message += String.Format("{0};{1};", player.playerName, player.isReady);
            }

            message += String.Format("{0}", sender.room.hostId);

            sender.SendMessage(message);
        }
    }
}
