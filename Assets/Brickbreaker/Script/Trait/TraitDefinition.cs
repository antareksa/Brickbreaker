using UnityEngine;

[System.Serializable]
public class TraitDefinition
{
    public TraitType Type;
    public string Name;
    [TextArea] public string Description;
    public Sprite Icon;

    // Parallel arrays, indexed by (currentLevel - 1) once a level is reached / (currentLevel)
    // when checking the cost to reach the next one. Length of both defines MaxLevel.
    public int[] CostPerLevel;
    public float[] ValuePerLevel;
    public ValueToken[] Tokens;

    public int MaxLevel => ValuePerLevel.Length;

    public string GetFormattedDescription(float levelValue, float? nextLevelValue = null)
    {
        string result = Description;
        foreach (var token in Tokens)
        {
            float displayValue = token.Multiplier * levelValue;
            string display = nextLevelValue.HasValue
                ? $"{displayValue}>{token.Multiplier * nextLevelValue.Value}"
                : displayValue.ToString();
            result = result.Replace($"<{token.Token}>", display);
        }
        return result;
    }
}

[System.Serializable]
public class ValueToken
{
    public string Token;      
    public float Multiplier = 1f;  
}
