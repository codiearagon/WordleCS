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
        public List<Player> players { get; private set; } = new List<Player>();
        public int hostId { get; private set; }

        public Room(Player host, string roomName)
        {
            this.roomName = roomName;

            players.Add(host);
            host.SetRoom(this);
            hostId = host.userId;

            Console.WriteLine("Successfully created {0} room", roomName);
        }

        public void AddPlayer(Player player) 
        {
            players.Add(player);
            player.SetRoom(this);

            Console.WriteLine("{0} joined room {1}", player.userId, roomName);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);

            Console.WriteLine("{0} left room {1}", player.userId, roomName);
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
