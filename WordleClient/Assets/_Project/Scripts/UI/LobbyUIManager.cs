using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private GameObject roomListingPrefab;
    [SerializeField] private GameObject roomListingParent;
    [SerializeField] private GameObject statusArea;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text nameText;

    Coroutine statusCoroutine;

    private void OnEnable()
    {
        NetworkManager.OnLobbyChanged += ProcessLobbyChanged;
        NetworkManager.OnCreateRoom += ProcessCreateStatus;
        NetworkManager.OnJoinRoom += ProcessJoinStatus;
        NetworkManager.OnUnexpected += ProcessOnUnexpected;

        // There is a slight timing window where OnEnable will be slower than
        // the server's reply after NetworkManager requests for LobbyData on joining.
        List<RoomData> lastUpdatedData = NetworkManager.Instance.GetLobbyData();
        if (lastUpdatedData != null)
            ProcessLobbyChanged(lastUpdatedData);
    }

    private void OnDisable()
    {
        NetworkManager.OnLobbyChanged -= ProcessLobbyChanged;
        NetworkManager.OnCreateRoom -= ProcessCreateStatus;
        NetworkManager.OnJoinRoom -= ProcessJoinStatus;
        NetworkManager.OnUnexpected -= ProcessOnUnexpected;
    }

    void Start()
    {
        nameText.text = PlayerManager.player.username;
    }

    private void ProcessLobbyChanged(List<RoomData> lobbyData)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            foreach(Transform child in roomListingParent.transform)
                Destroy(child.gameObject);

            foreach (RoomData roomData in lobbyData)
            {
                GameObject roomListing = Instantiate(roomListingPrefab, Vector3.zero, Quaternion.identity, roomListingParent.transform);
                roomListing.GetComponentInChildren<TMP_Text>().text = String.Format("{0};({1}/6)", roomData.roomName, roomData.playerCount);

                // listing still shows but cannot be pressed if full or in game
                if (roomData.playerCount >= 6)
                {
                    roomListing.GetComponentInChildren<TMP_Text>().text += ";(FULL)";
                    roomListing.GetComponent<Button>().interactable = false;
                }
                else if (roomData.inGame)
                {
                    roomListing.GetComponent<Button>().interactable = false;
                    roomListing.GetComponentInChildren<TMP_Text>().text += ";(IN GAME)";
                }

            }
        });
    }

    private void ProcessCreateStatus(string status, string reason)
    {
        if (status == "failed")
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                ShowStatus("Failed to create: " + reason);
                createButton.interactable = true;
            });
        } 
        else
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                createButton.interactable = true;
                SceneManager.LoadScene("RoomScene");
            });
        }
    }

    private void ProcessJoinStatus(string status, string reason)
    {
        if (status == "failed")
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                ShowStatus("Failed to join: " + reason);
                joinButton.interactable = true;
            });
        }
        else
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                joinButton.interactable = true;
                SceneManager.LoadScene("RoomScene");
            });
        }

    }

    private void ProcessOnUnexpected(string message)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            ShowStatus(message);
        });
    }

    public void CreateRoom(TMP_InputField roomName)
    {
        if (string.IsNullOrEmpty(roomName.text))
        {
            ShowStatus("Room name cannot be empty");
            return;
        } else if (roomName.text.Contains(';'))
        {
            ShowStatus("Room name cannot contain ;");
            return;
        }

        NetworkManager.Instance.CreateRoom(roomName.text);
        createButton.interactable = false;
    }

    public void JoinRoom(TMP_InputField roomName)
    {
        if (string.IsNullOrEmpty(roomName.text))
        {
            ShowStatus("Room name cannot be empty");
            return;
        }
        else if (roomName.text.Contains(';'))
        {
            ShowStatus("Room name cannot contain ;");
            return;
        }

        NetworkManager.Instance.JoinRoom(roomName.text);
        joinButton.interactable = false;
    }

    private void ShowStatus(string text)
    {
        if (statusCoroutine != null)
            StopCoroutine(statusCoroutine);

        statusCoroutine = StartCoroutine(ShowStatusBriefly(text));
    }

    IEnumerator ShowStatusBriefly(string text)
    {
        statusArea.GetComponentInChildren<TMP_Text>().text = text;
        statusArea.SetActive(true);
        yield return new WaitForSeconds(2f);
        statusArea.SetActive(false);
    }
}
