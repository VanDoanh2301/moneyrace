using UnityEngine;
using deVoid.Utils;

public static class LocalDataStorage 
{
    const string HIGH_SCORE_KEY = "HIGH_SCORE";
    static int highScore = int.MinValue;
    public static int HighScore
    {
        get
        {
            if(highScore<0)
            {
                highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            }
            return highScore;
        }
        set
        {
            if (value > highScore)
            {
                highScore = value;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                Signals.Get<NewHighScoreSignal>().Dispatch(highScore);
            }
        }
    }
}

public class NewHighScoreSignal:ASignal<int>{ }