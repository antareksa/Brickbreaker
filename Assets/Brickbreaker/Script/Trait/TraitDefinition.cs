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

    public int MaxLevel => ValuePerLevel.Length;
}
