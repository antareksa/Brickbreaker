using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One row's display, one per trait. Purely a dumb view -- SetInfo populates it from current
// TraitManager state, but it doesn't wire its own button click; UpgradeHUD owns that so it can
// refresh every row (not just this one) after a purchase changes the shared token count.
public class TraitRowPanel : MonoBehaviour
{
    public Image Icon;
    public TMP_Text NameText;
    public TMP_Text LevelText;
    public TMP_Text DescriptionText;
    public Button UpgradeButton;
    public TMP_Text UpgradeButtonText;
    public TMP_Text CostText;

    // Cached each SetInfo so the hover handlers (wired once in Awake) can swap between them
    // without recomputing anything on pointer enter/exit.
    private string _currentDescription;
    private string _hoverDescription;

    // Tracked separately from the text itself so a mid-hover SetInfo refresh (e.g. right after
    // clicking Upgrade, which re-populates every row) can restore the hover text instead of
    // clobbering it back to the non-hover description while the pointer never left the button.
    private bool _isHovering;

    private void Awake()
    {
        EventTrigger trigger = UpgradeButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = UpgradeButton.gameObject.AddComponent<EventTrigger>();

        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            _isHovering = true;
            DescriptionText.text = _hoverDescription;
        });
        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            _isHovering = false;
            DescriptionText.text = _currentDescription;
        });
    }

    private static void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    public void SetInfo(TraitDefinition definition)
    {
        Icon.sprite = definition.Icon;
        NameText.text = definition.Name;

        int level = TraitManager.Instance.GetLevel(definition.Type);
        LevelText.text = level <= 0 ? "Locked" : $"Lv. {level}/{definition.MaxLevel}";

        bool isMaxed = level >= definition.MaxLevel;

        float value = level > 0
        ? TraitManager.Instance.GetTraitValue(definition.Type)
        : definition.ValuePerLevel[0];

        // Once unlocked (and not maxed), hovering the upgrade button previews current>next so
        // the player can see the upgrade's effect before buying it. Locked/maxed rows only have
        // one value to show either way, so hover doesn't change anything for them.
        float? nextValue = (level > 0 && !isMaxed) ? definition.ValuePerLevel[level] : (float?)null;

        _currentDescription = definition.GetFormattedDescription(value);
        _hoverDescription = nextValue.HasValue ? definition.GetFormattedDescription(value, nextValue) : _currentDescription;
        DescriptionText.text = _isHovering ? _hoverDescription : _currentDescription;

        UpgradeButton.interactable = !isMaxed && TraitManager.Instance.CanUpgrade(definition.Type);

        if (isMaxed)
        {
            UpgradeButtonText.text = "MAX";
            CostText.text = string.Empty;
        }
        else
        {
            UpgradeButtonText.text = level <= 0 ? "Unlock" : "LV UP";
            CostText.text = definition.CostPerLevel[level].ToString();
        }
    }
}
