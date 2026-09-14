using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ScoreTweener : MonoBehaviour
{
    public GameObject scoreText;
    public Image scoreBackground;
    public Image highScoreBackground;
    public Image dotRight1;
    public Image dotRight2;
    public Image dotLeft1;
    public Image dotLeft2;
    public Image bestScoreBigCrown;
    public RectTransform bestScore;
    
    public Tween StartShowingTween()
    {
        //initial values
        float height = scoreBackground.rectTransform.sizeDelta.y;
        float width = scoreBackground.rectTransform.sizeDelta.x;
        scoreBackground.rectTransform.sizeDelta = Vector3.zero;
        Vector3 bestScorePosition = bestScore.anchoredPosition;

        //reset values
        dotRight1.color = dotRight2.color = dotLeft1.color = dotLeft2.color = scoreBackground.color;
        bestScoreBigCrown.enabled = false;
        highScoreBackground.enabled = false;
        dotRight1.rectTransform.localScale = Vector3.zero;
        dotLeft1.rectTransform.localScale = Vector3.zero;
        dotRight2.rectTransform.localScale = Vector3.zero;
        dotLeft2.rectTransform.localScale = Vector3.zero;
        bestScore.anchoredPosition = Vector3.zero;
        bestScore.gameObject.SetActive(false);
        scoreText.SetActive(false);

        //tweening
        Sequence seq  = DOTween.Sequence();
        seq.Append( scoreBackground.rectTransform.DOSizeDelta(new Vector2(height,height), 0.2f));
        seq.Append( scoreBackground.rectTransform.DOSizeDelta(new Vector2(width, height), 0.4f).SetEase(Ease.OutBack).OnComplete(()=> { scoreText.SetActive(true); }));
        seq.Join(dotRight1.rectTransform.DOScale(1, 0.1f).SetDelay(0.2f));
        seq.Join(dotLeft1.rectTransform.DOScale(1, 0.1f));
        seq.Join(dotRight1.rectTransform.DOPunchAnchorPos(new Vector2(50, 0), 0.2f));
        seq.Join(dotLeft1.rectTransform.DOPunchAnchorPos(new Vector2(-50, 0), 0.2f));
        seq.Join(bestScore.DOAnchorPos(bestScorePosition, 0.2f).SetEase(Ease.OutCubic).OnStart(()=> { bestScore.gameObject.SetActive(true); }));
        seq.Join(dotRight2.rectTransform.DOScale(1, 0.1f).SetDelay(0.1f).SetEase(Ease.OutCubic));
        seq.Join(dotLeft2.rectTransform.DOScale(1, 0.1f).SetEase(Ease.OutCubic));
        
        return seq;
    }

    public Tween NewHighScoreTween()
    {
        //initial values
        float height = scoreBackground.rectTransform.sizeDelta.y;
        float width = scoreBackground.rectTransform.sizeDelta.x;

        //reset values
        highScoreBackground.rectTransform.sizeDelta = new Vector2(width / 5, height);
        highScoreBackground.enabled = true;
        bestScoreBigCrown.enabled = false;
        bestScore.gameObject.SetActive(false);
        bestScoreBigCrown.rectTransform.anchoredPosition = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(highScoreBackground.rectTransform.DOSizeDelta(new Vector2(width, height), 0.4f).SetEase(Ease.OutBack)/*.OnComplete(() => { scoreText.SetActive(true); })*/);
        seq.Join(dotRight1.DOColor(highScoreBackground.color, 0).SetDelay(0.2f));
        seq.Join(dotLeft1.DOColor(highScoreBackground.color, 0));
        seq.Join(dotRight2.DOColor(highScoreBackground.color, 0));
        seq.Join(dotLeft2.DOColor(highScoreBackground.color, 0));
        seq.Join(dotRight1.rectTransform.DOPunchAnchorPos(new Vector2(50, 0), 0.2f));
        seq.Join(dotLeft1.rectTransform.DOPunchAnchorPos(new Vector2(-50, 0), 0.2f));
        seq.Join(dotRight2.rectTransform.DOPunchAnchorPos(new Vector2(100, 0), 0.2f));
        seq.Join(dotLeft2.rectTransform.DOPunchAnchorPos(new Vector2(-100, 0), 0.2f));
        seq.Join(bestScoreBigCrown.rectTransform.DOAnchorPosY(75, 0.2f).SetEase(Ease.OutBack).OnStart(()=> { bestScoreBigCrown.enabled = true; }));
        return seq;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            NewHighScoreTween();
        }
    }

}
