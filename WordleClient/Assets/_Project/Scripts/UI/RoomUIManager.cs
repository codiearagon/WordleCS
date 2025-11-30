using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerListObject;
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text hostIdText;
    [SerializeField] private Button startOrReadyButton;

    private RoomData roomData;

    void OnEnable()
    {
        NetworkManager.OnRoomDataUpdated += ProcessRoomData;

        // There is a slight timing window where OnEnable will be slower than
        // the server's reply after NetworkManager requests for RoomData on joining.
        RoomData lastUpdatedData = NetworkManager.Instance.GetRoomData();
        if (lastUpdatedData != null)
            ProcessRoomData(lastUpdatedData);
    }

    void OnDisable()
    {
        NetworkManager.OnRoomDataUpdated -= ProcessRoomData;
    }

    void ProcessRoomData(RoomData roomData)
    {
        this.roomData = roomData;

        // must be run on the main thread because
        // we're interacting with UI
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            roomNameText.text = "Room: " + roomData.roomName;
            hostIdText.text = "Host: " + roomData.hostId.ToString();

            if (PlayerManager.player.userId == roomData.hostId)
            {
                startOrReadyButton.interactable = false;
                startOrReadyButton.GetComponentInChildren<TMP_Text>().text = "Start Game";
            }
            else
            {
                startOrReadyButton.interactable = true;
                startOrReadyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            }

            foreach (Transform child in playerListObject.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Player player in roomData.players)
            {
                GameObject newPlayerList = Instantiate(playerListPrefab, Vector3.zero, Quaternion.identity, playerListObject.transform);
                TMP_Text[] texts = newPlayerList.GetComponentsInChildren<TMP_Text>();
                texts[0].text = player.username + "(" + player.userId + ")";
                texts[1].text = player.isReady ? "Ready" : "Not Ready";
            }
        });
    }

    public void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }

    public void StartOrReady()
    {
        
    }
}
