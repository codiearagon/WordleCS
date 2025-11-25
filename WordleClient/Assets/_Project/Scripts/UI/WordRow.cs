using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordRow : MonoBehaviour
{
    public List<TMP_Text> letters = new List<TMP_Text>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Transform child in transform)
        {
            letters.Add(child.GetComponentInChildren<TMP_Text>());
        }
    }
}
