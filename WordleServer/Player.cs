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

        public string ReceiveString()
        {
            byte[] dataLength = ReceiveAllData(4);
            int length = BitConverter.ToInt32(dataLength, 0);

            byte[] message = ReceiveAllData(length);

            return Encoding.UTF8.GetString(message);
        }

        public byte[] ReceiveAllData(int size)
        {
            byte[] buffer = new byte[size];
            
            int totalReceived = 0;
            while (totalReceived < size)
            {
                int bytesReceived = socket.Receive(buffer, totalReceived, size - totalReceived, SocketFlags.None);
                if (bytesReceived > 0)
                {
                    totalReceived += bytesReceived;
                    continue;
                }
            }

            return buffer;
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

                Console.WriteLine("Sent 0 bytes, connection could be lost.");
                return;
            }
        }
    }
}
