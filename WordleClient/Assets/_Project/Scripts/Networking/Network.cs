using UnityEngine;
using System.Net.Sockets;
using System;

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

        clientSocket.Close();
    }
}
