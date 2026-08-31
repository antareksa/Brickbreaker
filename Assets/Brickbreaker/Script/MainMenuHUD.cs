using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHUD : BaseHUD
{
    public GameObject PopupRoot;
    public Button StartButton;
    public Button UpgradeButton;
    public Button QuitButton;
    public UpgradeHUD UpgradeHUD;

    [Header("SkillPopUp")]
    public GameObject SkillChoicePopUp;
    public SkillChoiceButton SkillChoiceButtonPrefab;
    public Transform SkillChoiceButtonContainer;
    public ToggleGroup SkillChoiceGroup;

    protected override void Start()
    {
        base.Start();

        StartButton.onClick.AddListener(HandleStartClicked);
        UpgradeButton.onClick.AddListener(HandleUpgradeClicked);
        QuitButton.onClick.AddListener(() => Application.Quit());

        InitSkillChoice();

        Open();
    }

    public void Open()
    {
        PopupRoot.SetActive(true);
    }

    public void Close()
    {
        PopupRoot.SetActive(false);
    }

    private void HandleStartClicked()
    {
        SkillChoicePopUp.SetActive(true);
    }

    public void HandleStartGame()
    {
        SkillChoicePopUp.SetActive(false);
        Close();
        GameManager.Instance.BrickManager.RestartGame();
    }

    private void HandleUpgradeClicked()
    {
        UpgradeHUD.Open();
    }

    private void InitSkillChoice()
    {
        SkillChoicePopUp.SetActive(false);

        List<BaseSkillEffect> ListSkill = GameManager.Instance.SkillManager.ListSkillChoice;
        int SkillIndex = GameManager.Instance.SkillManager.GetSkillIndex();

        int index = 0;
        foreach (BaseSkillEffect effect in ListSkill)
        {
            SkillChoiceButton skillChoiceButton = Instantiate(SkillChoiceButtonPrefab, SkillChoiceButtonContainer);
            skillChoiceButton.UpdateChoiceData(effect, SkillChoiceGroup, index);
            if (index == SkillIndex) skillChoiceButton.Pick();

            index++;
        }
    }
}
