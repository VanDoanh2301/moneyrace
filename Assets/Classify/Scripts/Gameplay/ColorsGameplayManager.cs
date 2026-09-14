using deVoid.Utils;
using UnityEngine;
using System.Collections;
using DG.Tweening;


public class ColorsGameplayManager : GameplayManager
{
    [Header("Colors gameplay stuff")]
    public GameObject finger;
    public Transform mainCamera;
    [SerializeField] Animator ballHighlighterAnimator;
    [SerializeField] SpriteRenderer ballHighlighterRenderer;
    [SerializeField] CounterDown countDown;
    Item lastgenertedItem;
    public AudioSource backgroundMusicAudioSource;
    public AudioClip wrongItemSoundEffect;

    public override void StartGame()
    {
        base.StartGame();
        mainCamera.DOMoveY(0, .5f).SetEase(Ease.OutQuad);
        backgroundMusicAudioSource.Play();
    }
    protected void OnEnable()
    {
        Signals.Get<SwitchOpenGate>().AddListener(OnGateSwitched);
    }
    void OnDestroy()
    {
        Signals.Get<SwitchOpenGate>().RemoveListener(OnGateSwitched);
    }

    protected virtual void OnGateSwitched(int gateIndex)
    {
        if (State.Value == GameplayState.WaitingAction)
        {
            State.Value = GameplayState.Running;
            finger.SetActive(false);
            ReleaseNewBall(lastgenertedItem);
        }
    }

    void ReleaseNewBall(Item item)
    {

        (item as ColorItem).Release();
        delay = TimeBetweenItems;
    }

    protected override Item AddNewItemType()
    {
        Item item = base.AddNewItemType();
        (item as ColorItem).Freeze();
        //Close gate (i.e. open the other gate)
        gatesSwitch.OpenGate((LastAssignedGate + 1) % gates.Length);
        finger.SetActive(true);
        State.Value = GameplayState.WaitingAction;
        lastgenertedItem = item;
        return item;
    }

    public override void Pause()
    {
        if (!GameRunning) return;
        base.Pause();
        backgroundMusicAudioSource.Pause();
        uiManager.ShowScreen(Screens.Pause, false, true);
    }

    public override void Resume()
    {
        uiManager.ShowScreen(Screens.Gameplay, true, false);
        State.Value = GameplayState.Transitioning;
        backgroundMusicAudioSource.Play();
        countDown.StartCount().OnComplete(() => { base.Resume(); });
    }
    WaitForSeconds waitBeforeRelease = new WaitForSeconds(0.25f);
    protected override Item GenerateItem()
    {
        ColorItem item = base.GenerateItem() as ColorItem;
        ballHighlighterRenderer.color = ColorItemUitilites.IntIdToColor(item.ItemId);
        ballHighlighterAnimator.SetTrigger("Highlight");
        item.Freeze();
        StartCoroutine(ReleaseItem(waitBeforeRelease, item));
        //generationSoundEffect.Play();
        
        //@Testing
        if (AutoPlay)
        {
            StartCoroutine(OpenGate(itemsGates[item.ItemId]));
        }
        //End of Testing code
        return item;
    }

    IEnumerator ReleaseItem(WaitForSeconds wait, ColorItem item)
    {
        yield return wait;
        item.Release();
    }

    public override void EndGame()
    {
        base.EndGame();
        backgroundMusicAudioSource.Stop();
        StopAllCoroutines();
    }
    protected override void GameOver()
    {
        base.GameOver();
        StopAllCoroutines();
    }
    protected override void OnItemEnteredWrongGate(Item item)
    {
        AudioSource.PlayClipAtPoint(wrongItemSoundEffect, backgroundMusicAudioSource.transform.position);
        if (lives.Value > 0)
        {
            ColorItem colorItem = item as ColorItem;
            colorItem.Freeze();
            colorItem.Decay();
        }
        base.OnItemEnteredWrongGate(item);
    }

    #region Testing
    //@Testing

    public void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
    }
    IEnumerator OpenGate(int gateIndex)
    {
        yield return new WaitForSeconds(0.95f);
        gatesSwitch.OpenGate(gateIndex);
    }
    public bool AutoPlay { get; set; }
    //End of Testing code
    #endregion
}
