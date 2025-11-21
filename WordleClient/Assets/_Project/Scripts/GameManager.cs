using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Network network;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        network = new Network();
    }

    void Start()
    {
        network.ConnectToServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
