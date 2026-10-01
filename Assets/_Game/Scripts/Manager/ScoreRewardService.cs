using System;

/// <summary>
/// Phễu duy nhất biến điểm thành phần thưởng: cộng vào tổng điểm tích luỹ
/// (<see cref="PointsWallet"/>) rồi quy đổi ngay sang coin (<see cref="CoinWallet"/>).
///
/// Mọi nguồn điểm đều đi qua đây, nên tỉ lệ quy đổi luôn nhất quán:
///  - mua gói IAP  -> <see cref="IAPManager.ProcessPurchase"/>
///  - hết một lượt -> <see cref="GameManager.SettleRun"/>
///
/// Không chỗ nào khác được gọi <see cref="CoinWallet.Add"/> trực tiếp.
/// Tỉ lệ đặt ở <see cref="EconomyConfig"/>.
/// </summary>
public static class ScoreRewardService
{
    public static int CoinPerScore => EconomyConfig.Instance.CoinPerScore;

    /// <summary>Điểm vừa được quy đổi: (số điểm, số coin nhận được).</summary>
    public static event Action<int, int> OnScoreConverted;

    public static int ToCoins(int score) => score <= 0 ? 0 : score * CoinPerScore;

    /// <summary>
    /// Cộng điểm và quy đổi sang coin. Trả về số coin đã cộng.
    /// </summary>
    /// <param name="flush">true cho giao dịch tiền thật, xem <see cref="PrefsCounter.Add"/>.</param>
    public static int GrantScore(int score, bool flush)
    {
        if (score <= 0) return 0;

        int coins = ToCoins(score);

        PointsWallet.Add(score, flush);
        CoinWallet.Add(coins, flush);

        OnScoreConverted?.Invoke(score, coins);

        return coins;
    }
}
