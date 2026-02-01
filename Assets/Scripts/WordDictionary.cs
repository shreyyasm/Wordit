using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordDictionary : MonoBehaviour
{
    public static List<string> allWords = new List<string>();
    public static HashSet<string> usedWords = new HashSet<string>();

    void Awake()
    {
        LoadWords();
    }

    void LoadWords()
    {
        TextAsset file = Resources.Load<TextAsset>("words");
        string[] lines = file.text.Split('\n');

        foreach (var line in lines)
        {
            string w = line.Trim().ToUpper();

            // 🔥 THIS LINE IGNORES 2-LETTER WORDS
            if (w.Length >= 3 && w.Length <= 7 && w.All(char.IsLetter))
            {
                allWords.Add(w);
            }
        }

        Debug.Log("Loaded words: " + allWords.Count);
    }


    public static string GetWord(int length)
    {
        List<string> candidates = allWords.FindAll(
            w => w.Length == length && !usedWords.Contains(w)
        );

        if (candidates.Count == 0)
        {
            usedWords.Clear(); // safety fallback
            candidates = allWords.FindAll(w => w.Length == length);
        }

        string chosen = candidates[Random.Range(0, candidates.Count)];
        usedWords.Add(chosen);
        return chosen;
    }

    public static void ResetStreak()
    {
        usedWords.Clear();
    }
}
