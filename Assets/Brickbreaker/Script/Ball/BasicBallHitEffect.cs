public class BasicBallHitEffect : BaseBallHitEffect
{
    public override void OnHitBrick(BrickController brickController)
    {
        DealDamage(brickController);
    }
}
