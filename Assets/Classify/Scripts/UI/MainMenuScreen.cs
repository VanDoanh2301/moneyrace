using UnityEngine;
using DG.Tweening;

/// <summary>
/// The UI of the main menu screen
/// </summary>
public class MainMenuScreen : UIScreen
{
    public UIManager uiManager;
    public RectTransform clas;
    public RectTransform sify;
    public RectTransform highScorePanel;

    public GameObject PlayButton;

    public GameplayManager manager;

    public void Play()
    {
        uiManager.ShowScreen(Screens.Gameplay);
        manager.StartGame();
    }
    
    public override Tween ShowTweening()
    {
        PlayButton.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        PlayButton.transform.eulerAngles = new Vector3(0, 0, 90);
        clas.anchoredPosition = new Vector2(-600, 0);
        sify.anchoredPosition = new Vector2(600, 0);

        Sequence animSeq = DOTween.Sequence();
        animSeq.Append(clas.DOAnchorPos(Vector3.zero, 1).SetEase(Ease.InOutElastic));
        animSeq.Join(sify.DOAnchorPos(Vector3.zero, 1).SetEase(Ease.InOutElastic));
        animSeq.Append(PlayButton.transform.DOScale(1, 0.5f).SetEase(Ease.InQuart));
        animSeq.Join(PlayButton.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.InQuart));
        animSeq.Append(highScorePanel.DOAnchorPosY(-130, 0.5f).SetEase(Ease.OutBounce));
        return animSeq;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowTweening();
        }
    }
}
