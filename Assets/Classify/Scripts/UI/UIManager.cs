using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This calss handle all the game screen and the transition between them
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// A reference for all the UI screens in the game
    /// To add a new screen, create a script that inherits UIScreen and add its reference to this array 
    /// </summary>
    public UIScreen[] screens;
    Dictionary<Screens, UIScreen> screensDictionary;

    void Awake()
    {
        screensDictionary = new Dictionary<Screens, UIScreen>();
        for (int i = 0; i < screens.Length; i++)
            screensDictionary.Add(screens[i].screenId, screens[i]);
    }
    void Start()
    {
        CurrentScreen = startingScreen;
        screensDictionary[CurrentScreen].Show();
    }
    public Screens CurrentScreen
    {
        get;private set;
    }

    [SerializeField] Screens startingScreen;

    /// <summary>
    /// Call this function to switch screens
    /// </summary>
    /// <param name="screenId"> the new screen to be shown</param>
    /// <param name="hidePreviousScreen">if the current shown screen should get hidden</param>
    /// <param name="playShowTween">wheither to play show tween of the new screen or not</param>
    public void ShowScreen(Screens screenId, bool hidePreviousScreen = true, bool playShowTween = true)
    {
        if (screenId == CurrentScreen) return;
        if (hidePreviousScreen)
            screensDictionary[CurrentScreen].Hide();
        CurrentScreen = screenId;

        screensDictionary[CurrentScreen].Show(playShowTween);
    }
    


}
