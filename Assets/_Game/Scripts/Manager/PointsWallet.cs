using UnityEngine.Events;

/// <summary>
/// Tổng điểm tích luỹ bền vững — gồm điểm kiếm được khi chơi và điểm mua bằng IAP.
/// Khác với <see cref="ScoreManager.Score"/> (điểm của riêng lượt chơi hiện tại).
/// Port từ PointsWallet của game Terra.
/// </summary>
public static class PointsWallet
{
    private const string PPK_POINTS = "Points";

    private static readonly PrefsCounter _counter = new PrefsCounter(PPK_POINTS);

    /// <summary>Số điểm thay đổi (giá trị mới).</summary>
    public static UnityAction<int> OnPointsChanged
    {
        get => _counter.OnChanged;
        set => _counter.OnChanged = value;
    }

    public static int Points => _counter.Value;

    public static void Add(int amount, bool flush = false) => _counter.Add(amount, flush);

    public static bool TrySpend(int amount) => _counter.TrySpend(amount);

    public static void SetPoints(int value) => _counter.Set(value);

    public static void Flush() => _counter.Flush();
}
