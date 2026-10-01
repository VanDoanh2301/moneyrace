using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Một con số nguyên bền vững trong PlayerPrefs, nạp lười và phát sự kiện khi đổi.
/// Dùng làm ruột cho các ví tiền tệ (<see cref="CoinWallet"/>, <see cref="PointsWallet"/>).
/// </summary>
public class PrefsCounter
{
    private readonly string _key;

    private bool _isLoaded;
    private int _value;

    /// <summary>Giá trị vừa thay đổi (giá trị mới).</summary>
    public UnityAction<int> OnChanged;

    public PrefsCounter(string key)
    {
        _key = key;
    }

    public int Value
    {
        get
        {
            if (!_isLoaded)
            {
                _value = PlayerPrefs.GetInt(_key, 0);
                _isLoaded = true;
            }

            return _value;
        }
    }

    /// <param name="flush">
    /// true = ghi thẳng xuống đĩa ngay (dùng cho giao dịch tiền thật).
    /// false = chỉ ghi vào PlayerPrefs in-memory, gọi <see cref="Flush"/> một lần khi kết thúc lượt.
    /// </param>
    public void Add(int amount, bool flush = false)
    {
        if (amount <= 0) return;

        Save(Value + amount, flush);
    }

    /// <summary>Trừ nếu đủ. Trả về false nếu không đủ (không trừ gì cả).</summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0 || Value < amount) return false;

        Save(Value - amount, true);

        return true;
    }

    public void Set(int value)
    {
        Save(value < 0 ? 0 : value, true);
    }

    /// <summary>Ép ghi xuống đĩa.</summary>
    public void Flush()
    {
        PlayerPrefs.Save();
    }

    private void Save(int value, bool flush)
    {
        _value = value;
        _isLoaded = true;

        PlayerPrefs.SetInt(_key, _value);

        if (flush)
            PlayerPrefs.Save();

        OnChanged?.Invoke(_value);
    }
}
