using deVoid.Utils;
using UnityEngine;
using TMPro;
using System;

public class HighScoreText : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;

    private void Awake()
    {
        highScoreText.text = LocalDataStorage.HighScore.ToString();

    }
    private void OnEnable()
    {
        Signals.Get<NewHighScoreSignal>().AddListener(OnHighScoreChanged);
    }

    private void OnHighScoreChanged(int newHighScore)
    {
        highScoreText.text = newHighScore.ToString();
    }

    private void OnDisable()
    {
        Signals.Get<NewHighScoreSignal>().RemoveListener(OnHighScoreChanged);

    }
}
