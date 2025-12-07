using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    TMP_Text listText;
    string roomName;

    private void Awake()
    {
        listText = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        roomName = listText.text.Split(';')[0];
    }

    public void JoinRoom()
    {
        NetworkManager.Instance.JoinRoom(roomName);
    }
}
