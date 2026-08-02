// Per-hit context passed to PowerUp effect hooks -- carries whichever ball/shot/brick stats an
// effect might care about. Grows as new effect types need new context, never shrinks/changes
// shape for existing fields (so existing effects don't need touching when new ones are added).
public struct BallHitContext
{
    public int BouncesThisShot;
    public int FireIndexThisShot;
    public bool HitWallBeforeAnyBrick;
    public bool SideWallBounceSinceLastHit;
    public bool IsFirstHitThisShot;
    public int BricksRemainingOnField;
    public int BricksDestroyedThisShot;
    public BrickController HitBrick;
}
