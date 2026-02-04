using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI streakText;

    void Start()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        UpdatePlayerLevel();
        UpdateStreak(GameWordManager.Instance.CurrentLevel);
    }

    public void UpdatePlayerLevel()
    {
        int playerLevel = PlayerProgressionManager.PlayerLevel;
        playerLevelText.text = $"PLAYER LVL {playerLevel}";
    }

    public void UpdateStreak(int streak)
    {
        streakText.text = $"STREAK {streak}";
    }
}
