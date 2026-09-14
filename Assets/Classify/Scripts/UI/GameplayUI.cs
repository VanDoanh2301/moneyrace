using UnityEngine;
using deVoid.Utils;
using DG.Tweening;

/// <summary>
/// The UI of the Gameplay Screen
/// </summary>
public class GameplayUI : UIScreen
{
    [SerializeField] TMPro.TextMeshProUGUI scoreText;
    [SerializeField] TMPro.TextMeshProUGUI highScoreText;
    public FloatField score;
    public ScoreTweener scoreAnimator;
    public LivesController livesController;
    private void OnEnable()
    {
        score.onChange.AddListener(UpdateScore);
        Signals.Get<NewHighScoreSignal>().AddListener(UpdateHighScore);
        highScoreText.text = LocalDataStorage.HighScore.ToString();
    }
    public override void Show(bool animate = true)
    {
        base.Show(animate);
        highScoring = false;
    }
    public override void Hide()
    {
        base.Hide();
        livesController.Hide();
    }
    bool highScoring = false;
    private void UpdateHighScore(int newHighScore)
    {
        highScoreText.text = newHighScore.ToString();
        if (!highScoring)
        {
            scoreAnimator.NewHighScoreTween();
            highScoring = true;
        }
    }

    private void UpdateScore()
    {
        scoreText.text = score.Value.ToString();
    }

    private void OnDisable()
    {
        score.onChange.RemoveListener(UpdateScore);
        Signals.Get<NewHighScoreSignal>().RemoveListener(UpdateHighScore);

    }

    public override Tween ShowTweening()
    {
        livesController.Hide();
        return scoreAnimator.StartShowingTween().SetDelay(0.25f).OnComplete(()=> { livesController.Show(); });
    }

    public override Tween HideTweening()
    {
        livesController.Hide();
        return base.HideTweening();
    }
}
