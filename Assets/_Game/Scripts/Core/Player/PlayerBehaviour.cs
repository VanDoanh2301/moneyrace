using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    PlayerMovement _playerMovement;
    PlayerAnimation _playerAnimation;
    PlayerCollision _playerCollision;

    bool _isPlayerDeath = false;
    bool _isFirstJump = true;

    public static event Action OnFirstJump;
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerCollision = GetComponent<PlayerCollision>();
    }

    public void Revive(Vector3 position)
    {
        _playerMovement.Revive(position);

        _playerCollision.CanJump = true;
        _isPlayerDeath = false;
    }

    public void OnJump()
    {
        if (_isPlayerDeath || !_playerCollision.CanJump) return;

        if (_isFirstJump)
        {
            _isFirstJump = false;
            OnFirstJump?.Invoke();
        }

        _playerMovement.Jump();
        _playerAnimation.Jump();

        SoundController.GetInstance().PlayAudio(AudioType.JUMP);
    }

    public void GameOver()
    {
        if (_isPlayerDeath) return;
        _isPlayerDeath = true;

        OnPlayerDeath?.Invoke();
    }
}
