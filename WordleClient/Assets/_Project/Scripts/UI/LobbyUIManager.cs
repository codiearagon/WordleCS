using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private GameObject statusArea;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text nameText;

    Coroutine statusCoroutine;

    private void OnEnable()
    {
        NetworkManager.OnJoinRoom += ProcessJoinStatus;
    }

    private void OnDisable()
    {
        NetworkManager.OnJoinRoom -= ProcessJoinStatus;
    }

    void Start()
    {
        nameText.text = PlayerManager.player.username;
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
        SceneManager.LoadScene("RoomScene");
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
