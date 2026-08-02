using System.Collections.Generic;
using UnityEngine;

public class BallInfoHUD : BaseHUD
{
    public List<BallInfoPanel> Panels;

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.LaunchManager.OnBallAdded.AddListener(HandleBallAdded);

        // Prime the display with whatever balls already exist -- a subscription only fires on
        // future adds, not the balls already in the roster before this HUD subscribed.
        RefreshPanels();
    }

    private void HandleBallAdded(BallControllerV2 ball)
    {
        RefreshPanels();
    }

    // One panel per gacha entry -- always all 6, even at count 0 -- rather than only showing
    // whichever types the player currently happens to own.
    private void RefreshPanels()
    {
        List<BallGachaEntry> gachaEntries = GameManager.Instance.LaunchManager.GachaEntries;

        Dictionary<string, int> countByName = new Dictionary<string, int>();
        foreach (BallControllerV2 ball in GameManager.Instance.LaunchManager.Balls)
        {
            countByName.TryGetValue(ball.BallName, out int count);
            countByName[ball.BallName] = count + 1;
        }

        for (int i = 0; i < Panels.Count; i++)
        {
            if (i >= gachaEntries.Count || gachaEntries[i].Prefab == null)
            {
                Panels[i].gameObject.SetActive(false);
                continue;
            }

            BallControllerV2 prefab = gachaEntries[i].Prefab;
            countByName.TryGetValue(prefab.BallName, out int count);

            Panels[i].gameObject.SetActive(true);
            Panels[i].SetInfo(prefab.BallIcon, prefab.BallName, count);
        }
    }
}
