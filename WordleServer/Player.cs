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
        public int userId { get; private set; }
        public Room? room { get; private set; }
        public bool isReady { get; private set; }

        public Player(Socket socket, int userId)
        {
            this.socket = socket;
            this.userId = userId;
            playerName = "";
            isReady = false;
        }

        public void SetName(string name)
        {
            playerName = name;
        }

        public void SetRoom(Room room)
        {
            this.room = room;
        }

        public void SetReady(bool value)
        {
            isReady = value;
        }

        public void LeaveRoom()
        {
            room?.RemovePlayer(this);
        }

        public string ReceiveString()
        {
            byte[] dataLength = ReceiveAllData(4); // int byte size
            if (dataLength == null)
                return null;

            int length = BitConverter.ToInt32(dataLength, 0);

            byte[] message = ReceiveAllData(length);
            if (message == null)
                return null;

            return Encoding.UTF8.GetString(message);
        }

        public byte[] ReceiveAllData(int size)
        {
            byte[] buffer = new byte[size];
            
            int totalReceived = 0;
            while (totalReceived < size)
            {
                int bytesReceived = socket.Receive(buffer, totalReceived, size - totalReceived, SocketFlags.None);
                if (bytesReceived == 0)
                    return null;

                totalReceived += bytesReceived;
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
