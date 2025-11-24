using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace WordleServer
{
    class Room
    {
        public string roomName { get; private set; }
        public List<Player> players { get; private set; } = new List<Player>();

        public Room(Player host, string roomName)
        {
            this.roomName = roomName;
            players.Add(host);
        }

        public void AddPlayer(Player player) 
        {
            players.Add(player);
        }
    }
}
