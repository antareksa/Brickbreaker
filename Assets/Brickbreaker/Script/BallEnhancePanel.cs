using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One revealed (type, axis) option inside an opened Ball Enhance pack -- free to pick (the pack
// itself was already paid for via BallEnhancePackOfferPanel), just capped at that pack's
// MaxPicks (enforced by the owning BallEnhanceRevealPopup, not this script).
public class BallEnhancePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text CostText;
    public Button SelectButton;
    public TMP_Text SelectButtonText;
    public float HoverScale = 1.1f;

    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    public BallEnhanceType Type { get; private set; }
    public BallEnhanceAxis Axis { get; private set; }
    public bool IsPicked { get; private set; }

    private Vector3 originalScale;
    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetInfo(BallEnhanceType type, BallEnhanceAxis axis)
    {
        Type = type;
        Axis = axis;
        IsPicked = false;

        int currentLevel = BallEnhanceManager.Instance.GetLevel(type, axis);
        NameText.text = $"{type} - {axis} (Lvl {currentLevel + 1})";
        DescriptionText.text = BallEnhanceManager.GetAxisDescription(type, axis);
        if (CostText != null) CostText.text = "Free";

        SelectButton.interactable = true;
        if (SelectButtonText != null) SelectButtonText.text = "Select";
    }

    public void MarkPicked()
    {
        IsPicked = true;
        SelectButton.interactable = false;
        if (SelectButtonText != null) SelectButtonText.text = "Picked";
    }

    // Locked once the owning pack's pick cap (MaxPicks) is reached -- no affordability check
    // here, since revealed options are always free to pick.
    public void SetLocked(bool locked)
    {
        if (IsPicked) return;
        SelectButton.interactable = !locked;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * HoverScale;
        OnHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        OnHoverExit?.Invoke();
    }
}
