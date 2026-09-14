using UnityEngine;
using DG.Tweening;

/// <summary>
/// The base class for all UI screens
/// To add a new screen to the game 
/// 1.Create a script that inherits this class and attach it to the UI elment
/// 2.Add and entry to the Screens enum
/// 3.Assign it to the UImanager screens array
/// </summary>
public abstract class UIScreen : MonoBehaviour
{
    public Screens screenId;
    [SerializeField] Canvas canvas;
    public virtual Tween ShowTweening() { return null; }
    public virtual Tween HideTweening() { return null; }

    public virtual void Show(bool animate = true)
    {
        if (canvas)
            canvas.enabled = true;
        if (animate)
            ShowTweening();
    }

    public virtual void Hide()
    {
        Tween hideTweening = HideTweening();
        if (hideTweening != null)
        {
            if (canvas) hideTweening.OnComplete(() => { canvas.enabled = false; });
        }
        else
            if (canvas) canvas.enabled = false;
    }
}

public enum Screens
{
    MainMenu,
    Gameplay,
    GameOver,
    Pause,
    ExtraLifeRewardedAd,
    Shop
}
