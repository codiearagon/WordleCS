using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public string username { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetUsername(string username)
    {
        this.username = username;
        NetworkManager.network.SetUsername(username);
    }
}
