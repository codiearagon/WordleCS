using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordRow : MonoBehaviour
{
    public List<TMP_Text> letters = new List<TMP_Text>();
    public List<Image> letterBg = new List<Image>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Transform child in transform)
        {
            letters.Add(child.GetComponentInChildren<TMP_Text>());
        }

        foreach(Transform child in transform)
        {
            letterBg.Add(child.GetComponentInChildren<Image>());
        }
    }
}
