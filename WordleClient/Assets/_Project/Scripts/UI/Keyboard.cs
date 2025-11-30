using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private WordleTable wordleTable;
    private List<GameObject> wordRows = new List<GameObject>();

    private bool canType;
    private int rowPos;
    private int letterPos;
    private string currentWord;

    private void OnEnable()
    {
        wordleTable.OnLetterChecked += HandleLetterChecked;
    }

    private void OnDisable()
    {
        wordleTable.OnLetterChecked -= HandleLetterChecked;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in wordleTable.transform)
        {
            wordRows.Add(child.gameObject);
        }

        rowPos = 0;
        letterPos = 0;
        currentWord = "";
        canType = true;
    }

    private void HandleLetterChecked(Dictionary<string, LetterResult> letterDict)
    {
        TMP_Text[] letters = GetComponentsInChildren<TMP_Text>();

        foreach (KeyValuePair<string, LetterResult> pair in letterDict)
        {
            foreach (TMP_Text l in letters) 
            {
                if(l.text == pair.Key)
                {
                    switch (pair.Value)
                    {
                        case LetterResult.CORRECT:
                            l.transform.parent.GetComponent<Image>().color = Color.softGreen;
                            break;
                        case LetterResult.INCORRECT:
                            l.transform.parent.GetComponent<Image>().color = Color.darkGray;
                            break;
                        case LetterResult.WRONG_POS:
                            // don't change color if already marked correct
                            if (l.transform.parent.GetComponent<Image>().color != Color.softGreen)
                                l.transform.parent.GetComponent<Image>().color = Color.softYellow;
                            break;
                    }
                }
            }
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

        // if not word
        if (!WordBank.IsWord(currentWord.ToLower()))
        {
            Debug.Log(currentWord + " is not a word in the bank.");
            return;
        }
        
        letterPos = 0;
        rowPos++;

        Debug.Log("Submitted word: " + currentWord);
        NetworkManager.Instance.MakeGuess(currentWord); // send word to server

        currentWord = "";

        if (PlayerManager.player.guessCount >= 6)
            canType = false;
    }
}
