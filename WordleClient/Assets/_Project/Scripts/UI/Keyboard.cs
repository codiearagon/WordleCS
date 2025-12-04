using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private WordleTable wordleTable;
    private List<GameObject> wordRows = new List<GameObject>();

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
    }

    private void HandleLetterChecked(List<(string, LetterResult)> letterDict)
    {
        TMP_Text[] letters = GetComponentsInChildren<TMP_Text>();

        foreach ((string key, LetterResult lr) in letterDict)
        {
            foreach (TMP_Text l in letters) 
            {
                if(l.text == key)
                {
                    switch (lr)
                    {
                        case LetterResult.CORRECT:
                            l.transform.parent.GetComponent<Image>().color = Color.softGreen;
                            break;
                        case LetterResult.INCORRECT:
                            // don't change to incorrect if already corrrect or wrong pos
                            if (l.transform.parent.GetComponent<Image>().color != Color.softGreen ||
                                l.transform.parent.GetComponent<Image>().color != Color.softYellow)
                                l.transform.parent.GetComponent<Image>().color = Color.darkGray;
                            break;
                        case LetterResult.WRONG_POS:
                            // don't change to wrong pos if already marked correct
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
        if (currentWord.Length >= 5 || PlayerManager.player.finished)
            return;

        currentWord += letter;
        wordRows[rowPos].GetComponent<WordRow>().letters[letterPos].text = letter;
        letterPos++;
    }

    public void RemoveLetter()
    {
        if (currentWord.Length == 0 || PlayerManager.player.finished)
            return;

        letterPos--;
        currentWord = currentWord.Remove(currentWord.Length - 1);
        wordRows[rowPos].GetComponent<WordRow>().letters[letterPos].text = "";
    }

    public void Submit()
    {
        if (currentWord.Length < 5 || PlayerManager.player.finished)
            return;

         //if not word
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
    }
}
