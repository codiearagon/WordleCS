using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    public void ConnectToServer(TMP_InputField name)
    {
        if (string.IsNullOrEmpty(name.text))
        {
            Debug.Log("Insert name");
            return;
        }

        NetworkManager.network.ConnectToServer();
        PlayerManager.Instance.SetUsername(name.text);
        SceneManager.LoadScene("LobbyScene");
    }
}
