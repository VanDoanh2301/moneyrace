using UnityEngine;
using UnityEngine.UI;

public class ScoreManager_jump : MonoBehaviour
{
    private const string PPK_HIGHSCORE = "HighScore_PJump";

    // Instance hiện có trong scene (nếu có) để đồng bộ UI khi highScore đổi từ bên ngoài (vd: mua coin ở Shop).
    private static ScoreManager_jump s_Instance;

    public Text currentScoreLabel, highScoreLabel, currentScoreGameOverLabel, highScoreGameOverLabel;

    int currentScore, highScore;
    // Start is called before the first frame update

    //init and load highscore
    void Start()
    {
        s_Instance = this;

        if (!PlayerPrefs.HasKey(PPK_HIGHSCORE))
            PlayerPrefs.SetInt(PPK_HIGHSCORE, 0);

        highScore = PlayerPrefs.GetInt(PPK_HIGHSCORE);

        UpdateHighScore();
        ResetCurrentScore();
    }

    void OnDestroy()
    {
        if (s_Instance == this)
            s_Instance = null;
    }

    //save and update highscore
    void UpdateHighScore()
    {
        if (currentScore > highScore)
            highScore = currentScore;

        highScoreLabel.text = highScore.ToString();
        PlayerPrefs.SetInt(PPK_HIGHSCORE, highScore);
    }

    /// <summary>
    /// Cộng thêm điểm vào HighScore và lưu ngay xuống đĩa.
    /// Dùng khi mua coin ở Shop (IAP) để số coin mua được cũng cập nhật vào điểm.
    /// </summary>
    public static void AddHighScore(int amount)
    {
        if (amount <= 0) return;

        int highScore = PlayerPrefs.GetInt(PPK_HIGHSCORE, 0) + amount;
        PlayerPrefs.SetInt(PPK_HIGHSCORE, highScore);
        PlayerPrefs.Save();

        if (s_Instance != null)
        {
            s_Instance.highScore = highScore;

            if (s_Instance.highScoreLabel != null)
                s_Instance.highScoreLabel.text = highScore.ToString();

            if (s_Instance.highScoreGameOverLabel != null)
                s_Instance.highScoreGameOverLabel.text = highScore.ToString();
        }
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
        CoinWallet.Add(currentScore);

        currentScoreGameOverLabel.text = currentScore.ToString();
        highScoreGameOverLabel.text = highScore.ToString();
        //AdManager.instance.show_ads_ingames();

    }
}
