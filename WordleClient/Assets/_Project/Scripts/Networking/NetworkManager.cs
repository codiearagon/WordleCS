using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    private static Network network;

    private List<RoomData> latestLobbyData;
    private RoomData latestRoomData;
    private Queue<Player> latestResults;

    public static event Action<string> OnUnexpected;
    public static event Action<List<RoomData>> OnLobbyChanged;
    public static event Action<string, string> OnCreateRoom;
    public static event Action<string, string> OnJoinRoom;
    public static event Action<RoomData> OnRoomDataUpdated;
    public static event Action<int> OnUserIdReceived;
    public static event Action OnStartGame;
    public static event Action<int, int, Dictionary<int, LetterResult>> OnMakeGuess;
    public static event Action OnFinishedGame;
    public static event Action<Queue<Player>> OnReceivedResults;
    public static event Action OnRestartGame;

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

    private void OnApplicationQuit()
    {
        network.CloseSocket();
    }

    private void HandleServerMessage(string message)
    {
        string[] parts = message.Split(';', StringSplitOptions.RemoveEmptyEntries);
        Debug.Log(String.Format("Received message: {0}", message));

        switch (parts[0])
        {
            case "get_user_id":
                HandleGetUserId(parts);
                break;
            case "lobby_changed":
                HandleLobbyChanged(parts);
                break;
            case "create_status":
                HandleCreateStatus(parts);
                break;
            case "join_status":
                HandleJoinStatus(parts);
                break;
            case "room_changed":
                HandleOnRoomChanged(parts);
                break;
            case "start_game":
                OnStartGame?.Invoke();
                break;
            case "make_guess":
                HandleOnMakeGuess(parts);
                break;
            case "game_finished":
                OnFinishedGame?.Invoke();
                break;
            case "results":
                HandleOnReceivedResults(parts);
                break;
            case "restart_game":
                OnRestartGame?.Invoke();
                break;
            case "disconnected":
                UnityMainThreadDispatcher.Instance.Enqueue(() => 
                {
                    Destroy(PlayerManager.Instance.gameObject);
                    Destroy(NetworkManager.Instance.gameObject);
                    SceneManager.LoadScene("StartScene"); 
                });
                break;
            default:
                OnUnexpected?.Invoke(""); // return to lobby scene, if in game
                LeaveRoom();
                OnUnexpected?.Invoke("Received unknown message from server"); // status message on lobby
                break;
        }
    }
    private void HandleGetUserId(string[] parts)
    {
        OnUserIdReceived?.Invoke(int.Parse(parts[1]));
    }

    private void HandleLobbyChanged(string[] parts)
    {
        List<RoomData> lobbyData = new List<RoomData>();
        int roomCount = int.Parse(parts[1]);

        for(int i = 0; i < roomCount; i++)
        {
            RoomData roomData = new RoomData();
            roomData.roomName = parts[i * 3 + 2];
            roomData.playerCount = int.Parse(parts[i * 3 + 3]);
            bool.TryParse(parts[i * 3 + 4], out roomData.inGame);

            lobbyData.Add(roomData);
        }

        latestLobbyData = lobbyData;
        OnLobbyChanged?.Invoke(lobbyData);
    }

    private void HandleCreateStatus(string[] parts)
    {
        OnCreateRoom?.Invoke(parts[1], parts[2]);
    }

    private void HandleJoinStatus(string[] parts)
    {
        OnJoinRoom?.Invoke(parts[1], parts[2]);
    }

    // Highly inefficient but should be fine for this project
    private void HandleOnRoomChanged(string[] parts)
    {
        RoomData roomData = new RoomData();
        roomData.roomName = parts[1];
        roomData.hostId = int.Parse(parts[2]);
        int playerCount = int.Parse(parts[3]);

        for (int i = 0; i < playerCount; i++)
        {
            Player newPlayer = new Player();
            newPlayer.SetUsername(parts[i * 3 + 4]);
            newPlayer.SetUserId(int.Parse(parts[i * 3 + 5]));
            bool.TryParse(parts[i * 3 + 6], out newPlayer.isReady);

            roomData.players.Add(newPlayer);
            roomData.playerCount = roomData.players.Count;
        }

        latestRoomData = roomData;
        OnRoomDataUpdated?.Invoke(latestRoomData);
    }

    private void HandleOnMakeGuess(string[] parts)
    {
        int userId = int.Parse(parts[1]);
        int guessCount = int.Parse(parts[2]);
        Dictionary<int, LetterResult> letterResults = new Dictionary<int, LetterResult>();

        // all words are 5 letters
        for(int i = 0; i < 5; i++)
        {
            if (parts[i * 2 + 4] == "correct")
                letterResults.Add(i, LetterResult.CORRECT);
            else if (parts[i * 2 + 4] == "incorrect")
                letterResults.Add(i, LetterResult.INCORRECT);
            else if (parts[i * 2 + 4] == "wrong_pos")
                letterResults.Add(i, LetterResult.WRONG_POS);
        }

        OnMakeGuess?.Invoke(userId, guessCount, letterResults);
    }

    private void HandleOnReceivedResults(string[] parts)
    {
        Queue<Player> results = new Queue<Player>();
        List<Player> roomPlayers = PlayerManager.currentRoom.players;

        for (int i = 0; i < roomPlayers.Count; i++)
        {
            int userId = int.Parse(parts[i + 1]);

            foreach (Player player in roomPlayers) 
            {
                if(player.userId == userId)
                    results.Enqueue(player);
            }
        }

        latestResults = results;
        OnReceivedResults?.Invoke(latestResults);
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
        latestRoomData = null;
    }

    public void GetUserId()
    {
        network.SendMessage("get_user_id");
    }

    public void ChangeReady()
    {
        network.SendMessage("change_ready");
    }

    public void StartGame()
    {
        network.SendMessage("start_game");
    }

    public void MakeGuess(string word)
    {
        network.SendMessage(String.Format("make_guess;{0}", word.ToLower()));
    }

    public void RestartGame()
    {
        network.SendMessage("restart_game");
    }

    public void Disconnect()
    {
        network.Disconnect();
    }

    public List<RoomData> GetLobbyData() => latestLobbyData;
    public RoomData GetRoomData() => latestRoomData;
    public Queue<Player> GetResults() => latestResults;
}
