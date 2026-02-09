using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject columnPrefab;
    public Transform boardParent;
    public GameObject lockedColumnPrefab;


    private LetterColumn[] columns;

    public void Generate(string word, int level)
    {
        currentWord = word;
        do
        {
            foreach (Transform c in boardParent)
                Destroy(c.gameObject);

            int minH = DifficultyManager.GetMinColumnHeight(level);
            int maxH = DifficultyManager.GetMaxColumnHeight(level);

            int lockedCount =
            DifficultyManager.GetLockedColumnCount(word.Length);

            // choose unique locked indices
            HashSet<int> lockedIndices = new HashSet<int>();

            while (lockedIndices.Count < lockedCount)
            {
                int index = Random.Range(0, word.Length);

                // avoid first & last column for UX safety
                if (index == 0 || index == word.Length - 1)
                    continue;

                lockedIndices.Add(index);
            }


            columns = new LetterColumn[word.Length];

            for (int i = 0; i < word.Length; i++)
            {
                if (lockedIndices.Contains(i))
                {
                    GameObject col = Instantiate(lockedColumnPrefab, boardParent);
                    LockedColumn lc = col.GetComponent<LockedColumn>();
                    lc.Init(word[i]);

                    columns[i] = null; // IMPORTANT: this slot is fixed
                }
                else
                {
                    int height = Random.Range(minH, maxH + 1);
                    GameObject col = Instantiate(columnPrefab, boardParent);
                    LetterColumn lc = col.GetComponent<LetterColumn>();
                    lc.Init(word[i], height);
                    columns[i] = lc;
                }
            }

        } while (IsAnyValidWordSolved() || IsInvalidStartState());


      

    }
    bool IsSolvedAtStart(string targetWord)
    {
        if (columns == null || columns.Length == 0)
            return false;

        for (int i = 0; i < columns.Length; i++)
        {
            // locked column: always contributes correct letter
            if (columns[i] == null)
                continue;

            // if ANY draggable column does NOT have
            // the correct letter in the center, we are safe
            if (columns[i].GetCenterLetter() != targetWord[i])
                return false;
        }

        // all columns match target → solved at start (BAD)
        return true;
    }

    public void OnSolved()
    {
        if (columns == null) return;

        foreach (Transform child in boardParent)
        {
            LetterColumn lc = child.GetComponent<LetterColumn>();
            if (lc != null)
            {
                lc.SetSolvedState();
                lc.PlaySolvedSqueeze();
                continue;
            }

            LockedColumn locked = child.GetComponent<LockedColumn>();
            if (locked != null)
            {
                locked.SetSolvedState();
                locked.PlaySolvedSqueeze();
            }
        }
    }


    public string GetFormedWord(string targetWord)
    {
        if (columns == null || columns.Length == 0)
            return "";

        string formed = "";

        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] != null)
                formed += columns[i].GetCenterLetter();
            else
                formed += targetWord[i]; // 🔒 locked column
        }

        return formed;
    }


   
    public bool IsAnyValidWordSolved()
    {
        if (columns == null || columns.Length == 0)
            return false;

        string formed = "";

        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] != null)
                formed += columns[i].GetCenterLetter();
            else
                formed += currentWord[i]; // locked
        }

        return WordDictionary.allWords.Contains(formed);
    }
    bool HasAtLeastOneValidWord()
    {
        return CheckColumn(0, "");
    }
    string currentWord;

    bool CheckColumn(int index, string current)
    {
        if (index == columns.Length)
            return WordDictionary.allWords.Contains(current);

        // 🔒 LOCKED COLUMN
        if (columns[index] == null)
        {
            // use the fixed letter from the target word
            return CheckColumn(index + 1, current + currentWord[index]);
        }

        // 🔄 NORMAL COLUMN
        foreach (char c in columns[index].GetAllLetters())
        {
            if (CheckColumn(index + 1, current + c))
                return true; // early exit
        }

        return false;
    }
    public string GetCurrentCenterWord()
    {
        if (columns == null || columns.Length == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i] != null)
                sb.Append(columns[i].GetCenterLetter());
            else
                sb.Append(currentWord[i]); // 🔒 locked column
        }

        return sb.ToString();
    }
    bool IsInvalidStartState()
    {
        string formed = GetCurrentCenterWord();

        if (formed.Length < 3)
            return false;

        return WordDictionary.allWords.Contains(formed);
    }


}
