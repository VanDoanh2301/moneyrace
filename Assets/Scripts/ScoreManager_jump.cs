using UnityEngine;
using UnityEngine.UI;

public class ScoreManager_jump : MonoBehaviour
{
    public Text currentScoreLabel, highScoreLabel, currentScoreGameOverLabel, highScoreGameOverLabel;

    int currentScore, highScore;
    // Start is called before the first frame update

    //init and load highscore
    void Start()
    {
        if (!PlayerPrefs.HasKey("HighScore_PJump"))
            PlayerPrefs.SetInt("HighScore_PJump", 0);

        highScore = PlayerPrefs.GetInt("HighScore_PJump");

        UpdateHighScore();
        ResetCurrentScore();
    }

    //save and update highscore
    void UpdateHighScore()
    {
        if (currentScore > highScore)
            highScore = currentScore;

        highScoreLabel.text = highScore.ToString();
        PlayerPrefs.SetInt("HighScore_PJump", highScore);
    }

    //update currentscore
    public void UpdateScore(int value)
    {
        currentScore += value;
        currentScoreLabel.text = currentScore.ToString();
    }

    //reset current score
    public void ResetCurrentScore()
    {
        currentScore = 0;
        UpdateScore(0);
    }

    //update gameover scores
    public void UpdateScoreGameover()
    {
        UpdateHighScore();

        currentScoreGameOverLabel.text = currentScore.ToString();
        highScoreGameOverLabel.text = highScore.ToString();
        //AdManager.instance.show_ads_ingames();

    }
}
