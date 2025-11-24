using UnityEngine;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
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

    public void ConnectToServer(TMP_InputField name)
    {
        if (string.IsNullOrEmpty(name.text))
        {
            Debug.Log("Insert name");
            return;
        }

        network.ConnectToServer();
        network.SetUsername(name.text);
    }
}
