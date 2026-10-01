using TMPro;
using UnityEngine;

public class GameplayMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] private TMP_Text _scoreText;

    public override void SetEnable()
    {
        base.SetEnable();

        int score = FindAnyObjectByType<ScoreManager>().Score;
        UpdateScore(score);
    }

    private void UpdateScore(int currentScore)
    {
        _scoreText.text = currentScore.ToString();
    }

    private void OnEnable()
    {
        GameManager.OnScoreUpdated += UpdateScore;
    }

    private void OnDisable()
    {
        GameManager.OnScoreUpdated -= UpdateScore;
    }
}