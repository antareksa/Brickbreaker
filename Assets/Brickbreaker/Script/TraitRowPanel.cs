using TMPro;
using UnityEngine;
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

    public void SetInfo(TraitDefinition definition)
    {
        Icon.sprite = definition.Icon;
        NameText.text = definition.Name;
        DescriptionText.text = definition.Description;

        int level = TraitManager.Instance.GetLevel(definition.Type);
        LevelText.text = $"Lv. {level}/{definition.MaxLevel}";

        bool isMaxed = level >= definition.MaxLevel;
        UpgradeButton.interactable = !isMaxed && TraitManager.Instance.CanUpgrade(definition.Type);

        if (isMaxed)
        {
            UpgradeButtonText.text = "MAX";
            CostText.text = string.Empty;
        }
        else
        {
            UpgradeButtonText.text = "LV UP";
            CostText.text = definition.CostPerLevel[level].ToString();
        }
    }
}
