using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallInfoPanel : MonoBehaviour
{
    public Image BallIcon;
    public TMP_Text BallNameText;
    public TMP_Text BallCountText;

    public void SetInfo(Sprite icon, string ballName, int count)
    {
        BallIcon.sprite = icon;
        BallNameText.text = ballName;
        BallCountText.text = "x" + count;
    }
}
