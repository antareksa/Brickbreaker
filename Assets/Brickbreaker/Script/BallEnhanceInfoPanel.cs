using TMPro;
using UnityEngine;

// One owned Ball Enhance entry in the info popup -- read-only, unlike BallEnhancePanel (the
// pickable version shown inside an opened pack). Shows the axis's ACTUAL current number, not
// just what it governs, so the player can see what their purchased levels bought them.
public class BallEnhanceInfoPanel : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text LevelText;

    public void SetInfo(BallEnhanceType type, BallEnhanceAxis axis)
    {
        int level = BallEnhanceManager.Instance.GetLevel(type, axis);

        NameText.text = $"{type} - {axis}";
        LevelText.text = $"Lv. {level}/{BallEnhanceManager.MaxLevel}";
        DescriptionText.text = $"{BallEnhanceManager.GetAxisDescription(type, axis)}: {GetValueLabel(type, axis, level)}";
    }

    // The concrete value this axis currently sits at -- e.g. "60%", "2 tiles", "3 neighbors".
    private static string GetValueLabel(BallEnhanceType type, BallEnhanceAxis axis, int level)
    {
        BallEnhanceManager manager = BallEnhanceManager.Instance;

        switch (axis)
        {
            case BallEnhanceAxis.Chance:
                return $"{manager.GetChanceValueAtLevel(level) * 100f:F0}%";

            case BallEnhanceAxis.Range:
                int range = manager.GetRangeValueAtLevel(level);
                return type == BallEnhanceType.Bomb ? $"{range} neighbors" : $"{range} tiles";

            case BallEnhanceAxis.ProcCount:
                return $"{manager.GetProcChanceAtLevel(level) * 100f:F0}%";

            default:
                return string.Empty;
        }
    }
}
