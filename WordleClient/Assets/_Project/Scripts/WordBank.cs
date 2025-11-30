using System;
using System.IO;
using UnityEngine;

public class WordBank : MonoBehaviour
{
    static TextAsset wordFile = Resources.Load<TextAsset>("words");
    static string[] words = wordFile.text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
    public static bool IsWord(string word)
    {
        foreach(string w in words)
        {
            if (w == word)
                return true;
        }

        return false;
    }
}
