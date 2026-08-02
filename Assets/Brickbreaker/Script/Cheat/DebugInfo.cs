using UnityEngine;

// Immediate-mode debug overlay (OnGUI, same approach as CheatGUI) showing the current game
// phase -- placed top-right since CheatGUI already occupies top-left.
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

        GUILayout.BeginArea(new Rect(Screen.width - PanelWidth - Margin, Margin, PanelWidth, 40));

        GameState state = GameManager.Instance.StateMachine.CurrentState;
        GUILayout.Label($"Phase: {state}", _rightAlignedStyle);

        GUILayout.EndArea();
    }
}
