using UnityEngine;
using UnityEngine.UI;

// One HP "pip" in the health display -- unlike PowerUpPanel/ConsumablePanel there's no icon/name/
// description to show, just whether this pip is currently filled (active) or empty (inactive).
public class HealthPlayerPanel : MonoBehaviour
{
    public Image Fill;
    public void SetActive(bool isActive)
    {
        Fill.gameObject.SetActive(isActive);
    }
}
