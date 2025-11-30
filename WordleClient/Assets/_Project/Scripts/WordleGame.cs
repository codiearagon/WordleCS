using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public enum LetterResult
{
    CORRECT, WRONG_POS, INCORRECT
}

public class WordleGame : MonoBehaviour
{
    private List<GameObject> wordRows = new List<GameObject>();
    private List<GameObject> rowLetters = new List<GameObject>();

    private bool canType;
    private int rowPos;
    private int letterPos;
    private string currentWord;
    private string lastGuessWord;

    private void OnEnable()
    {
        NetworkManager.OnMakeGuess += ProcessOnMakeGuess;
    }

    private void OnDisable()
    {
        NetworkManager.OnMakeGuess -= ProcessOnMakeGuess;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            wordRows.Add(child.gameObject);
        }

        rowPos = 0;
        letterPos = 0;
        currentWord = "";
        lastGuessWord = "";
        canType = true;
    }

    private void ProcessOnMakeGuess(Dictionary<int, LetterResult> result)
    {
        foreach(KeyValuePair<int, LetterResult> pair in result)
        {
            // set letter bg color of previous guess according to correctness
            if (pair.Value == LetterResult.CORRECT)
                wordRows[rowPos - 1].GetComponent<WordRow>().letterBg[pair.Key].tintColor = Color.darkGreen;
            else if(pair.Value == LetterResult.WRONG_POS)
                wordRows[rowPos - 1].GetComponent<WordRow>().letterBg[pair.Key].tintColor = Color.softYellow;
            else
                wordRows[rowPos - 1].GetComponent<WordRow>().letterBg[pair.Key].tintColor = Color.darkGray;
        }
    }

    public void AddLetter(string letter)
    {
        if (currentWord.Length >= 5 || !canType)
            return;

        currentWord += letter;
        wordRows[rowPos].GetComponent<WordRow>().letters[letterPos].text = letter;
        letterPos++;
    }

    public void RemoveLetter() 
    {
        if (currentWord.Length == 0 || !canType)
            return;

        letterPos--;
        currentWord = currentWord.Remove(currentWord.Length - 1);
        wordRows[rowPos].GetComponent<WordRow>().letters[letterPos].text = "";
    }

    public void Submit()
    {
        if (currentWord.Length < 5 || !canType)
            return;

        letterPos = 0;
        rowPos++;

        Debug.Log("Submitted word: " + currentWord);
        NetworkManager.Instance.MakeGuess(currentWord); // send word to server
        
        lastGuessWord = currentWord;
        currentWord = "";

        if (PlayerManager.player.guessCount >= 6)
            canType = false;
    }
}
