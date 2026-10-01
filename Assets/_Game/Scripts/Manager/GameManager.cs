using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Object References :")]
    [SerializeField] Transform _base;
    [SerializeField] PlayerBehaviour _player;

    float _lastZpos = 0;
    bool _isGameOver = false;
    bool _isRevive = false;

    /// <summary>Lượt chơi đã được chốt thưởng chưa — chặn cộng coin hai lần.</summary>
    bool _isRunSettled = false;

    MenuManager _menuController;
    ScoreManager _scoreManager;

    public static event Action OnGameEnd;
    public static event Action OnRevive;
    public static event Action<int> OnScoreUpdated;

    private void Awake()
    {
        _scoreManager = GetComponent<ScoreManager>();
    }

    private void OnEnable()
    {
        Piece.OnGameOver += GameEnd;
        Piece.OnLastPieceExit += UpdateLastPos;
        Piece.OnGettingScore += SetScore;

        PlayerBehaviour.OnPlayerDeath += GameEnd;
        PlayerBehaviour.OnFirstJump += StartGameplay;

        ReviveMenu.OnReviveRequested += Revive;
        ReviveMenu.OnReviveDeclined += GiveUp;
    }

    private void SetScore(int val)
    {
        _scoreManager.AddScore(val);
        OnScoreUpdated?.Invoke(_scoreManager.Score);
    }

    private void Start()
    {
        _menuController = MenuManager.GetInstance();
    }

    public void GameEnd()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        OnGameEnd?.Invoke();

        _player.GameOver();

        // Chỉ mời hồi sinh khi người chơi thật sự trả được — không hiện panel vô dụng.
        bool canRevive = !_isRevive
                         && _scoreManager.Score > 2
                         && CoinWallet.Coins >= EconomyConfig.Instance.ReviveCost;

        if (canRevive)
        {
            _menuController.SwitchMenu(MenuType.Revive);
            _isRevive = true;
        }
        else
        {
            SettleRun();
            _menuController.SwitchMenu(MenuType.GameOver);
        }

        SoundController.GetInstance().PlayAudio(AudioType.GAMEOVER);
    }

    /// <summary>
    /// Chốt thưởng cuối lượt: lưu kỷ lục và quy đổi điểm sang coin.
    ///
    /// Phải gọi đúng một lần cho mỗi lượt. Không thể đặt trong <see cref="GameEnd"/>
    /// vì hàm đó còn chạy ở lần chết mở panel hồi sinh, và điểm thì cộng dồn qua lần
    /// hồi sinh — đặt ở đó sẽ cộng coin hai lần.
    /// </summary>
    public void SettleRun()
    {
        if (_isRunSettled) return;
        _isRunSettled = true;

        _scoreManager.CommitBestScore();
        ScoreRewardService.GrantScore(_scoreManager.Score, true);
    }

    /// <summary>Người chơi bỏ hồi sinh (bấm Skip hoặc hết thời gian).</summary>
    public void GiveUp()
    {
        SettleRun();
        _menuController.SwitchMenu(MenuType.GameOver);
    }

    /// <summary>
    /// Hồi sinh và chơi tiếp. Coin đã được <see cref="ReviveMenu"/> trừ trước khi gọi tới đây.
    /// </summary>
    private void Revive()
    {
        if (_isRunSettled)
        {
            Debug.LogError("Hồi sinh sau khi đã chốt thưởng lượt — luồng game sai.");
            return;
        }

        _isGameOver = false;
        OnRevive?.Invoke();

        _menuController.SwitchMenu(MenuType.Gameplay);

        Vector3 revivePosition = Vector3.forward * _lastZpos;

        _base.position = revivePosition;
        _player.Revive(revivePosition + Vector3.up);
    }

    private void UpdateLastPos(Vector3 lastPos)
    {
        _lastZpos = lastPos.z;
    }

    public void StartGameplay()
    {
        if (_menuController.GetCurrentMenu != MenuType.Gameplay)
        {
            _menuController.SwitchMenu(MenuType.Gameplay);
        }
    }

    private void OnDisable()
    {
        Piece.OnGameOver -= GameEnd;
        Piece.OnLastPieceExit -= UpdateLastPos;
        Piece.OnGettingScore -= SetScore;

        PlayerBehaviour.OnPlayerDeath -= GameEnd;
        PlayerBehaviour.OnFirstJump -= StartGameplay;

        ReviveMenu.OnReviveRequested -= Revive;
        ReviveMenu.OnReviveDeclined -= GiveUp;
    }
}
