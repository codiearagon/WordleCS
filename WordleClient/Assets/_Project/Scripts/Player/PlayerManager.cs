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
}
