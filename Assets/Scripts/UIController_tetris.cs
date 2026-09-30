using TMPro;
using UnityEngine;

public class UIController_tetris : MonoBehaviour
{
    [Header("End Game")]
    [SerializeField]
    private GameObject endGameUI;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI nextLevelBtnText;

    [SerializeField]
    private GameObject endGameFailUI;
    [SerializeField]
    private TextMeshProUGUI endGameFailText;

    [SerializeField]
    private GameObject gameStartUI;

    public GameObject panel_loading;

    [Header("Điểm")]
    [SerializeField]
    private PointsScorer_tetris scorer;
    [SerializeField]
    private TextMeshProUGUI startPointsText;
    [SerializeField]
    private TextMeshProUGUI endSuccessPointsText;
    [SerializeField]
    private TextMeshProUGUI endFailPointsText;

    private void Start()
    {
        levelText.text = "Level " + PlayerPrefs.GetInt("Level_tetris", 1) + " Success!";
        nextLevelBtnText.text = "GO Level " + (PlayerPrefs.GetInt("Level_tetris", 1) + 1) + " !";
        endGameUI.SetActive(false);

        endGameFailText.text = "Level " + PlayerPrefs.GetInt("Level_tetris", 1) + " FAILED!";
        endGameFailUI.SetActive(false);

        if (startPointsText != null)
            startPointsText.text = "Points: " + PointsWallet.Points;

        if (PlayerPrefs.GetInt("IsFirstTimePlay_tetris", 1) == 0)
            gameStartUI.SetActive(false);

    }

    private void OnEnable()
    {
        if(GameObject.FindGameObjectWithTag("Player")!=null)
          GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_tetris>().endGameEvent += showEndGameUI;
    }
    private void OnDisable()
    {
        if(GameObject.FindGameObjectWithTag("Player")!=null)
          GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_tetris>().endGameEvent -= showEndGameUI;
    }
    public void showEndGameUI(object sender, PlayerController_tetris.endGameEventArgs e)
    {
        int level = PlayerPrefs.GetInt("Level_tetris", 1);

        // Chốt điểm TRƯỚC khi đọc RunPoints, để panel thắng gồm cả thưởng qua màn.
        if (scorer != null) scorer.endRun(e.IsGameSuccess);

        if (e.IsGameSuccess)
        {
            // Đổ lại text theo level hiện tại — Start() chỉ chạy một lần nên sẽ cũ sau vài lượt.
            levelText.text = "Level " + level + " Success!";
            nextLevelBtnText.text = "GO Level " + (level + 1) + " !";
            fillPointsText(endSuccessPointsText);
            endGameUI.SetActive(true);
        }
        else
        {
            endGameFailText.text = "Level " + level + " FAILED!";
            fillPointsText(endFailPointsText);
            endGameFailUI.SetActive(true);
        }
    }

    /// <summary>Điểm kiếm được lượt này + tổng số dư.</summary>
    private void fillPointsText(TextMeshProUGUI target)
    {
        if (target == null) return;

        int run = scorer != null ? scorer.RunPoints : 0;

        target.text = "+" + run + " Points  ·  Total: " + PointsWallet.Points;
    }

    public void startGame()
    {
        gameStartUI.SetActive(false);
        endGameFailUI.SetActive(false);
        PlayerPrefs.SetInt("IsFirstTimePlay_tetris", 0);
        PlayerPrefs.Save();
        if (scorer != null) scorer.beginRun();
        Camera.main.GetComponent<GameController_tetris>().resetGame();

    }
    public void startNextGame()
    {
        endGameUI.SetActive(false);
        // Level_tetris được tăng trong GameController_tetris.restartGame() ngay trước khi load scene.
        // Trước đây tăng ở cả hai chỗ nên level nhảy cách 2.
        if (scorer != null) scorer.beginRun();
        Camera.main.GetComponent<GameController_tetris>().restartGame();
    }

    public void exitGame()
    {
        panel_loading.SetActive(true);
        PlayerPrefs.SetInt("IsFirstTimePlay_tetris", 1);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("IsFirstTimePlay_tetris", 1);
        PlayerPrefs.Save();
    }
}
