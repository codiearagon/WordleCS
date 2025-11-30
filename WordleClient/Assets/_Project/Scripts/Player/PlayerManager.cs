using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager Instance;
    public static Player player;

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        player = new Player();
    }

    private void OnEnable()
    {
        NetworkManager.OnUserIdReceived += HandleUserId;
    }

    private void OnDisable()
    {
        NetworkManager.OnUserIdReceived -= HandleUserId;
    }

    private void HandleUserId(int userId)
    {
        player.SetUserId(userId);
        Debug.Log("Received ID: " + userId);
    }
}
