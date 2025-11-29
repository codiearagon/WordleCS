using UnityEngine;
using TMPro;
using System;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    private static Network network;

    private RoomData latestRoomData;

    public static event Action<RoomData> OnRoomDataUpdated;

    void Awake()
    {
        if(Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        network = new Network();
    }

    private void OnEnable()
    {
        network.OnMessageReceived += HandleServerMessage;
    }

    private void OnDisable()
    {
        network.OnMessageReceived -= HandleServerMessage;
    }

    private void HandleServerMessage(string message)
    {
        string[] parts = message.Split(';');
        Debug.Log(String.Format("Received message: {0}", parts[0]));

        switch (parts[0])
        {
            case "room_changed":
                HandleOnRoomChanged(message);
                break;
            default:
                Debug.Log("Unrecognized message");
                break;
        }

    }

    private void HandleOnRoomChanged(string message)
    {
        string[] parts = message.Split(';');

        RoomData roomData = new RoomData();
        roomData.roomName = parts[1];
        roomData.hostId = int.Parse(parts[2]);
        int playerCount = int.Parse(parts[3]);

        for (int i = 0; i < playerCount; i++)
        {
            Player newPlayer = new Player();
            newPlayer.SetUsername(parts[i + 4]);
            newPlayer.userId = int.Parse(parts[i + 5]);
            newPlayer.isReady = bool.Parse(parts[i + 6]);

            roomData.players.Add(newPlayer);
        }

        latestRoomData = roomData;
        OnRoomDataUpdated?.Invoke(roomData);
    }

    public void ConnectToServer()
    {
        network.ConnectToServer();
    }

    public void SetUsername(string name)
    {
        network.SendMessage(String.Format("set_username;{0}", name));
    }

    public void CreateRoom(string roomName)
    {
        network.SendMessage(String.Format("create_room;{0}", roomName));
    }

    public void JoinRoom(string roomName)
    {
        network.SendMessage(String.Format("join_room;{0}", roomName));
    }

    public void LeaveRoom()
    {
        network.SendMessage("leave_room");
    }

    public RoomData GetRoomData() => latestRoomData;
}
