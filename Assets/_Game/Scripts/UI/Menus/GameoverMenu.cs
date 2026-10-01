using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameoverMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _bestScoreText;
    [SerializeField] Button _restartButton;
    [SerializeField] Button _homeButton;
    [SerializeField] Button _shopButton;

    public override void SetEnable()
    {
        base.SetEnable();

        _restartButton.interactable = true;
        _homeButton.interactable = true;

        SetScoreDisplay();
    }

    private void Start()
    {
        OnButtonPressed(_restartButton, RestartButton);
        OnButtonPressed(_homeButton, HomeButton);
        OnButtonPressed(_shopButton, ShopButton);
    }

    private void SetScoreDisplay()
    {
        ScoreManager sc = FindAnyObjectByType<ScoreManager>();
        int lastScore = sc.Score;
        int bestScore = sc.BestScore;

        _scoreText.text = $"Last : {lastScore}";
        _bestScoreText.text = $"Best : {bestScore}";
    }

    private void ShopButton()
    {
        MenuManager.GetInstance().OpenMenu(MenuType.Shop);
    }

    private void HomeButton()
    {
        _homeButton.interactable = false;
        _restartButton.interactable = false;

        StartCoroutine(LevelLoader.ReloadLevelAsync(() =>
        {
            MenuManager.GetInstance().SwitchMenu(MenuType.Main);
        }));
    }

    private void RestartButton()
    {
        _restartButton.interactable = false;
        _homeButton.interactable = false;

        StartCoroutine(LevelLoader.ReloadLevelAsync(() =>
        {
            MenuManager.GetInstance().SwitchMenu(MenuType.Gameplay);
        }));
    }
}
