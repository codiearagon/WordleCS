using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    public void ConnectToServer(TMP_InputField name)
    {
        if (string.IsNullOrEmpty(name.text))
        {
            Debug.Log("Name cannot be empty.");
            return;
        }
        else if (name.text.Contains(';'))
        {
            Debug.Log("Name cannot contain ;");
            return;
        }

        NetworkManager.Instance.ConnectToServer();
        PlayerManager.player.SetUsername(name.text);
        NetworkManager.Instance.GetUserId();
        SceneManager.LoadScene("LobbyScene");
    }
}
