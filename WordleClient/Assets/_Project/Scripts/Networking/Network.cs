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

    public event Action<string> OnMessageReceived;
    
    public void ConnectToServer()
    {
        clientSocket.Connect(ADDRESS, PORT);
 	    Debug.Log("Connected to Server");

        Thread thread = new Thread(ReceivingLoop);
        thread.IsBackground = true;
        thread.Start();
    }

    public void CloseConnection()
    {
        clientSocket.Close();
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

                OnMessageReceived?.Invoke(message);
            }
        } 
        catch (SocketException)
        {
            Debug.LogError("Disconnected unexpectedly");
        }
        finally
        {
            clientSocket.Close();
        }
    }

    public void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        byte[] dataSize = BitConverter.GetBytes(data.Length);

        SendAllData(dataSize);
        SendAllData(data);
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
