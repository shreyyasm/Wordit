using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public GameObject AddXPParent;
    public TextMeshProUGUI AddXPText;

    //XP Things
    public RectTransform imageRect;   // image to grow
    public Transform childParent;      // children source

    public float baseWidth = 120f;     // width at 0 children
    public float growthPerChild = 40f; // width added per child
    public float animDuration = 0.35f;


   
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TransitionManager.Instance.Open();
        FindAnyObjectByType<ColorSchemeManager>().ApplyRandomScheme();
        PlayerProgressionManager.ResetProgress();
        PlayerProgressionManager.ResetSessionXP();
        StartLevel();
     
      

        
    }
    public bool levelSolved = false;

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

            StartCoroutine(XPDelay());
            hud.UpdatePlayerLevel();
            hud.UpdateCoins();

            Debug.Log("Solved with: " + formed);
        }


    }

    IEnumerator XPDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // ✅ 1. ADD XP ONCE
        PlayerProgressionManager.AddXPForClearedLevel();

        // ✅ 2. SHOW POPUP
        AddXPParent.SetActive(true);

        // ✅ 3. READ THE REAL AWARDED XP
        int shown = 0;
        int target = PlayerProgressionManager.LastAwardedXP;
       
        // Safety
        DOTween.Kill(AddXPText);

        DOTween.To(() => shown, x =>
        {
            shown = x;
            AddXPText.text = $"+{shown} XP";
        }, target, 0.4f).SetEase(Ease.OutExpo);

        // ✅ 4. SCALE ANIMATION
        AddXPParent.transform.DOKill();
        AddXPParent.transform.localScale = new Vector3(1f, 0f, 1f);

        AddXPParent.transform
            .DOScaleY(1f, 0.35f)
            .SetEase(Ease.OutBack);
    }

    void StartLevel()
    {
        StartCoroutine(ChildCount());
        AddXPParent.SetActive(false);
        levelSolved = false;
        winTransition.DelaodStreak();
        winTransition.DeloadLose();
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
       
      
        LoseManager.instance.PlayLoseScreen();
       

    }
    public void PlayAgain()
    {
        LoseManager.instance.CloseScree(() =>
        {
            PlayerProgressionManager.ResetProgress();
            PlayerProgressionManager.ResetSessionXP();
            WordDictionary.ResetStreak();

            TransitionManager.Instance.Close(() =>
            {
                SceneManager.LoadScene(1);
            });
           
        });
       

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
    private int lastChildCount = 3;
    public void UpdateWidth()
    {
        int childCount = childParent.childCount;

        // 🚫 No change → no animation
        if (childCount == lastChildCount)
            return;
      
        lastChildCount = childCount;

        float targetWidth = imageRect.rect.width + 103;

        imageRect.DOKill();

        imageRect.DOSizeDelta(
            new Vector2(targetWidth, imageRect.sizeDelta.y),
            animDuration
        ).SetEase(Ease.OutBack);
    }
    IEnumerator ChildCount()
    {
        yield return new WaitForSeconds(1);
        UpdateWidth();
    }
}
