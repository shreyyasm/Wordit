using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI playerXPText;
    public TextMeshProUGUI streakText;

    [Header("Coins")]
    public TextMeshProUGUI coinsText;

    [Header("Hint")]
    public TextMeshProUGUI hintText;

    void Start()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        UpdatePlayerLevel();
        UpdateStreak(GameWordManager.Instance.CurrentLevel);
        UpdateCoins();
        HideHint();
    }

    public void UpdatePlayerLevel()
    {
        int playerxp = PlayerProgressionManager.PlayerLevel;
        playerXPText.text = PlayerProgressionManager.PlayerBaseXP.ToString();
        DevvitBridge.Instance.SaveScore(PlayerProgressionManager.PlayerBaseXP);
    }

    public void UpdateStreak(int streak)
    {
        streakText.text = streak.ToString();
        // <sprite=0> assumes your coin sprite is index 0
        streakText.text = $"<sprite=0>{streak - 1}";
    }
    public void UpdateCoins()
    {
        // <sprite=0> assumes your coin sprite is index 0
        coinsText.text = $"<sprite=0> {PlayerCurrencyManager.Coins}";
    }

    public void ShowHint(string word)
    {
        hintText.text = $"HINT: {word}";
        hintText.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        hintText.gameObject.SetActive(false);
    }

}
