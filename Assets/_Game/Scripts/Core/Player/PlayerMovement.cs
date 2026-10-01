using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _jumpTime = .2f;
    [SerializeField] float _gap = 3f;

    [Space]
    [SerializeField] Vector3 _gravity;

    float _elapsedTime = 0;
    float _startVal, _endVal;

    Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Physics.gravity = _gravity;
    }

    private void Update()
    {
        // calculate percentage
        _elapsedTime += Time.deltaTime;
        float timePercentage = _elapsedTime / _jumpTime;

        Move(timePercentage);
    }

    public void Jump()
    {
        _elapsedTime = 0;
        _startVal = transform.position.z;
        _endVal += _gap;

        Vector3 newPos = transform.position;
        newPos.y = 0f;
        transform.position = newPos;
    }

    public void Revive(Vector3 position)
    {
        _endVal -= _gap;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        transform.position = new Vector3(position.x, position.y, _endVal);
    }

    public void EnableKinematic()
    {
        _rb.isKinematic = true;
    }

    public void DisableKinematic()
    {
        _rb.isKinematic = false;
    }

    private void Move(float percentage)
    {
        float zPos = Mathf.Lerp(_startVal, _endVal, percentage);
        transform.position = new Vector3(transform.position.x, transform.position.y, zPos);
    }
}
