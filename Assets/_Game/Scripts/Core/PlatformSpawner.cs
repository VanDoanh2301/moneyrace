using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    const float _gapFromLastPlatformToTarget = 30f;

    [SerializeField] float _gap;
    [Range(.01f,1f)]
    [SerializeField] float _difficultyIncrease = .02f;
    [SerializeField] Platform _platformPrefab;
    [SerializeField] Transform _target;

    bool _invertPlatform;
    Vector3 _lastPlatformPosition;

    int _platformCount = 0;

    float DifficultyValue => _platformCount * _difficultyIncrease;

    private void Start()
    {
        _lastPlatformPosition = Vector3.forward * _gap;
    }

    private void Update()
    {
        if (Vector3.SqrMagnitude(_lastPlatformPosition - _target.position) < _gapFromLastPlatformToTarget * _gapFromLastPlatformToTarget)
        {
            SpawnPlatform(_lastPlatformPosition, _platformPrefab);
        }   
    }

    private void SpawnPlatform(Vector3 position, Platform prefab)
    {
        Platform newPlatform = Instantiate(prefab, transform);
        newPlatform.transform.position = position;

        // calculate difficulty
        float difficulty = Mathf.Clamp(DifficultyValue, 0f, 5f);
        newPlatform.CalculateRandomSpeed(difficulty);

        newPlatform.InvertPos = _invertPlatform;
        newPlatform.gameObject.SetActive(true);

        // update last platform position z
        _lastPlatformPosition.z += _gap;

        _invertPlatform = !_invertPlatform;

        _platformCount++;
    }
}
