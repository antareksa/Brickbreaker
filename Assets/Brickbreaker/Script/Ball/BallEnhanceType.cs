// Basic is included only as a non-enhanceable sentinel (BallEnhanceManager always reports no
// axes for it) -- BasicBallHitEffect needs a value for BaseBallHitEffect.EnhanceType same as
// every other hit effect, but Basic itself is never offered in Shop packs or purchasable.
public enum BallEnhanceType
{
    Basic,
    Bomb,
    Fire,
    Lightning,
    Row,
    Column,
    Cross,
}
