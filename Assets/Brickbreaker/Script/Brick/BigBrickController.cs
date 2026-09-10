using UnityEngine;
using UnityEngine.UI;

public class BigBrickController : BrickController
{
    public Slider HPSlider;

    public override void Spawn(int hitPoint, Vector2Int gridPosition)
    {
        base.Spawn(hitPoint, gridPosition);

        HPSlider.maxValue = hitPoint;
        HPSlider.value = hitPoint;
    }

    public override void DamageBrick(int damage)
    {
        base.DamageBrick(damage);

        HPSlider.value = _hitPoint;
    }
}