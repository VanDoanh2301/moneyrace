using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] int _scoreValue = 1;

    [Space]
    [SerializeField] Collider col;
    [SerializeField] MeshRenderer _renderer;
    [SerializeField] GameData _data;

    [field: SerializeField]
    public float HalfWidth { get; private set; }

    bool _isGameOver = false;

    public static event Action OnGameOver;
    public static event Action<Vector3> OnLastPieceExit;
    public static event Action<int> OnGettingScore;

    private void OnEnable()
    {
        GameManager.OnRevive += Revive;
    }

    private void OnDisable()
    {
        GameManager.OnRevive -= Revive;
    }

    private void Start()
    {
        _renderer.material = _data.GetRandomMaterial;
    }

    private void Revive()
    {
        _isGameOver = false;
    }

    private void OnCollisionEnter(Collision c)
    {
        float xDistanceToTarget = Mathf.Abs(c.transform.position.x - transform.position.x);
        if (xDistanceToTarget >= col.bounds.size.x * .5f)
        {
            _isGameOver = true;

            Rigidbody rb = c.gameObject.GetComponent<Rigidbody>();
            if (!rb) return;
            float torquePower = (c.transform.position.x > transform.position.x) ? -10f : 10f;
            rb.AddTorque(Vector3.forward * torquePower);

            OnGameOver?.Invoke();
        }
        else
        {
            OnGettingScore?.Invoke(_scoreValue);
            LeanTween.moveLocalY(gameObject, -.3f, .2f).setEase(LeanTweenType.easeOutQuad).setLoopPingPong(1);

            // spawn floating score
            FloatingScore floatingScore = Instantiate(_data.GetFloatingScore);
            floatingScore.Spawn(transform.position, _scoreValue);
            Destroy(floatingScore.gameObject, 1f);
        }

        SoundController.GetInstance().PlayAudio(AudioType.LANDING);
    }

    private void OnCollisionExit(Collision c)
    {
        if (!_isGameOver)
        {
            StartCoroutine(FallingPlatform());

            OnLastPieceExit?.Invoke(transform.position);
        }
    }

    IEnumerator FallingPlatform()
    {
        yield return new WaitForSeconds(.5f);
        transform.parent.parent.gameObject.AddComponent<Rigidbody>();
        Destroy(transform.parent.parent.gameObject, 2f);
    }
}
