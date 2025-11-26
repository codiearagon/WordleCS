using UnityEngine;

public class RoomUIManager : MonoBehaviour
{
    RoomData roomData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.network.GetRoomData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
