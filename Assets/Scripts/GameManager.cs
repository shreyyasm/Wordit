using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardGenerator board;
    public TimerManager timer;

    int level = 1;
    string currentWord;

    void Start()
    {
        FindAnyObjectByType<ColorSchemeManager>().ApplyRandomScheme();

        StartLevel();
    }
    bool levelSolved = false;

    void Update()
    {
        if (levelSolved) return;

        string formed = board.GetFormedWord(currentWord);


        if (WordDictionary.allWords.Contains(formed))
        {
            levelSolved = true;

            // 🔥 tell the board to switch center squares to solved color
            board.OnSolved();

            Invoke(nameof(NextLevel),1f);
            Debug.Log("Solved with: " + formed);
        }


    }



    void StartLevel()
    {
        levelSolved = false;

        int len = DifficultyManager.GetWordLength(level);
        currentWord = WordDictionary.GetWord(len);

        board.Generate(currentWord, level);
        timer.StartTimer(300);
        //timer.StartTimer(DifficultyManager.GetTimeLimit(level));
    }


    void NextLevel()
    {
        level++;
        StartLevel();
    }

    public void Lose()
    {
        level = 1;
        WordDictionary.ResetStreak();
        StartLevel();
    }
}
