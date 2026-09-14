using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// The UI of the Game Over Screen
/// </summary>
public class GameOverUI : UIScreen
{
    public Image backgroundPanel;
    public RectTransform gameOverLabelBG;
    public GameObject gameOverLabel;
    public RectTransform replayButton;
    public RectTransform homeButton;
    public TextMeshProUGUI scoreText;
    public FloatField score;
    public Color highScoreColor;
    public Color normalScoreColor;
    public RectTransform crown;
    public override void Show(bool animate = true)
    {
        if (score.Value == LocalDataStorage.HighScore)
        {
            scoreText.color = highScoreColor;
            crown.gameObject.SetActive(true);
        }
        else
        {
            scoreText.color = normalScoreColor;
            crown.gameObject.SetActive(false);
        }
        scoreText.text = score.Value.ToString();
        base.Show(animate);
    }
    public override Tween ShowTweening()
    {

        gameOverLabelBG.localScale = new Vector3(0, 0.25f);
        replayButton.localScale = Vector3.zero;
        homeButton.localScale = Vector3.zero;
        crown.localScale = Vector3.zero;
        scoreText.rectTransform.localScale = new Vector3(0.01f, 0.01f,0.01f);

        gameOverLabel.SetActive(false);
        Sequence animSeq = DOTween.Sequence();
        backgroundPanel.color = new Color(0.13f, 0.13f, 0.13f, 0);
        animSeq.Append(backgroundPanel.DOFade(0.9f, 0.5f).SetEase(Ease.InExpo));
        animSeq.Append(gameOverLabelBG.DOScaleX(1, 0.2f));
        animSeq.Append(gameOverLabelBG.DOScaleY(1, 0.2f).OnComplete(() => { gameOverLabel.SetActive(true); }));
        animSeq.Append(replayButton.DOScale(1, 0.25f).SetEase(Ease.InOutBack) );
        animSeq.Join(homeButton.DOScale(1, 0.25f).SetEase(Ease.InOutBack) );
        animSeq.Append(scoreText.rectTransform.DOScale(1, 0.25f).SetEase(Ease.InOutBack));
        animSeq.Append(crown.DOScale(1,0.5f).SetEase(Ease.OutBounce));
        animSeq.SetUpdate(true);
        return animSeq;

    }


    
}
