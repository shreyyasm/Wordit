using UnityEngine;

public class GameWordManager : MonoBehaviour
{
    public static GameWordManager Instance;

    public int CurrentLevel => level;

    public HUDUI hud;
    public BoardGenerator board;
    public TimerManager timer;

    int level = 1;
    string currentWord;
    void Awake()
    {
        Instance = this;
    }

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
            PlayerProgressionManager.AddXPForClearedLevel(level);
            hud.UpdatePlayerLevel();

            Debug.Log("Solved with: " + formed);
        }


    }



    void StartLevel()
    {
        levelSolved = false;

        int len = DifficultyManager.GetWordLength(level);
        currentWord = WordDictionary.GetWord(len);

        board.Generate(currentWord, level);
        timer.StartTimer(PlayerProgressionManager.GetTimePerLevel());

        hud.UpdateStreak(level);
        hud.UpdatePlayerLevel();

        //timer.StartTimer(DifficultyManager.GetTimeLimit(level));
    }


    void NextLevel()
    {
        level++;
        StartLevel();
    }

    public void Lose()
    {
        hud.UpdateStreak(level);

        level = 1;
        WordDictionary.ResetStreak();
        StartLevel();

    }
}
