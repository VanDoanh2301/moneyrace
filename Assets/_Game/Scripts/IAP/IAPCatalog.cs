/// <summary>
/// Nguồn sự thật duy nhất cho danh mục gói IAP.
///
/// Bên Terra bảng này nằm ở hai chỗ (IAPManager.GetPointsForProduct và ShopPanelBuilder.Packs)
/// và phải đồng bộ tay; ở đây gộp lại một chỗ để <see cref="IAPManager"/> và
/// ShopMenuBuilder cùng đọc.
///
/// Giá thật đặt trên Google Play Console — code chỉ quy đổi ra điểm.
/// Điểm rồi quy tiếp ra coin theo <see cref="ScoreRewardService.CoinsPerScore"/>.
/// </summary>
public static class IAPCatalog
{
    public struct Pack
    {
        /// <summary>ID sản phẩm, phải trùng Google Play Console.</summary>
        public string ProductId;

        /// <summary>Số điểm nhận được khi mua.</summary>
        public int Points;

        public string DisplayName;

        /// <summary>Chữ giá tạm hiển thị khi store chưa trả về giá nội tệ.</summary>
        public string PricePlaceholder;

        public Pack(string productId, int points, string displayName, string pricePlaceholder)
        {
            ProductId = productId;
            Points = points;
            DisplayName = displayName;
            PricePlaceholder = pricePlaceholder;
        }

        /// <summary>Số coin người chơi thực nhận.</summary>
        public int Coins => ScoreRewardService.ToCoins(Points);
    }

    public static readonly Pack[] Packs =
    {
        new Pack("iap1",  100, "Starter Stack", "$0.50"),
        new Pack("iap2",  200, "Small Stack",   "$1"),
        new Pack("iap3",  400, "Block Stack",   "$2"),
        new Pack("iap4",  600, "Big Stack",     "$3"),
        new Pack("iap5", 1000, "Mega Stack",    "$5"),
        new Pack("iap6", 2000, "Turbo Stack",   "$7"),
        new Pack("iap7", 5000, "Ultra Stack",   "$10"),
    };

    /// <summary>Số điểm tương ứng một product id (0 nếu không phải gói điểm).</summary>
    public static int GetPoints(string productId)
    {
        for (int i = 0; i < Packs.Length; i++)
        {
            if (Packs[i].ProductId == productId)
                return Packs[i].Points;
        }

        return 0;
    }
}
