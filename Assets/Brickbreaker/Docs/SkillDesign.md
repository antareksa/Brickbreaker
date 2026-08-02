# Skill System Design Notes

## System overview

- `SkillManager` accumulates skill points per ball hit (`AddSkillPoint(1f)` — always 1 raw point
  per hit, regardless of level). Activation is deferred: points only trigger `TryActivateSkill()`
  after the current shot fully finishes (including coin collection), never mid-shot.
- Threshold to activate is `ActiveSkill.SkillPointNeeded`, read live every check (not cached),
  since it depends on the current wave-derived level.
- While a boss is alive and not yet defeated, activating the skill damages the boss
  (`BossManager.DamageBoss(ActiveSkill.CurrentDamage)`) instead of running the skill's normal
  brick-clearing effect. Effect only fires normally once the boss is defeated.

## Skill effects (`BaseSkillEffect` subclasses)

| Skill | Behavior |
|---|---|
| `RandomColumnsSkillEffect` | Picks even columns (2,4,6) or odd columns (1,3,5) at random, damages every brick in those columns. |
| `FirstRowsSkillEffect` | Damages the top `RowCount` rows counting down from the spawn row (newest bricks). |
| `LastRowsSkillEffect` | Damages the bottom `RowCount` rows counting up from the bottom row (bricks closest to the player). |

Not yet wired to a skill-selection UI — `SkillManager.ActiveSkill` is a single fixed reference for
now.

## Level, charge, damage (implemented)

### Level

`BaseSkillEffect.CurrentLevel` is a computed property, derived from the current wave — no longer
a manually-set field: `CurrentLevel => (GameManager.Instance.GetWave() / 10) + 1`. Every 10 waves
bumps the level by 1 (wave 1–10 → Lvl.1, 11–20 → Lvl.2, 21–30 → Lvl.3, ...).

### Charge requirement (skill points needed)

`BaseSkillEffect.PercentPerHitByLevel` is a public `AnimationCurve` — a visual graph, editable in
the Inspector by dragging keyframes, rather than a hardcoded formula or array. This was chosen
specifically because the real per-level values are still being tuned against reference data (only
levels 1, 2, 3, 7 are confirmed so far) — a curve lets design adjust the shape directly without
touching code.

Seeded default keyframes (level → % per hit):

| Level | % per hit | Points needed (100 / %) | Status |
|---|---|---|---|
| 1 | 0.500% | 200 | confirmed |
| 2 | 0.2% | 500 | confirmed |
| 3 | 0.12% | 833 | confirmed |
| 7 | 0.025% | 4,000 | confirmed |
| 10 | 0.0205% | 4,878 | interpolated (asymptotic fit, floor ≈0.02%) |

Levels 4, 5, 6, 8, 9 are whatever the curve interpolates between those keyframes in the Editor
(smooth by default) — not separately confirmed values. Adjust keyframes directly in the Inspector
as more real data comes in; no code change needed for retuning.

`SkillManager.GetSkillPointNeeded()` / `IsSkillPoint()` both read `ActiveSkill.SkillPointNeeded`
live (computed as `100 / PercentPerHitByLevel.Evaluate(CurrentLevel)`), so the threshold updates
automatically as the wave advances. `PlayerUniqueSkillHUD` refreshes the meter's `maxValue` and
level text on `GameManager.OnWaveChanged`, not just once at `Start()`.

### Damage

`BaseSkillEffect.BaseDamage` is a flat `int` per skill (no longer a `DamagePerLevel[]` array or a
raw-wave multiplier). `CurrentDamage`:

```
bonusDamageFromTrait = BaseDamage * (TraitManager.Instance.GetTraitValue(TraitType.SkillDamageBoost) / 100f)
CurrentDamage = Mathf.RoundToInt((BaseDamage + bonusDamageFromTrait) * CurrentLevel)
```

`TraitType.SkillDamageBoost` is a persistent meta-upgrade (`TraitManager`, independent of the
current run) whose value is stored as a whole percent number (e.g. `20` for 20%).

## Still open

- Exact shape of `PercentPerHitByLevel` between the confirmed keyframes (4–6, 8–9) — needs real
  data or a design pass on the curve's tangents/interpolation in the Inspector.
- Whether the 0.02% floor at level 10 is correct.
- `BaseDamage` value per skill (`RandomColumnsSkillEffect`, `FirstRowsSkillEffect`,
  `LastRowsSkillEffect`) — not yet set, needs balancing.
- Skill-selection UI (`SkillManager.ActiveSkill` is still a single fixed Inspector reference).
