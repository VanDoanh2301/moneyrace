using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    public Transform _player;
    [SerializeField]
    float _moveSpeedDefault = 5f;
    [SerializeField]
    float _moveMaxX = 5f;
    [SerializeField]
    float _sensivity = 1f;
    bool _isPress;
    Vector3 _preMosePos, _startMosePos, _curentMosePos, _startPlayerPos;
    float _swipeScale;
    void Start()
    {
        SetDefaultSettingGame();
    }
    void SetDefaultSettingGame()
    {
        _swipeScale = _moveSpeedDefault / _sensivity;
    }
    void Update()
    {
        if (!_isPress)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isPress = true;
                //_preMosePos = _StartMosePos = Input.mousePosition;
                _preMosePos = _startMosePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                _startPlayerPos = _player.localPosition;
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isPress = false;
            }
            else if (Input.GetMouseButton(0))
            {
                //_curentMosePos = Input.mousePosition;
                _curentMosePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                if (_curentMosePos.x != _preMosePos.x)
                {
                    Movement(_curentMosePos.x - _startMosePos.x);
                    _preMosePos = _curentMosePos;
                }
            }

        }
       
    }
    public void Movement(float distance)
    {
        _player.localPosition = new Vector3(Mathf.Clamp(distance * _swipeScale + _startPlayerPos.x, -_moveMaxX, _moveMaxX), 0, 0);
    }
}
