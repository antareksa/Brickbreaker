using UnityEngine;

// One SO type for every PowerUp -- no per-effect subclasses here. The actual behavior lives in
// a separate BasePowerUpEffect asset (its own polymorphic hierarchy), referenced by Effect.
[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Brickbreaker/PowerUp")]
public class BasePowerUp : ScriptableObject
{
    public Sprite PowerUpImage;
    public string PowerUpName;
    [TextArea] public string Description;

    public BasePowerUpEffect Effect;

    // Flat base cost -- no edition/rarity modifier yet (see Shop doc section 5.2).
    public int BuyCost = 3;

    // Prefers the effect's generated text (which carries its real numbers) over the hand-typed
    // Description, so retuning a value in the Inspector updates every place this is shown.
    // Description is the fallback for effects that have no numbers worth generating.
    public string GetDescription()
    {
        string generated = Effect != null ? Effect.GetDescription() : string.Empty;
        return string.IsNullOrEmpty(generated) ? Description : generated;
    }
}
