using UnityEngine;

public static class PlayerCurrencyManager
{
    const string COINS_KEY = "PLAYER_COINS";

    public static int Coins
    {
        get => PlayerPrefs.GetInt(COINS_KEY, 0);
        private set => PlayerPrefs.SetInt(COINS_KEY, value);
    }

    public static bool CanAfford(int amount)
    {
        return Coins >= amount;
    }

    public static bool SpendCoins(int amount)
    {
        if (!CanAfford(amount))
            return false;

        Coins -= amount;
        PlayerPrefs.Save();
        return true;
    }

    public static void AddCoins(int amount)
    {
        Coins += amount;
        PlayerPrefs.Save();
    }

}
