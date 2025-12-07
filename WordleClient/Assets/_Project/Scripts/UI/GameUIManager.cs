using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject tablePrefab;
    [SerializeField] private GameObject otherPlayers;
    [SerializeField] private WordleTable localTable;

    private void OnEnable()
    {
        NetworkManager.OnFinishedGame += ProcessFinishGame;
        NetworkManager.OnRoomDataUpdated += ProcessRoomData;
        NetworkManager.OnUnexpected += ProcessOnUnexpected;
    }

    private void OnDisable()
    {
        NetworkManager.OnFinishedGame -= ProcessFinishGame;
        NetworkManager.OnRoomDataUpdated -= ProcessRoomData;
        NetworkManager.OnUnexpected -= ProcessOnUnexpected;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventSystem.current.sendNavigationEvents = false;
        localTable.SetPlayer(PlayerManager.player);

        foreach(Player p in PlayerManager.currentRoom.players)
        {
            if(p.userId != PlayerManager.player.userId)
            {
                GameObject newTable = Instantiate(tablePrefab, Vector3.zero, Quaternion.identity, otherPlayers.transform);
                newTable.GetComponent<WordleTable>().SetPlayer(p);
            }
        }
    }

    void ProcessRoomData(RoomData roomData)
    {
        PlayerManager.currentRoom = roomData;

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            // destroy table of players no longer in room
            foreach (Transform child in otherPlayers.transform)
            {
                if (!roomData.players.Any(p => p.userId == child.GetComponent<WordleTable>().player.userId))
                {
                    Destroy(child.gameObject);
                }
            }
        });
    }

    void ProcessOnUnexpected(string message)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SceneManager.LoadScene("LobbyScene");
        });
    }

    void ProcessFinishGame()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SceneManager.LoadScene("ResultsScene");
        });
    }

    public void LeaveRoom()
    {
        PlayerManager.player.SetGuessCount(0);
        PlayerManager.player.finished = false;

        NetworkManager.Instance.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }
}
