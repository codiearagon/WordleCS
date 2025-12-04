using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
public enum LetterResult
{
    CORRECT, WRONG_POS, INCORRECT
}

public class WordleTable : MonoBehaviour
{
    public event Action<List<(string key, LetterResult lr)>> OnLetterChecked;

    public Player player {  get; private set; }
    private TMP_Text playerName;
    private List<GameObject> wordRows = new List<GameObject>();

    private void OnEnable()
    {
        NetworkManager.OnMakeGuess += ProcessOnMakeGuess;
    }

    private void OnDisable()
    {
        NetworkManager.OnMakeGuess -= ProcessOnMakeGuess;
    }

    // Start is called once before the first execution `of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            if(child.GetComponent<WordRow>() != null)
                wordRows.Add(child.gameObject);
        }
    }

    private void ProcessOnMakeGuess(int userId, int guessCount, Dictionary<int, LetterResult> result)
    {
        if (userId != player.userId)
            return;

        List<(string, LetterResult)> letterDict = new List<(string, LetterResult)>();

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            // set guess count for this object's player and room's player object
            player.SetGuessCount(guessCount);
            foreach(Player p in PlayerManager.currentRoom.players)
            {
                if(p.userId == userId)
                    p.SetGuessCount(guessCount);
            }

            // if all letteres are correct or out of guesses, mark as finished
            player.finished = result.Values.All(b => b == LetterResult.CORRECT) || guessCount >= 6;

            // this part is simply so below code will work
            // since 7 will break the code
            if (guessCount > 6)
                guessCount = 6;

            foreach (KeyValuePair<int, LetterResult> pair in result)
            {
                WordRow row = wordRows[guessCount - 1].GetComponent<WordRow>();

                // set letter bg color of previous guess according to correctness
                if (pair.Value == LetterResult.CORRECT)
                {
                    row.letterBg[pair.Key].color = Color.softGreen;
                    letterDict.Add((row.letters[pair.Key].text, LetterResult.CORRECT));
                }
                else if (pair.Value == LetterResult.WRONG_POS)
                {
                    row.letterBg[pair.Key].color = Color.softYellow;
                    letterDict.Add((row.letters[pair.Key].text, LetterResult.WRONG_POS));
                }
                else
                {
                    row.letterBg[pair.Key].color = Color.darkGray;
                    letterDict.Add((row.letters[pair.Key].text, LetterResult.INCORRECT));
                }
            }

            // only player keyboard will be listening to this
            OnLetterChecked?.Invoke(letterDict);
        });
    }

    public void SetPlayer(Player player)
    {
        this.player = player;

        if(player.userId != PlayerManager.player.userId)
        {
            playerName = GetComponentInChildren<TMP_Text>();
            playerName.text = player.username;
        }
    }
}