using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ExtraLifeScreen : UIScreen
{

    public Image backgroundPanel;
    public RectTransform showAdButton;
    public RectTransform cancelButton;

    public override Tween ShowTweening()
    {
        showAdButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        Sequence animSeq = DOTween.Sequence();
        backgroundPanel.color = new Color(0.13f, 0.13f, 0.13f, 0);
        animSeq.Append(backgroundPanel.DOFade(0.9f, 0.5f).SetEase(Ease.InExpo)).OnComplete(()=> {
            showAdButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
        });
        animSeq.SetUpdate(true);
        return animSeq;
    }
}
