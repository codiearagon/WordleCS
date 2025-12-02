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

    void OnEnable()
    {
        NetworkManager.OnRoomDataUpdated += ProcessRoomData;
        NetworkManager.OnStartGame += ProcessStartGame;

        // There is a slight timing window where OnEnable will be slower than
        // the server's reply after NetworkManager requests for RoomData on joining.
        RoomData lastUpdatedData = NetworkManager.Instance.GetRoomData();
        if (lastUpdatedData != null)
            ProcessRoomData(lastUpdatedData);
    }

    void OnDisable()
    {
        NetworkManager.OnRoomDataUpdated -= ProcessRoomData;
        NetworkManager.OnStartGame -= ProcessStartGame;
    }

    void ProcessRoomData(RoomData roomData)
    {
        PlayerManager.currentRoom = roomData;

        // must be run on the main thread because
        // we're interacting with UI
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            roomNameText.text = "Room: " + roomData.roomName;
            hostIdText.text = "Host: " + roomData.hostId.ToString();

            // destroy all player listings and recreate them
            // simplest method
            foreach (Transform child in playerListObject.transform)
                Destroy(child.gameObject);

            foreach (Player player in roomData.players)
            {
                GameObject newPlayerList = Instantiate(playerListPrefab, Vector3.zero, Quaternion.identity, playerListObject.transform);
                TMP_Text[] texts = newPlayerList.GetComponentsInChildren<TMP_Text>();
                texts[0].text = player.username + "(" + player.userId + ")";
                texts[1].text = player.isReady ? "Ready" : "Not Ready";
                texts[1].color = player.isReady ? Color.darkGreen : Color.darkRed;

                // if current iter player is the client, set their local ready variable
                if (player.userId == PlayerManager.player.userId)
                    PlayerManager.player.isReady = player.isReady;
            }

            // --non host only--
            if (PlayerManager.player.userId != roomData.hostId)
            {
                startOrReadyButton.interactable = false;
                startOrReadyButton.GetComponentInChildren<TMP_Text>().text = PlayerManager.player.isReady ? "Unready" : "Ready";

                // start a cooldown for toggling ready again
                // to avoid flooding network with requests
                StartCoroutine(ReadyToggleCooldown());
            }

            // --for host only--
            // check if all players are ready and enable to start game
            if (PlayerManager.player.userId == roomData.hostId)
            {
                startOrReadyButton.GetComponentInChildren<TMP_Text>().text = "Start Game";

                foreach (Player player in roomData.players)
                {
                    if (!player.isReady)
                    {
                        startOrReadyButton.interactable = false;
                        return;
                    }
                }

                startOrReadyButton.interactable = true;
            }
        });
    }

    void ProcessStartGame()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }

    public void StartOrReady()
    {
        if (PlayerManager.player.userId == PlayerManager.currentRoom.hostId)
            NetworkManager.Instance.StartGame();
        else
            NetworkManager.Instance.ChangeReady();
    }

    IEnumerator ReadyToggleCooldown()
    {
        yield return new WaitForSeconds(2f);
        startOrReadyButton.interactable = true;
    }
}
