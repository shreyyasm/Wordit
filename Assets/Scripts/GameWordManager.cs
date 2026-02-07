using UnityEngine;

public class GameWordManager : MonoBehaviour
{
    public static GameWordManager Instance;
    public WinTransition winTransition;
    const int HINT_COST = 500;
    const float HINT_TIME_BONUS = 10f;

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

            winTransition.Play(() =>
            {
                NextLevel();
            });
            PlayerProgressionManager.AddXPForClearedLevel(level);
            hud.UpdatePlayerLevel();
            hud.UpdateCoins();

            Debug.Log("Solved with: " + formed);
        }


    }



    void StartLevel()
    {
        levelSolved = false;
        winTransition.DelaodStreak();
        int len = DifficultyManager.GetWordLength(level);
        currentWord = WordDictionary.GetWord(len);

        board.Generate(currentWord, level);
        timer.StartTimer(PlayerProgressionManager.GetTimePerLevel());

        hud.UpdateStreak(level);
        hud.UpdatePlayerLevel();
        hud.HideHint();

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
    public void UseHint()
    {
        if (!PlayerCurrencyManager.CanAfford(HINT_COST))
        {
            Debug.Log("Not enough coins for hint");
            return;
        }

        bool spent = PlayerCurrencyManager.SpendCoins(HINT_COST);
        if (!spent)
            return;

        // show the word as hint
        hud.ShowHint(currentWord);

        // add temporary time bonus
        timer.AddTime(HINT_TIME_BONUS);

        // update coin UI
        hud.UpdateCoins();
    }
    public void AddCoins(int amount)
    {
        PlayerCurrencyManager.AddCoins(amount);
        hud.UpdateCoins();
    }


}
