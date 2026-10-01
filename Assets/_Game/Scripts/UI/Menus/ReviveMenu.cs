using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel mời hồi sinh. Trước đây hồi sinh bằng cách xem quảng cáo; giờ trả coin.
///
/// Menu tự trừ coin rồi mới phát sự kiện, vì <see cref="CoinWallet.TrySpend"/> là
/// thao tác nguyên tử — phải biết kết quả TRƯỚC khi chuyển màn, nếu không sẽ kẹt
/// như bản cũ (tắt nút, dừng timer, rồi không đi đâu cả).
/// </summary>
public class ReviveMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] Button _continueButton;
    [SerializeField] Button _skipButton;

    [SerializeField] TMP_Text _timerText;
    [SerializeField] Image _timerFill;

    [Header("Giá hồi sinh :")]
    [SerializeField] TMP_Text _costText;
    [Tooltip("Dòng chữ 'không đủ coin', bật lên khi trả không nổi.")]
    [SerializeField] GameObject _notEnoughRoot;

    /// <summary>Đã trả coin xong, xin hồi sinh.</summary>
    public static event Action OnReviveRequested;

    /// <summary>Bỏ hồi sinh — bấm Skip hoặc hết thời gian.</summary>
    public static event Action OnReviveDeclined;

    private Timer _timer;

    protected override void Awake()
    {
        base.Awake();

        _timer = GetComponent<Timer>();
    }

    public override void SetEnable()
    {
        base.SetEnable();

        int cost = EconomyConfig.Instance.ReviveCost;

        if (_costText != null)
            _costText.text = cost.ToString();

        bool canAfford = CoinWallet.Coins >= cost;

        _continueButton.interactable = canAfford;
        _skipButton.interactable = true;

        if (_notEnoughRoot != null)
            _notEnoughRoot.SetActive(!canAfford);

        // Timer luôn chạy, kể cả khi không đủ coin — đảm bảo luồng luôn kết thúc ở GameOver.
        _timer.PlayTimer(i => _timerText.text = i, j => _timerFill.fillAmount = j, OnTimerExpired);

        if (canAfford)
            LeanTween.scale(_continueButton.gameObject, Vector2.one * 1.1f, .3f)
                     .setEase(LeanTweenType.easeOutQuad).setLoopPingPong();
    }

    private void Start()
    {
        OnButtonPressed(_continueButton, ContinueButtonPressed);
        OnButtonPressed(_skipButton, SkipButtonPressed);
    }

    private void OnDisable()
    {
        // Tween chạy trên nút thuộc canvas đã tắt vẫn sống nếu không huỷ.
        StopPulse();
    }

    private void SkipButtonPressed()
    {
        _skipButton.interactable = false;
        _continueButton.interactable = false;
        StopPulse();

        _timer.StopTimer();

        OnReviveDeclined?.Invoke();
    }

    private void ContinueButtonPressed()
    {
        int cost = EconomyConfig.Instance.ReviveCost;

        if (!CoinWallet.TrySpend(cost))
        {
            // Không đủ coin: giữ nguyên nút và timer, người chơi vẫn bấm Skip được.
            if (_notEnoughRoot != null)
                _notEnoughRoot.SetActive(true);

            return;
        }

        _continueButton.interactable = false;
        _skipButton.interactable = false;
        StopPulse();

        _timer.StopTimer();

        OnReviveRequested?.Invoke();
    }

    private void StopPulse()
    {
        LeanTween.cancel(_continueButton.gameObject);
        _continueButton.transform.localScale = Vector3.one;
    }

    private void OnTimerExpired()
    {
        StopPulse();

        OnReviveDeclined?.Invoke();
    }
}
