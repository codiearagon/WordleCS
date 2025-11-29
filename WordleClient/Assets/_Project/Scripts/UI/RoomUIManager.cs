using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RoomUIManager : MonoBehaviour
{

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
        Debug.Log("Joined room: " + roomData.roomName);
    }

}
