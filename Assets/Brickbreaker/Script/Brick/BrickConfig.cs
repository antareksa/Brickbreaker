using UnityEngine;

public static class BrickConfig
{
    // 2 bricks (waves 1-3) -> 3 (4-15) -> 4 (16-54) -> 5 every 5th wave from 55+
    public static int GetBricksSpawned(int wave)
    {
        if (wave <= 3) return 2;
        if (wave <= 15) return 3;
        if (wave >= 55 && wave % 5 == 0) return 5;
        return 4;
    }

    // Quadratic fit to the hand-tuned HP curve, floored at 1
    public static int GetBaseHP(int wave)
    {
        float hp = 0.043f * wave * wave + 2.18f * wave - 3.8f;
        return Mathf.Max(1, Mathf.RoundToInt(hp));
    }

    // Gold brick on waves where wave % 10 is 3, 6, or 9
    public static bool IsGoldWave(int wave)
    {
        int m = wave % 10;
        return m == 3 || m == 6 || m == 9;
    }

    // Tank brick unlocks in 3 tiers: mod5==0 from wave 5, mod5==3 from wave 33, mod5==2 from wave 52
    public static bool IsTankWave(int wave)
    {
        int m = wave % 5;
        if (m == 0 && wave >= 5) return true;
        if (m == 3 && wave >= 33) return true;
        if (m == 2 && wave >= 52) return true;
        return false;
    }

    // 2 tanks on 5-brick waves, otherwise 1 (0 if not a tank wave)
    public static int GetTankCount(int wave)
    {
        if (!IsTankWave(wave)) return 0;
        return GetBricksSpawned(wave) == 5 ? 2 : 1;
    }

    // Tank HP is always exactly 2x that wave's base HP
    public static int GetTankHP(int wave)
    {
        return GetBaseHP(wave) * 2;
    }
}
