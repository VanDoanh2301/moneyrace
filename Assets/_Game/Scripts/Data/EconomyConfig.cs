using UnityEngine;

/// <summary>
/// Các con số kinh tế của game, tinh chỉnh được trong Inspector mà không cần sửa code.
/// Asset nằm ở "Assets/Resources/Economy Config.asset" — phải ở trong Resources vì
/// <see cref="IAPManager"/> có thể cộng thưởng trước khi có bất kỳ object nào trong scene.
/// </summary>
[CreateAssetMenu(menuName = "Database/Economy Config")]
public class EconomyConfig : ScriptableObject
{
    public const string ResourcePath = "Economy Config";

    [Header("Quy đổi :")]
    [Tooltip("1 điểm đổi được bao nhiêu coin.")]
    [SerializeField] [Min(1)] int _coinPerScore = 10;

    [Header("Giá :")]
    [Tooltip("Số coin phải trả để hồi sinh.")]
    [SerializeField] [Min(0)] int _reviveCost = 25;

    public int CoinPerScore => Mathf.Max(1, _coinPerScore);
    public int ReviveCost => Mathf.Max(0, _reviveCost);

    private static EconomyConfig _instance;

    public static EconomyConfig Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = Resources.Load<EconomyConfig>(ResourcePath);

            if (_instance == null)
            {
                Debug.LogWarning($"Không tìm thấy '{ResourcePath}' trong Resources. " +
                                 "Dùng giá trị mặc định — tạo asset qua menu Create > Database > Economy Config.");

                _instance = CreateInstance<EconomyConfig>();
            }

            return _instance;
        }
    }
}
