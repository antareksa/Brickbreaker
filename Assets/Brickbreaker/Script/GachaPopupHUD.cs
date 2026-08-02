using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaPopupHUD : BaseHUD
{
    public GameObject PopupRoot;
    public List<GachaInfoPanel> Panels;
    public float CloseDelay = 1f;

    private readonly List<GachaInfoPanel> _activePanels = new List<GachaInfoPanel>();

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.LaunchManager.OnGachaRolled.AddListener(HandleGachaRolled);
        PopupRoot.SetActive(false);
    }

    private void HandleGachaRolled(List<BallControllerV2> balls)
    {
        StopAllCoroutines();
        StartCoroutine(ShowResultRoutine(balls));
    }

    // One panel per individual ball rolled -- rolling 1 ball shows 1 panel, rolling 6 shows 6
    // (no grouping/counting, since the panel doesn't display a count anymore).
    private IEnumerator ShowResultRoutine(List<BallControllerV2> balls)
    {
        _activePanels.Clear();

        // Activate the popup root first -- a panel's Animator doesn't initialize (and SetTrigger
        // just warns and does nothing) while any ancestor, including PopupRoot, is still inactive.
        PopupRoot.SetActive(true);

        int panelIndex = 0;
        foreach (BallControllerV2 ball in balls)
        {
            if (ball == null) continue;
            if (panelIndex >= Panels.Count) break;

            GachaInfoPanel panel = Panels[panelIndex];

            panel.gameObject.SetActive(true);
            panel.SetInfo(ball.BallIcon, ball.BallName);
            panel.PlayReveal();
            _activePanels.Add(panel);

            panelIndex++;
        }

        for (int i = panelIndex; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }

        // Let the reveal animations play, but a click anywhere skips straight to the end.
        while (!AllRevealsFinished())
        {
            if (Input.GetMouseButtonDown(0))
            {
                SkipAllReveals();
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(CloseDelay);

        // Only after the delay does a click anywhere close the popup.
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        PopupRoot.SetActive(false);
    }

    private bool AllRevealsFinished()
    {
        foreach (GachaInfoPanel panel in _activePanels)
        {
            if (!panel.IsRevealFinished) return false;
        }
        return true;
    }

    private void SkipAllReveals()
    {
        foreach (GachaInfoPanel panel in _activePanels)
        {
            panel.SkipReveal();
        }
    }
}
