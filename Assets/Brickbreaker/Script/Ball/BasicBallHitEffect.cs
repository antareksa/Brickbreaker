public class BasicBallHitEffect : BaseBallHitEffect
{
    protected override BallEnhanceType EnhanceType => BallEnhanceType.Basic;

    protected override void ResolveHit(BrickController brickController)
    {
        DealDamage(brickController);
    }
}
