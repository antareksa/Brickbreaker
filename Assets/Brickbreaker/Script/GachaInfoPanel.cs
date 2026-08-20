using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaInfoPanel : MonoBehaviour
{
    public Image BallIcon;
    public TMP_Text BallNameText;
    public Animator Animator;
    public GameObject NewIndicator;

    // Set true once the reveal animation completes (or is skipped). The reveal clip should call
    // OnRevealAnimationFinished via an Animation Event at the point it's considered "done".
    public bool IsRevealFinished { get; private set; } = true;

    public void SetInfo(Sprite icon, string ballName, bool isNew = false)
    {
        BallIcon.sprite = icon;
        BallNameText.text = ballName;
        NewIndicator.gameObject.SetActive(isNew);
    }

    public void PlayReveal()
    {
        IsRevealFinished = false;
        Animator.SetTrigger("Play");
    }

    public void SkipReveal()
    {
        if (IsRevealFinished) return;

        Animator.SetTrigger("Skip");
        IsRevealFinished = true;
    }

    // Wire this up as an Animation Event at the end of the reveal clip.
    public void OnRevealAnimationFinished()
    {
        IsRevealFinished = true;
    }
}
