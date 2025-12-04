using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject tablePrefab;
    [SerializeField] private GameObject otherPlayers;
    [SerializeField] private WordleTable localTable;

    private void OnEnable()
    {
        NetworkManager.OnFinishedGame += ProcessFinishGame;
    }

    private void OnDisable()
    {
        NetworkManager.OnFinishedGame -= ProcessFinishGame;
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

    void ProcessFinishGame()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SceneManager.LoadScene("ResultsScene");
        });
    }
}
