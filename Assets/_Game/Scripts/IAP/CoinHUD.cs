using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị số coin của ví bền vững (<see cref="CoinWallet"/>).
/// Port từ PointsHUD của game Terra.
///
/// Giữ nguyên chữ đặt sẵn trong scene làm format string, nên đặt text là
/// "{0}" hay "Coin: {0}" đều được; không có "{0}" thì mặc định chỉ hiện số.
/// </summary>
public class CoinHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinText;

    private string _format;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        CoinWallet.OnCoinsChanged += OnCoinsChanged;

        Refresh();
    }

    private void OnDisable()
    {
        CoinWallet.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int coins)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_coinText == null) return;

        if (string.IsNullOrEmpty(_format))
        {
            _format = _coinText.text;

            if (string.IsNullOrEmpty(_format) || !_format.Contains("{0}"))
                _format = "{0}";
        }

        _coinText.text = string.Format(_format, CoinWallet.Coins);
    }
}
