using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Network
{
    private const string ADDRESS = "127.0.0.1";
    private const int PORT = 11020;

    private Socket clientSocket = new Socket
        (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    public void ConnectToServer()
    {
        clientSocket.Connect(ADDRESS, PORT);
 	    Debug.Log("Connected to Server");
    }

    public void SetUsername(string name)
    {
        SendMessage(String.Format("set_username;{0}", name));
    }

    public void CreateRoom(string roomName)
    {
        SendMessage(String.Format("create_room;{0}", roomName));
    }

    public void JoinRoom(string roomName)
    {
        SendMessage(String.Format("join_room;{0}", roomName));
    }

    //-----------------------------socket helper functions below--------------------------------
    private string ReceiveString()
    {
        byte[] dataLength = ReceiveAllData(4);
        int length = BitConverter.ToInt32(dataLength, 0);

        byte[] message = ReceiveAllData(length);

        return Encoding.UTF8.GetString(message);
    }

    private byte[] ReceiveAllData(int size)
    {
        byte[] buffer = new byte[size];

        int totalReceived = 0;
        while (totalReceived < size)
        {
            int bytesReceived = clientSocket.Receive(buffer, totalReceived, size - totalReceived, SocketFlags.None);
            if (bytesReceived > 0)
            {
                totalReceived += bytesReceived;
                continue;
            }
        }

        return buffer;
    }

    private void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        byte[] dataSize = BitConverter.GetBytes(data.Length);

        SendAllData(dataSize);
        SendAllData(data);
    }

    private void SendAllData(byte[] data)
    {
        int totalSent = 0;
        while (totalSent < data.Length)
        {
            int bytesSent = clientSocket.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);
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
