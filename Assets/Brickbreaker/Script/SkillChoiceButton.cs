using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillChoiceButton : MonoBehaviour
{
    public Image Icon;
    public TMP_Text Name;
    public TMP_Text Description;

    private int _skillIndex;
    private Toggle _toggle;
    private ToggleGroup _group;

    private void Awake()
    {
        
    }

    public void UpdateChoiceData(BaseSkillEffect skill, ToggleGroup group, int skillIndex)
    {
        Icon.sprite = skill.SkillIcon;
        Name.text = skill.SkillName;
        Description.text = skill.Description;

        _skillIndex = skillIndex;

        _toggle = GetComponent<Toggle>();
        _toggle.group = group;
        _toggle.onValueChanged.AddListener(OnToggleChoice);
    }

    private void OnToggleChoice(bool isPicked)
    {
        if(isPicked)
        {
            GameManager.Instance.SkillManager.ChangeSkill(_skillIndex);
        }
    }

    public void Pick()
    {
        _toggle.isOn = true;
    }
}
