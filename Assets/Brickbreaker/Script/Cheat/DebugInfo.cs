using UnityEngine;

// Immediate-mode debug overlay (OnGUI, same approach as CheatGUI) showing the current game
// state and boss phase progress -- placed top-right since CheatGUI already occupies top-left.
public class DebugInfo : MonoBehaviour
{
    public int PanelWidth = 200;
    public int Margin = 10;

    private GUIStyle _rightAlignedStyle;

    private void OnGUI()
    {
        if (_rightAlignedStyle == null)
        {
            _rightAlignedStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
        }

        GUILayout.BeginArea(new Rect(Screen.width - PanelWidth - Margin, Margin, PanelWidth, 60));

        GameState state = GameManager.Instance.StateMachine.CurrentState;
        GUILayout.Label($"Phase: {state}", _rightAlignedStyle);

        // CurrentPhaseHP counts DOWN from the phase's max, so progress toward clearing it is
        // the inverse -- matches how BossHUD's Slider computes the same thing.
        BossManager bossManager = GameManager.Instance.BossManager;
        if (bossManager != null && !bossManager.IsDefeated)
        {
            float maxHp = bossManager.CurrentPhaseMaxHP;
            float damageDealt = maxHp - bossManager.CurrentPhaseHP;
            GUILayout.Label($"Boss: {damageDealt:F0} / {maxHp:F0}", _rightAlignedStyle);
        }

        GUILayout.EndArea();
    }
}
