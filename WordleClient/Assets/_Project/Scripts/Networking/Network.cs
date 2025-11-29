using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Network
{
    private const string ADDRESS = "127.0.0.1";
    private const int PORT = 11020;

    private Socket clientSocket = new Socket
        (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    private Action<RoomData> pendingRoomDataCallback;
    
    public void ConnectToServer()
    {
        clientSocket.Connect(ADDRESS, PORT);
 	    Debug.Log("Connected to Server");
        Thread thread = new Thread(ReceivingLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    public void ReceivingLoop()
    {
        try
        {
            while (true)
            {
                string message = ReceiveString();
                if (message == null)
                {
                    Debug.Log("Client disconnected.");
                    break;
                }

                HandleServerMessage(message);
            }
        } 
        catch (SocketException)
        {
            Debug.LogError("Disconnected unexpectedly");
        }
    }

    public void HandleServerMessage(string message)
    {
        string[] parts = message.Split(';');
        Debug.Log(String.Format("Received message: {0}", parts[0]));

        switch (parts[0]) 
        {
            case "get_room_data":
                RoomData roomData = new RoomData();
                roomData.roomName = parts[1];
                roomData.hostId = int.Parse(parts[2]);
                int playerCount = int.Parse(parts[3]);

                for(int i = 0; i < playerCount; i++)
                {
                    Player newPlayer = new Player();
                    newPlayer.SetUsername(parts[i + 4]);
                    newPlayer.userId = int.Parse(parts[i + 5]);
                    newPlayer.isReady = bool.Parse(parts[i + 6]);

                    roomData.players.Add(newPlayer);
                }

                pendingRoomDataCallback?.Invoke(roomData);

                pendingRoomDataCallback = null;

                break;
            default:
                Debug.Log("Unrecognized message");
                break;
        }

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

    public void LeaveRoom()
    {
        SendMessage("leave_room");
    }

    public void GetRoomData(Action<RoomData> callback)
    {
        pendingRoomDataCallback = callback;
        SendMessage("get_room_data");
    }

    //-----------------------------socket helper functions below--------------------------------
    private string ReceiveString()
    {
        byte[] dataLength = ReceiveAllData(4);
        if(dataLength == null)
            return null;

        int length = BitConverter.ToInt32(dataLength, 0);

        byte[] message = ReceiveAllData(length);
        if(message == null) 
            return null;

        return Encoding.UTF8.GetString(message);
    }

    private byte[] ReceiveAllData(int size)
    {
        byte[] buffer = new byte[size];

        int totalReceived = 0;
        while (totalReceived < size)
        {
            int bytesReceived = clientSocket.Receive(buffer, totalReceived, size - totalReceived, SocketFlags.None);
            if (bytesReceived == 0)
                return null;

            totalReceived += bytesReceived;
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
