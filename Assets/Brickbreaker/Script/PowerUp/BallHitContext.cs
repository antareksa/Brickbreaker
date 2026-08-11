// Per-hit context passed to PowerUp effect hooks -- carries whichever ball/shot/brick stats an
// effect might care about. Grows as new effect types need new context, never shrinks/changes
// shape for existing fields (so existing effects don't need touching when new ones are added).
public struct BallHitContext
{
    // Combined total. Bricks are tagged Bouncy too, so a brick hit reflects the ball and counts
    // here just like a wall does -- the two split-out counters below are what effects should use
    // when they specifically mean one surface or the other.
    public int BouncesThisShot;

    public int BrickBouncesThisShot;
    public int WallBouncesThisShot;
    public int FireIndexThisShot;
    public bool HitWallBeforeAnyBrick;
    public bool SideWallBounceSinceLastHit;
    public bool IsFirstHitThisShot;
    public int BricksRemainingOnField;
    public int BricksDestroyedThisShot;

    // How many times THIS ball already hit THIS brick earlier in the same shot (0 on the first
    // hit). Tracked per (ball, brick) on the ball itself -- not a shared counter on the brick,
    // so hits from other balls (or from spread/skill damage) don't feed this.
    public int RepeatHitsOnBrick;

    public BrickController HitBrick;
}
