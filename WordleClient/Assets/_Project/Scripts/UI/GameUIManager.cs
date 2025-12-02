using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject tablePrefab;
    [SerializeField] private GameObject otherPlayers;
    [SerializeField] private WordleTable localTable;

    private void OnEnable()
    {
        NetworkManager.OnGameStateChanged += ProcessGameStateChanged;

        RoomData lastUpdatedData = NetworkManager.Instance.GetRoomData();
        if (lastUpdatedData != null)
            ProcessGameStateChanged(lastUpdatedData);
    }

    private void OnDisable()
    {
        NetworkManager.OnGameStateChanged -= ProcessGameStateChanged;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localTable.SetPlayer(PlayerManager.player);

        foreach(Player p in PlayerManager.currentRoom.players)
        {
            if(p.userId != PlayerManager.player.userId)
            {
                GameObject newTable = Instantiate(tablePrefab, Vector3.zero, Quaternion.identity, otherPlayers.transform);
                newTable.GetComponent<WordleTable>().SetPlayer(p);
            }
        }

        NetworkManager.Instance.GameLoaded();
    }

    private void ProcessGameStateChanged(RoomData gameData)
    {

    }
}
