using UnityEngine;
using TMPro;
using DG.Tweening;
public class CounterDown : MonoBehaviour
{
    public TextMeshProUGUI countText;
    public Tween StartCount()
    {
        countText.text = "3";
        Sequence seq = DOTween.Sequence();
        countText.transform.localScale = new Vector3(1, 1, 1);
        countText.alpha = 1;
        seq.Append(countText.transform.DOScale(2, 1));
        seq.Join(countText.DOFade(0, 1).OnComplete(()=> {
            countText.text = "2";
            countText.transform.localScale = new Vector3(1, 1, 1);
            countText.alpha = 1;
        }));
        seq.Append(countText.transform.DOScale(2, 1));
        seq.Join(countText.DOFade(0, 1).OnComplete(() => {
            countText.text = "1";
            countText.transform.localScale = new Vector3(1, 1, 1);
            countText.alpha = 1;
        }));
        seq.Append(countText.transform.DOScale(2, 1));
        seq.Join(countText.DOFade(0, 1));
        seq.SetUpdate(true);

        return seq;
    }
    
}
