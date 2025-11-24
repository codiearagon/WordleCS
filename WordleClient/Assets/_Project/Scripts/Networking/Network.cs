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

    // helper functions
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
