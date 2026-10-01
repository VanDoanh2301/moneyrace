using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] Button _yesButton;
    [SerializeField] Button _noButton;

    public override void SetEnable()
    {
        base.SetEnable();

        _noButton.interactable = true;
    }

    private void Start()
    {
        OnButtonPressed(_yesButton, YesButtonPressed);
        OnButtonPressed(_noButton, NoButtonPressed);
    }

    private void NoButtonPressed()
    {
        _noButton.interactable = false;

        MenuManager.GetInstance().CloseMenu();
    }

    private void YesButtonPressed()
    {
        Application.Quit();
    }


}
