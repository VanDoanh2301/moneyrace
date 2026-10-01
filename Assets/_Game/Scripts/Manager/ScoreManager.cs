using UnityEngine;

/// <summary>
/// Điểm của lượt chơi hiện tại. Reset theo mỗi lần load lại scene.
/// Tổng điểm tích luỹ nằm ở <see cref="PointsWallet"/>.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string PPK_BEST_SCORE = "BestScore";

    public int Score { get; private set; }

    /// <summary>Điểm cao nhất đã lưu. Chỉ đọc, không ghi gì.</summary>
    public int BestScore => PlayerPrefs.GetInt(PPK_BEST_SCORE, 0);

    public void AddScore(int score)
    {
        Score += score;
    }

    public void ResetScore()
    {
        Score = 0;
    }

    /// <summary>Lưu điểm lượt này thành điểm cao nhất nếu vượt kỷ lục.</summary>
    public void CommitBestScore()
    {
        if (Score <= BestScore) return;

        PlayerPrefs.SetInt(PPK_BEST_SCORE, Score);
        PlayerPrefs.Save();
    }
}
