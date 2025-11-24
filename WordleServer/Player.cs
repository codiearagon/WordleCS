using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WordleServer
{
    class Player
    {
        public Socket socket { get; private set; }
        public string playerName { get; private set; }

        public Player(Socket socket)
        {
            this.socket = socket;
            playerName = "";
        }

        public void SetName(string name)
        {
            playerName = name;
        }

        public void SendInt(int num)
        {
            num = IPAddress.HostToNetworkOrder(num);
            byte[] data = BitConverter.GetBytes(num);
            int numSize = IPAddress.HostToNetworkOrder(sizeof(int));
            byte[] dataSize = BitConverter.GetBytes(numSize);

            SendAllData(dataSize);
            SendAllData(data);
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] dataSize = BitConverter.GetBytes(data.Length);

            SendAllData(dataSize);
            SendAllData(data);
        }

        // helper functions
        private void SendAllData(byte[] data)
        {
            int totalSent = 0;
            while (totalSent < data.Length)
            {
                int bytesSent = socket.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);
                if (bytesSent > 0)
                {
                    totalSent += bytesSent;
                    continue;
                }
            }
        }
    }
}
