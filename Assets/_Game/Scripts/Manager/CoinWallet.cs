using UnityEngine.Events;

/// <summary>
/// Ví coin bền vững của người chơi — thứ người chơi tiêu (hồi sinh, ...).
/// Coin chỉ sinh ra từ <see cref="ScoreRewardService"/>: điểm chơi được và điểm mua bằng IAP
/// đều quy đổi qua đó, không nơi nào khác được cộng coin trực tiếp.
/// </summary>
public static class CoinWallet
{
    private const string PPK_COINS = "Coins";

    private static readonly PrefsCounter _counter = new PrefsCounter(PPK_COINS);

    /// <summary>Số coin thay đổi (giá trị mới).</summary>
    public static UnityAction<int> OnCoinsChanged
    {
        get => _counter.OnChanged;
        set => _counter.OnChanged = value;
    }

    public static int Coins => _counter.Value;

    public static void Add(int amount, bool flush = false) => _counter.Add(amount, flush);

    /// <summary>Trừ coin nếu đủ. Trả về false nếu không đủ.</summary>
    public static bool TrySpend(int amount) => _counter.TrySpend(amount);

    public static void SetCoins(int value) => _counter.Set(value);

    public static void Flush() => _counter.Flush();
}
