using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<GameObject> wordRows = new List<GameObject>();
    private List<GameObject> rowLetters = new List<GameObject>();

    private bool canType;
    private int guessCount;
    private int letterPos;
    private string currentWord;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            wordRows.Add(child.gameObject);
        }

        guessCount = 0;
        letterPos = 0;
        currentWord = "";
        canType = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLetter(string letter)
    {
        if (currentWord.Length >= 5 || !canType)
            return;

        currentWord += letter;
        wordRows[guessCount].GetComponent<WordRow>().letters[letterPos].text = letter;
        letterPos++;

        Debug.Log(currentWord);
    }

    public void RemoveLetter() 
    {
        if (currentWord.Length == 0 || !canType)
            return;

        letterPos--;
        currentWord = currentWord.Remove(currentWord.Length - 1);
        wordRows[guessCount].GetComponent<WordRow>().letters[letterPos].text = "";

        Debug.Log(currentWord);
    }

    public void Submit()
    {
        if (currentWord.Length < 5)
            return;

        Debug.Log("Submitted word");
        guessCount++;
        currentWord = "";
        letterPos = 0;

        if (guessCount >= 6)
            canType = false;
    }
}
