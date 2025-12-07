using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WordleServer
{
    class Room
    {
        public string roomName { get; private set; }
        public int maxPlayers { get; private set; }
        public List<Player> players { get; private set; } = new List<Player>();
        public int hostId { get; private set; }
        public bool inGame { get; private set; }

        public string word { get; private set; }

        public List<Player> finishedPlayers { get; private set; } = new List<Player>();

        public Room(Player host, string roomName)
        {
            this.roomName = roomName;
            maxPlayers = 6;

            players.Add(host);
            host.SetRoom(this);
            hostId = host.userId;
            inGame = false;
            word = WordBank.GetRandomWord();

            Console.WriteLine("{0}({1}) successfully created {2} room", host.playerName, host.userId, roomName);
        }

        public void AddPlayer(Player player) 
        {
            players.Add(player);
            player.SetRoom(this);

            Console.WriteLine("{0}({1}) joined room {2}", player.playerName, player.userId, roomName);
        }

        public void RemovePlayer(Player player)
        {
            players.RemoveAll(p => p.userId == player.userId);

            Console.WriteLine("{0}({1}) left room {2}", player.playerName, player.userId, roomName);
        }

        public void SetWord(string word)
        {
            this.word = word;
        }

        public void SetInGame(bool value)
        {
            inGame = value;
        }

        public void AddResult(Player player)
        {
            finishedPlayers.Add(player);

            // sorting by guess count
            finishedPlayers.Sort((a, b) => a.guessCount.CompareTo(b.guessCount));
        }

        public void ResetResults()
        {
            finishedPlayers.Clear();
        }

        public void BroadcastMessage(string message)
        {
            foreach (Player player in players) 
            {
                player.SendMessage(message);
            }
        }
    }
}
