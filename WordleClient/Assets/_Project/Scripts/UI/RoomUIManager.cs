using System.Collections;
using UnityEngine;

public class RoomUIManager : MonoBehaviour
{
    RoomData roomData = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(RoomDataUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RoomDataUpdate()
    {
        while (true)
        {
            Debug.Log("Requesting for room udpate...");
            NetworkManager.network.GetRoomData(data => {
                roomData = data;
            });
            yield return new WaitForSeconds(1f);
        }
    }

}
