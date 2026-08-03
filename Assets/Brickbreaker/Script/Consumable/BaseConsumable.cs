using UnityEngine;

// One SO type for every Consumable -- no per-effect subclasses here, same split as BasePowerUp.
// The actual one-time behavior lives in a separate BaseConsumableEffect asset, referenced by Effect.
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Brickbreaker/Consumable")]
public class BaseConsumable : ScriptableObject
{
    public Sprite ConsumableImage;
    public string ConsumableName;
    [TextArea] public string Description;

    public BaseConsumableEffect Effect;

    // Flat base cost -- no edition/rarity modifier, same as BasePowerUp.
    public int BuyCost = 3;
}
