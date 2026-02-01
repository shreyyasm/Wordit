using UnityEngine;

public static class DifficultyManager
{
    public static int minColumnHeight = 2;
    public static int maxColumnHeight = 6;
    // controls how height grows inside a tier (0 → 1)
    public static AnimationCurve columnHeightCurve =
        AnimationCurve.Linear(0, 0, 1, 1);

    // ===== TUNING VARIABLES =====
    public static int baseWordLength = 3;
    public static int levelsPerTier = 4;     // every 4 levels difficulty increases
    public static int maxWordLength = 7;

    public static int baseMinHeight = 2;
    public static int baseMaxHeight = 3;


    public static float baseTime = 20f;
    public static float timeDecayPerLevel = 0.8f;
    public static float minTime = 7f;

    // ===== DERIVED DIFFICULTY =====
    public static int GetTier(int level)
    {
        return (level - 1) / levelsPerTier;
    }

    public static int GetWordLength(int level)
    {
        int len = baseWordLength + GetTier(level);
        return Mathf.Min(len, maxWordLength);
    }

    public static int GetTierProgress(int level)
    {
        return (level - 1) % levelsPerTier;
    }

    public static float GetTierProgress01(int level)
    {
        return GetTierProgress(level) / (float)(levelsPerTier - 1);
    }

    public static int GetMinColumnHeight(int level)
    {
        // always reset at the start of each tier
        return minColumnHeight;
    }

    public static int GetMaxColumnHeight(int level)
    {
        float t = GetTierProgress01(level);

        int extra =
            Mathf.RoundToInt(
                columnHeightCurve.Evaluate(t) *
                (maxColumnHeight - minColumnHeight)
            );

        return minColumnHeight + extra;
    }

    public static float GetTimeLimit(int level)
    {
        float t = baseTime - (level - 1) * timeDecayPerLevel;
        return Mathf.Max(t, minTime);
    }
    public static int GetLockedColumnCount(int wordLength)
    {
        if (wordLength < 4)
            return 0;

        // 4 → 1, 6 → 2, 8 → 3, 10 → 4
        return Mathf.Clamp(
            1 + (wordLength - 4) / 2,
            0,
            wordLength - 2 // never lock all columns
        );
    }

}
