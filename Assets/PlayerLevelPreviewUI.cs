using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevelPreviewUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI rewardText;
    public Button leftButton;
    public Button rightButton;

    int viewedLevel;
    int playerLevel;

    public TextMeshProUGUI levelTextMenu;
    public Slider xpSliderMenu;

    public GameObject Tick;

    void Start()
    {
        playerLevel = PlayerProgressionManager.PlayerLevel;
        viewedLevel = playerLevel; // 🔹 start from player level

        levelTextMenu.text = $"Level {PlayerProgressionManager.PlayerLevel} -->";

        int xpRequired = XPToNextLevel(playerLevel);

        xpSliderMenu.minValue = 0;
        xpSliderMenu.maxValue = xpRequired;

        int totalXP = PlayerProgressionManager.PlayerBaseXP;

        int xpSpent = GetTotalXPRequiredBeforeLevel(playerLevel);

        int currentXP = totalXP - xpSpent;
        Debug.Log(xpSpent);
        currentXP = Mathf.Clamp(currentXP, 0, xpRequired);

        xpSliderMenu.value = currentXP;

        leftButton.onClick.AddListener(ViewPreviousLevel);
        rightButton.onClick.AddListener(ViewNextLevel);

       
    }

    void ViewPreviousLevel()
    {
        if (viewedLevel > 1)
        {
            viewedLevel--;
            RefreshUI();
        }
    }

    void ViewNextLevel()
    {
        viewedLevel++;
        RefreshUI();
    }

    void RefreshUI()
    {
        levelText.text = $"Level {viewedLevel}";
       
        int xpRequired = XPToNextLevel(viewedLevel);

        xpSlider.minValue = 0;
        xpSlider.maxValue = xpRequired;

        if (viewedLevel == playerLevel)
        {
            int totalXP = PlayerProgressionManager.PlayerBaseXP;

            int xpSpent = GetTotalXPRequiredBeforeLevel(playerLevel);

            int currentXP = totalXP - xpSpent;
  
            currentXP = Mathf.Clamp(currentXP, 0, xpRequired);

            xpSlider.value = currentXP;
            xpText.text = $"({currentXP}/{xpRequired})";
            Tick.SetActive(false);
        }
        // ===== COMPLETED LEVELS (LEFT SIDE) =====
        else if (viewedLevel < playerLevel)
        {
            xpSlider.value = xpRequired; // 🔹 full bar
            xpText.text = xpRequired.ToString();
            Tick.SetActive(true);
        }
        // ===== FUTURE LEVELS (RIGHT SIDE) =====
        else
        {
            xpSlider.value = 0;
            xpText.text = xpRequired.ToString();
            Tick.SetActive(false);
        }

        // ===== REWARD TEXT =====
        int nextTime = BASE_TIME + viewedLevel * TIME_PER_LEVEL;
        rewardText.text = $"Timer +{TIME_PER_LEVEL}s ---> {nextTime}s";

        // ===== BUTTON STATES =====
        leftButton.interactable = viewedLevel > 1;
    }
    int GetTotalXPRequiredBeforeLevel(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++)
        {
            total += XPToNextLevel(i);
        }
        return total;
    }
    // ===== MIRROR LOGIC =====
    const int BASE_TIME = 10;
    const int TIME_PER_LEVEL = 2;
    int GetXPSpentBeforeLevel(int level)
    {
        int total = 0;

        for (int i = 1; i < level; i++)
        {
            total += XPToNextLevel(i);
        }

        return total;
    }
    int XPToNextLevel(int level)
    {
        return (level + 1) * 100;
    }
    public GameObject LevelPanel;
    public void OpenPlayerLevel()
    {
        RefreshUI();
        LevelPanel.SetActive(true);
    }
    public void ClosePlayerLevel()
    {
        LevelPanel.SetActive(false);
    }
}