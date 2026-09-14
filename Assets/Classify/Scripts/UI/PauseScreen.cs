using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PauseScreen : UIScreen
{
    public RectTransform pauseLabelBG;
    public GameObject pauseLabel;
    public RectTransform resumeButton;
    public RectTransform homeButton;

    public override Tween ShowTweening()
    {

        pauseLabelBG.localScale = new Vector3(0, 0.25f);
        resumeButton.anchoredPosition = pauseLabelBG.anchoredPosition;
        homeButton.anchoredPosition = pauseLabelBG.anchoredPosition;

        pauseLabel.SetActive(false);
        resumeButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);

        Sequence animSeq = DOTween.Sequence();
        animSeq.Append(pauseLabelBG.DOScaleX(1, 0.2f));
        animSeq.Append(pauseLabelBG.DOScaleY(1, 0.2f).OnComplete(() => { pauseLabel.SetActive(true); }));
       
        animSeq.Append(resumeButton.DOAnchorPos(Vector3.zero, 0.5f).SetEase(Ease.OutBounce)
            .OnStart(() => { resumeButton.gameObject.SetActive(true); })
            );
        animSeq.Join(homeButton.DOAnchorPos(Vector3.zero, 0.15f).SetEase(Ease.InQuint).OnStart(
           () =>
           {
               homeButton.gameObject.SetActive(true);
           }
           ));
        animSeq.Join(homeButton.DOAnchorPos(new Vector2(0,-380), 0.4f).SetEase(Ease.OutBounce).SetDelay(0.15f));

        animSeq.SetUpdate(true);
        return animSeq;

    }

    public override Tween HideTweening()
    {
        pauseLabel.SetActive(false);
        Sequence animSeq = DOTween.Sequence();
        animSeq.Append(homeButton.DOAnchorPos(pauseLabelBG.anchoredPosition, 0.3f).OnComplete(()=> {
            homeButton.gameObject.SetActive(false);
            pauseLabel.SetActive(false);
        }) );
        animSeq.Join(resumeButton.DOAnchorPos(pauseLabelBG.anchoredPosition, 0.2f).SetDelay(0.1f).OnComplete(() => {
            resumeButton.gameObject.SetActive(false);
        }));
        animSeq.Append(pauseLabelBG.DOScaleY(0.25f, 0.2f));
        animSeq.Append(pauseLabelBG.DOScaleX(0, 0.2f));
        animSeq.SetUpdate(true);
        return animSeq;
    }
    
}
