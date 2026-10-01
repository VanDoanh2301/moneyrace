using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] TMP_Text _devText;
    [SerializeField] TMP_Text _sfxText;
    [SerializeField] TMP_Text _contactText;
    [Space]
    [SerializeField] Button _backButton;

    [Header("Database References :")]
    [SerializeField] CreditDataSO _data;

    private void OnEnable()
    {
        _backButton.interactable = true; 

        _devText.text = _data.DevText;
        _sfxText.text = _data.SfxTxt;
        _contactText.text = _data.ContactText;
    }

    private void Start()
    {
        OnButtonPressed(_backButton, BackButtonPressed);
    }

    private void BackButtonPressed()
    {
        _backButton.interactable = false;
        MenuManager.GetInstance().CloseMenu();
    }
}
