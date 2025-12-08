using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WordleServer
{
    class Server
    {
        private const int PORT = 11020;

        private static Socket serverSock = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static List<Room> rooms = new List<Room>();
        private static List<Player> players = new List<Player>();

        private static int userIdIncrement = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter Address: ");
            string address = Console.ReadLine();

            if (address == null)
            {
                Console.WriteLine("Address is null");
                return;
            }

            CreateServer(address);
        }

        private static void CreateServer(string address)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(address), PORT);

            Console.WriteLine("Creating server...");
            serverSock.Bind(localEndPoint);

            serverSock.Listen(10);
            Console.WriteLine("Listening to {0}:{1}", address, PORT);

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
            players.Add(newPlayer);
            userIdIncrement++;

            newPlayer.SendMessage(GetLobbyData()); // as soon as player connects, send lobby data

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
                players.RemoveAll(p => p.userId == newPlayer.userId);
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
                    player.room?.SetInGame(true);
                    player.room?.BroadcastMessage("start_game");
                    LobbyChanged();
                    break;
                case "make_guess":
                    MakeGuess(player, parts[1]);
                    break;
                case "restart_game":
                    RestartGame(player.room);
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
                    host.SendMessage("create_status;failed;room already exists");
                    return;
                }
            }

            Room newRoom = new Room(host, roomName);
            rooms.Add(newRoom);
            host.SetReady(true); // host will always be ready
            host.SendMessage("create_status;success;created");

            RoomChanged(newRoom);
            LobbyChanged(); // update players in lobby with new room
        }

        private static void JoinRoom(Player player, string roomName)
        {
            foreach (Room room in rooms)
            {
                if (room.roomName == roomName)
                {
                    if (room.players.Count == room.maxPlayers)
                        player.SendMessage("join_status;failed;room is full");
                    else if (room.inGame)
                        player.SendMessage("join_status;failed;room is in game");
                    else 
                    {
                        room.AddPlayer(player);
                        player.SendMessage("join_status;success;room exists");

                        RoomChanged(player.room);
                        LobbyChanged(); // update players in lobby with room player count
                    }

                    return;
                }
            }

            player.SendMessage("join_status;failed;room doesn't exist");
        }

        private static void LeaveRoom(Player player)
        {
            if(player.room == null)
                return;

            player.SetReady(false);
            player.SetGuessCount(0);
            player.room.RemovePlayer(player);

            RoomChanged(player.room);

            // if player leaves in middle of game and everybody else is done, finish the game
            if(player.room.inGame && player.room.finishedPlayers.Count == player.room.players.Count)
                CheckResults(player.room);

            if (player.room.players.Count <= 0)
            {
                rooms.RemoveAll(r => r.roomName == player.room.roomName);
                Console.WriteLine("{0} room disbanded, no players left.", player.room.roomName);
            }

            player.SetRoom(null);
            LobbyChanged(); // update players in lobby with room player count
        }

        private static void MakeGuess(Player player, string guessWord)
        {
            if (player.room == null)
            {
                player.SendMessage("room_null");
                return;
            }

            player.SetGuessCount(player.guessCount + 1);

            // add player result to room, if lost or won
            if (guessWord == player.room.word || player.guessCount >= 6)
            {
                // if couldn't guess the word at last guess, make it 7 guesses to indicate loss
                if (guessWord != player.room.word)
                    player.SetGuessCount(player.guessCount + 1);

                player.room.AddResult(player);
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

            // everybody is done when finished count is equal to player count
            if (player.room.finishedPlayers.Count == player.room.players.Count)
                CheckResults(player.room);
        }

        private static void CheckResults(Room room)
        {
            // let the clients transition scenes first
            room.BroadcastMessage("game_finished"); 

            string message = "results;";

            foreach (Player p in room.finishedPlayers) 
                message += String.Format("{0};", p.userId);

            room.BroadcastMessage(message);
        }

        private static void RestartGame(Room room)
        {
            if (room == null)
                return;

            room.ResetResults();
            room.SetWord(WordBank.GetRandomWord());
            room.SetInGame(false);

            foreach(Player p in room.players)
            {
                if (p.userId != room.hostId)
                    p.SetReady(false);

                p.SetGuessCount(0);
            }

            room.BroadcastMessage("restart_game");
            RoomChanged(room);
            LobbyChanged();
        }

        private static void LobbyChanged()
        {
            string lobbyData = GetLobbyData();

            foreach(Player p in players)
            {
                if(p.room == null) // only send to players in lobby (they are not in a room)
                    p.SendMessage(lobbyData);
            }
        }

        private static void RoomChanged(Room room)
        {
            string message = String.Format("room_changed;{0};{1};{2};", room.roomName, room.hostId, room.players.Count);

            for (int i = 0; i < room.players.Count; i++)
            {
                Player player = room.players[i];
                message += String.Format("{0};{1};{2};", player.playerName, player.userId, player.isReady);
            }

            room.BroadcastMessage(message);
        }

        private static string GetLobbyData()
        {
            string data = String.Format("lobby_changed;{0};", rooms.Count);

            foreach(Room r in rooms)
            {
                data += String.Format("{0};{1};{2};", r.roomName, r.players.Count, r.inGame);
            }

            return data;
        }
    }
}
