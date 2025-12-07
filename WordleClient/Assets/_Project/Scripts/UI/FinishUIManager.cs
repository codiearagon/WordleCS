using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPlacePrefab;
    [SerializeField] private GameObject playerList;
    [SerializeField] private Button restartButton;

    private void OnEnable()
    {
        NetworkManager.OnReceivedResults += ProcessOnReceivedResults;
        NetworkManager.OnRestartGame += ProcessOnRestartGame;

        // There is a slight timing window where OnEnable will be slower than
        // the server's reply after NetworkManager requests for Results on joining.
        Queue<Player> lastUpdatedData = NetworkManager.Instance.GetResults();
        if (lastUpdatedData != null)
            ProcessOnReceivedResults(lastUpdatedData);
    }

    private void OnDisable()
    {
        NetworkManager.OnReceivedResults -= ProcessOnReceivedResults;
        NetworkManager.OnRestartGame -= ProcessOnRestartGame;
    }

    private void Start()
    {
        if (PlayerManager.player.userId == PlayerManager.currentRoom.hostId)
            restartButton.interactable = true;
    }

    void ProcessOnReceivedResults(Queue<Player> results) 
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            while (results.Count > 0)
            {
                Player player = results.Dequeue();
                GameObject newPlace = Instantiate(playerPlacePrefab, transform.position, Quaternion.identity, playerList.transform);

                if(player.guessCount <= 6 && player.guessCount >= 1)
                    newPlace.GetComponent<TMP_Text>().text = String.Format("{0}/6: {1}", player.guessCount, player.username);
                else
                    newPlace.GetComponent<TMP_Text>().text = String.Format("X/6: {0}", player.username);
            }
        });
    }

    void ProcessOnRestartGame()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            PlayerManager.player.finished = false;
            PlayerManager.player.SetGuessCount(0);

            foreach (Player player in PlayerManager.currentRoom.players)
            {
                player.finished = false;
                player.SetGuessCount(0);
            }

            SceneManager.LoadScene("RoomScene");
        });
    }

    public void Restart()
    {
        NetworkManager.Instance.RestartGame();
    }
    
    public void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }
}
