using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    [SerializeField] private GameObject statusArea;
    Coroutine statusCoroutine;

    public void ConnectToServer(TMP_InputField name)
    {
        if (string.IsNullOrEmpty(name.text))
        {
            ShowStatus("Name cannot be empty.");
            return;
        }
        else if (name.text.Contains(';'))
        {
            ShowStatus("Name cannot contain ;");
            return;
        }

        try
        {
            NetworkManager.Instance.ConnectToServer();
        }
        catch
        {
            ShowStatus("Failed to connect to server");
            return;
        }

        PlayerManager.player.SetUsername(name.text);
        NetworkManager.Instance.SetUsername(name.text);
        NetworkManager.Instance.GetUserId(); // user id is server generated
        SceneManager.LoadScene("LobbyScene");
    }

    private void ShowStatus(string text)
    {
        if (statusCoroutine != null)
            StopCoroutine(statusCoroutine);

        statusCoroutine = StartCoroutine(ShowStatusBriefly(text));
    }

    IEnumerator ShowStatusBriefly(string text)
    {
        statusArea.GetComponentInChildren<TMP_Text>().text = text;
        statusArea.SetActive(true);
        yield return new WaitForSeconds(2f);
        statusArea.SetActive(false);
    }
}
