using UnityEngine;

/// <summary>
/// Cộng coin kiếm được khi chơi vào <see cref="PointsWallet"/> — cùng một số dư với
/// coin mua bằng IAP. Thay cho PointsScorer_tetris của game Tetra Rush (vốn tính điểm
/// theo block/level của Tetris).
///
/// Luật của Ball: +<see cref="coinsPerBall"/> mỗi ball ăn được, và +floor(score) khi hết lượt.
/// </summary>
public class CoinScorer : MonoBehaviour
{
    public static CoinScorer Instance { get; private set; }

    [Header("Coin thưởng")]
    [Tooltip("Coin cho mỗi ball cùng màu ăn được.")]
    [SerializeField]
    private int coinsPerBall = 1;

    /// <summary>Coin kiếm được trong lượt chơi hiện tại (đã cộng vào ví).</summary>
    public int RunCoins { get; private set; }

    /// <summary>
    /// Chặn chốt lượt hai lần. GameManager.GameOver() có thể bị gọi nhiều lần nếu hai
    /// obstacle sai màu chạm player trong cùng một frame (OnTriggerEnter đã kịp fire
    /// trước khi player.SetActive(false) chặn được) — không có cờ này thì floor(score)
    /// bị cộng đôi.
    /// </summary>
    private bool _runEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            UnityEngine.Object.Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>Bắt đầu một lượt chơi mới — reset coin của lượt (số dư trong ví giữ nguyên).</summary>
    public void BeginRun()
    {
        RunCoins = 0;
        _runEnded = false;
    }

    /// <summary>Vừa ăn một ball cùng màu.</summary>
    public void AddBallCollected()
    {
        Award(coinsPerBall);
    }

    /// <summary>
    /// Chốt lượt chơi: cộng thêm floor(score) rồi ghi xuống đĩa đúng một lần mỗi lượt,
    /// thay vì mỗi lần cộng coin.
    /// </summary>
    public void EndRun()
    {
        if (_runEnded) return;
        _runEnded = true;

        if (ScoreManager.Instance != null)
        {
            Award(Mathf.FloorToInt(ScoreManager.Instance.currentScore));
        }

        PointsWallet.Flush();
    }

    private void Award(int amount)
    {
        if (amount <= 0) return;

        RunCoins += amount;
        PointsWallet.Add(amount, false);
    }
}
