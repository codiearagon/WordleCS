using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nameText.text = PlayerManager.Instance.username;
    }

    public void CreateRoom(TMP_InputField roomName)
    {
        if (string.IsNullOrEmpty(roomName.text))
        {
            Debug.Log("Room name cannot be empty.");
            return;
        } else if (roomName.text.Contains(';'))
        {
            Debug.Log("Room name cannot contain ;");
            return;
        }

        NetworkManager.network.CreateRoom(roomName.text);
    }

    public void JoinRoom(TMP_InputField roomName)
    {
        if (string.IsNullOrEmpty(roomName.text))
        {
            Debug.Log("Empty room name");
            return;
        }
        else if (roomName.text.Contains(';'))
        {
            Debug.Log("Room name cannot contain ;");
            return;
        }

        NetworkManager.network.JoinRoom(roomName.text);
    }
}
