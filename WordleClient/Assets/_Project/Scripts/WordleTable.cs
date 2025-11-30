using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public enum LetterResult
{
    CORRECT, WRONG_POS, INCORRECT
}

public class WordleTable : MonoBehaviour
{
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
            wordRows.Add(child.gameObject);
        }
    }

    private void ProcessOnMakeGuess(int userId, int guessCount, Dictionary<int, LetterResult> result)
    {
        // return if not local player
        if (userId != PlayerManager.player.userId)
            return;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            PlayerManager.player.SetGuessCount(guessCount);

            foreach (KeyValuePair<int, LetterResult> pair in result)
            {
                // set letter bg color of previous guess according to correctness
                if (pair.Value == LetterResult.CORRECT)
                    wordRows[guessCount - 1].GetComponent<WordRow>().letterBg[pair.Key].color = Color.darkGreen;
                else if (pair.Value == LetterResult.WRONG_POS)
                    wordRows[guessCount - 1].GetComponent<WordRow>().letterBg[pair.Key].color = Color.softYellow;
                else
                    wordRows[guessCount - 1].GetComponent<WordRow>().letterBg[pair.Key].color = Color.darkGray;
            }
        });
    }
}
