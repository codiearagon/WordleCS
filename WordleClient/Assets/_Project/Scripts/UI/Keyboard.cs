using System.Collections.Generic;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private GameObject wordleTable;
    private List<GameObject> wordRows = new List<GameObject>();

    private bool canType;
    private int rowPos;
    private int letterPos;
    private string currentWord;

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

        currentWord = "";

        if (PlayerManager.player.guessCount >= 6)
            canType = false;
    }
}
