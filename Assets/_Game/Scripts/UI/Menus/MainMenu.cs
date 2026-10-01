using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] Button _creditButton;
    [SerializeField] Button _rateButton;
    [SerializeField] Button _settingsButton;
    [SerializeField] Button _shopButton;

    [Header("Database References :")]
    [SerializeField] CreditDataSO _data;

    TitleAnimation _titleAnim;

    protected override void Awake()
    {
        base.Awake();

        _titleAnim = GetComponent<TitleAnimation>();
    }

    public override void SetEnable()
    {
        base.SetEnable();

        _titleAnim.SetEnable();
    }

    public override void SetDisable()
    {
        base.SetDisable();

        _titleAnim.SetDisable();
    }

    private void Start()
    {
        OnButtonPressed(_creditButton, CreditButtonPressed);
        OnButtonPressed(_rateButton, RateButtonPressed);
        OnButtonPressed(_settingsButton, SettingsButtonPressed);
        OnButtonPressed(_shopButton, ShopButtonPressed);
    }

    private void ShopButtonPressed()
    {
        MenuManager.GetInstance().OpenMenu(MenuType.Shop);
    }

    private void SettingsButtonPressed()
    {
        MenuManager.GetInstance().OpenMenu(MenuType.Setting);
    }

    private void RateButtonPressed()
    {
        Application.OpenURL($"market://details?id={Application.identifier}");
    }

    private void CreditButtonPressed()
    {
        MenuManager.GetInstance().OpenMenu(MenuType.Credit);
    }
}
