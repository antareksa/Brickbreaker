using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpPanel : MonoBehaviour
{
    public Image PowerUpIcon;
    public TMP_Text PowerUpNameText;
    public TMP_Text DescriptionText;

    public void SetInfo(BasePowerUp powerUp)
    {
        PowerUpIcon.sprite = powerUp.PowerUpImage;
        PowerUpNameText.text = powerUp.PowerUpName;
        DescriptionText.text = powerUp.Description;
    }
}
