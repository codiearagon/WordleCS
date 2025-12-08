using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    [SerializeField] private GameObject statusArea;
    [SerializeField] private TMP_InputField addressField;
    [SerializeField] private TMP_InputField nameField;
    Coroutine statusCoroutine;

    public void ConnectToServer()
    {
        if (string.IsNullOrEmpty(nameField.text))
        {
            ShowStatus("Name cannot be empty.");
            return;
        }
        else if (nameField.text.Contains(';'))
        {
            ShowStatus("Name cannot contain ;");
            return;
        }

        try
        {
            NetworkManager.Instance.ConnectToServer(addressField.text);
        }
        catch
        {
            ShowStatus("Failed to connect to server, server may be closed or address is wrong");
            return;
        }

        PlayerManager.player.SetUsername(nameField.text);
        NetworkManager.Instance.SetUsername(nameField.text);
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
