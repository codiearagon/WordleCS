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
                        Console.WriteLine("Client disconnected cleanly.");
                        break;
                    }

                    HandleClientMessage(newPlayer, message);
                }
            }
            catch (SocketException)
            {
                
                Console.WriteLine("Client disconnected unexpectedly.");
            }
            finally
            {
                client.Close();
                LeaveRoom(newPlayer);
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
                case "get_user_id":
                    player.SendMessage("get_user_id;" + player.userId);
                    break;
                case "change_ready":
                    player.SetReady(!player.isReady); // toggle ready

                    if(player.room != null)
                        RoomChanged(player.room); 
                    break;
                case "start_game":
                    player.room?.BroadcastMessage("start_game");
                    break;
                case "on_game_loaded":
                    OnGameLoaded(player);
                    break;
                case "make_guess":
                    MakeGuess(player, parts[1]);
                    break;
                default:
                    Console.WriteLine("Unrecognized message");
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
            host.SetReady(true); // host will always be ready

            RoomChanged(newRoom);
        }

        private static void JoinRoom(Player player, string roomName)
        {
            foreach (Room room in rooms)
            {
                if (room.roomName == roomName)
                {
                    room.AddPlayer(player);
                    player.SendMessage("status;Successfully joined room.");
                    RoomChanged(player.room);
                    return;
                }
            }
        }

        private static void LeaveRoom(Player player)
        {
            if(player.room == null)
                return;

            player.room.RemovePlayer(player);

            // Only send a status message to the client if not leaving in an unexpected dropped connection
            if(player.socket.Connected)
                player.SendMessage("status;Successfully left room.");

            RoomChanged(player.room);

            if (player.room.players.Count <= 0)
            {
                rooms.RemoveAll(r => r.roomName == player.room.roomName);
                Console.WriteLine("{0} room disbanded, no players left.", player.room.roomName);
            }
        }

        private static void RoomChanged(Room room)
        {
            string message = String.Format("room_changed;{0};{1};{2};", room.roomName, room.hostId, room.players.Count);

            for(int i = 0; i < room.players.Count; i++)
            {
                Player player = room.players[i];
                message += String.Format("{0};{1};{2};", player.playerName, player.userId, player.isReady);
            }

            room.BroadcastMessage(message);
        }

        // this function is to ensure no one can start typing until everybody loads in
        private static void OnGameLoaded(Player player)
        {
            if (player.room == null)
                return;

            player.room.BroadcastMessage(String.Format("on_game_loaded"));
        }

        private static void MakeGuess(Player player, string guessWord)
        {
            if (player.room == null)
                return;

            player.SetGuessCount(player.guessCount + 1);

            // add player result to room, if lost or won
            if(guessWord == player.room.word || player.guessCount >= 6)
            {
                player.room.AddResult(player.userId, player.guessCount);
            }

            string message = String.Format("make_guess;{0};{1};", player.userId, player.guessCount);

            Dictionary<char, int> actualWordCount = new Dictionary<char, int>();
            Dictionary<int, string> correctness = new Dictionary<int, string>(); 
            bool[] passed = new bool[guessWord.Length];

            // check correct and incorrect letters
            for (int i = 0; i < guessWord.Length; i++)
            {

                if (player.room.word.Contains(guessWord[i]))
                {
                    // mark as correct letter if in correct pos
                    if (player.room.word[i] == guessWord[i])
                    {
                        passed[i] = true;
                        correctness[i] = "correct";
                    }
                    else
                    {
                        if (!actualWordCount.ContainsKey(player.room.word[i]))
                            actualWordCount[player.room.word[i]] = 0;
                        actualWordCount[player.room.word[i]]++;
                        correctness[i] = "";
                    }
                }
                else
                {
                    if (!actualWordCount.ContainsKey(player.room.word[i]))
                        actualWordCount[player.room.word[i]] = 0;
                    actualWordCount[player.room.word[i]]++;
                    passed[i] = true;
                    correctness[i] = "incorrect";
                }
            }   

            // mark correct letters but wrong position
            for (int i = 0; i < guessWord.Length; i++)
            {
                if (passed[i])
                    continue;

                if (actualWordCount.ContainsKey(guessWord[i]) && actualWordCount[guessWord[i]] > 0)
                {
                    actualWordCount[guessWord[i]]--;
                    correctness[i] = "wrong_pos";
                }
                else
                    correctness[i] = "incorrect";
            }

            // create final message
            foreach(KeyValuePair<int, string> pair in correctness)
                message += String.Format("{0};{1};", pair.Key, pair.Value);

            player.room.BroadcastMessage(message);
        }
    }
}
