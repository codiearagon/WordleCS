using UnityEngine;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager Instance;
    public static Network network;

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
}
