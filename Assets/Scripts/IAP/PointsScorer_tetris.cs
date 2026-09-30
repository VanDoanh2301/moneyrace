using UnityEngine;

/// <summary>
/// Cộng điểm khi chơi vào <see cref="PointsWallet"/> — cùng một số dư với điểm mua bằng IAP.
/// Đặt trên GameObject Canvas, cạnh <see cref="UIController_tetris"/>.
/// </summary>
public class PointsScorer_tetris : MonoBehaviour
{
    public static PointsScorer_tetris Instance { get; private set; }

    [Header("Điểm thưởng")]
    [Tooltip("Điểm cho mỗi khối hạ xuống thành công (không giết player).")]
    [SerializeField]
    private int pointsPerBlock = 10;

    [Tooltip("Điểm thưởng khi qua màn, nhân với số level hiện tại.")]
    [SerializeField]
    private int levelBonus = 100;

    /// <summary>Điểm kiếm được trong lượt chơi hiện tại (đã cộng vào ví).</summary>
    public int RunPoints { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Bắt đầu một lượt chơi mới — reset điểm của lượt (số dư trong ví giữ nguyên).</summary>
    public void beginRun()
    {
        RunPoints = 0;
    }

    /// <summary>Một khối vừa hạ xuống thành công.</summary>
    public void addBlockPlaced()
    {
        award(pointsPerBlock);
    }

    /// <summary>
    /// Chốt lượt chơi: cộng thưởng qua màn rồi ghi xuống đĩa.
    /// Cố tình KHÔNG tự nghe endGameEvent — <see cref="UIController_tetris"/> gọi hàm này
    /// trước khi đọc <see cref="RunPoints"/>, nếu không kết quả sẽ phụ thuộc vào
    /// thứ tự subscribe của hai component và panel thắng sẽ thiếu điểm thưởng.
    /// </summary>
    public void endRun(bool isGameSuccess)
    {
        if (isGameSuccess)
        {
            int level = PlayerPrefs.GetInt("Level_tetris", 1);
            award(levelBonus * level);
        }

        // Ghi xuống đĩa đúng một lần mỗi lượt, thay vì mỗi lần cộng điểm.
        PointsWallet.Flush();
    }

    private void award(int amount)
    {
        if (amount <= 0) return;

        RunPoints += amount;
        PointsWallet.Add(amount, false);
    }
}
