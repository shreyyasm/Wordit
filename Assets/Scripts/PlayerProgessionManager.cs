using UnityEngine;

public static class PlayerProgressionManager
{
    // ===== CONSTANTS =====
    const string LEVEL_KEY = "PLAYER_LEVEL";
    const string XP_KEY = "PLAYER_XP";
    const int LEVEL_UP_COIN_REWARD = 100;


    const int BASE_TIME = 10;
    const int TIME_PER_LEVEL = 2;

    public const int BASE_XP_PER_LEVEL = 10;

    const string BASE_XP_KEY = "PLAYER_BASE_XP";
    const int BASE_XP = 0;

    const string LAST_GAINED_XP_KEY = "LAST_GAINED_XP";

    // ===== SESSION XP =====
    static int sessionXP = 0;
    public static int SessionXP => sessionXP;
    // XP curve (simple linear for now, infinite-safe)
    static int XPToNextLevel(int level)
    {
        // Level 1 → 200
        // Level 2 → 300
        // Level 3 → 400
        return (level + 1) * 100;
    }


    // ===== PLAYER LEVEL =====
    public static int PlayerLevel
    {
        get => PlayerPrefs.GetInt(LEVEL_KEY, 1);
        private set => PlayerPrefs.SetInt(LEVEL_KEY, value);
    }

    // ===== PLAYER XP =====
    static int PlayerXP
    {
        get => PlayerPrefs.GetInt(XP_KEY, 0);
        set => PlayerPrefs.SetInt(XP_KEY, value);
    }
    public static int PlayerBaseXP
    {
        get => PlayerPrefs.GetInt(BASE_XP_KEY, 0);
        set => PlayerPrefs.SetInt(BASE_XP_KEY, value);
    }
    // ===== TIME PER GAME LEVEL =====
    public static int GetTimePerLevel()
    {
        return BASE_TIME + (PlayerLevel - 1) * TIME_PER_LEVEL;
    }
    static int LastGainedXP
    {
        get => PlayerPrefs.GetInt(LAST_GAINED_XP_KEY, 0);
        set => PlayerPrefs.SetInt(LAST_GAINED_XP_KEY, value);
    }
    public static int LastAwardedXP => LastGainedXP;
    // ===== XP GAIN =====
    public static void AddXPForClearedLevel()
    {
        int gainedXP;

        if (LastGainedXP == 0)
        {
            gainedXP = BASE_XP_PER_LEVEL;
        }
        else
        {
            gainedXP = LastGainedXP * 2;
        }

        LastGainedXP = gainedXP;

        // 🔹 GLOBAL XP
        PlayerXP += gainedXP;
        PlayerBaseXP += gainedXP;

        // 🔹 SESSION XP
        sessionXP += gainedXP;

        CheckLevelUp();
    }

    // ===== SESSION CONTROL =====
    public static void ResetSessionXP()
    {
        sessionXP = 0;
        LastGainedXP = 0; // optional but recommended
    }
    static void CheckLevelUp()
    {
        if (PlayerXP < XPToNextLevel(PlayerLevel))
            return;

        // LEVEL UP
        PlayerLevel++;

        // 🎁 reward coins on EVEN levels
        if (PlayerLevel % 2 == 0)
        {
            PlayerCurrencyManager.AddCoins(100);
        }

        // 🔁 RESET XP (NO CARRY OVER)
        PlayerXP = 0;

        PlayerPrefs.Save();
    }


    // ===== DEBUG / RESET =====
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LEVEL_KEY);
        PlayerPrefs.DeleteKey(XP_KEY);
        PlayerPrefs.DeleteKey("LAST_GAINED_XP");
    }
}
