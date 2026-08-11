using UnityEngine;

// #11: Bricks do not descend this wave (skips the row-shift for one AdvanceWave cycle).
[CreateAssetMenu(fileName = "SkipDescendConsumableEffect", menuName = "Brickbreaker/Consumable Effect/Skip Descend")]
public class SkipDescendConsumableEffect : BaseConsumableEffect
{
    public override void Use()
    {
        GameManager.Instance.BrickManager.SkipNextDescend();
    }

    public override string GetDescription() => "Bricks do not descend this wave";
}
