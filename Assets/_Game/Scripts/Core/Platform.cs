using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] Vector2 _randomSpeed;

    [SerializeField] Vector2 _gap;
    [SerializeField] int _seedSize = 10;
    [SerializeField] Transform _container;

    [Space]
    [SerializeField] Piece[] _pieces;

    int[] seed;

    float _pieceSpeed;
    float newXPos = 0f;
    float _resetPos;
    bool _isOver = false;

    bool _isInverted = false;
    public bool InvertPos { set { _isInverted = value; } }

    List<Piece> _pieceList = new List<Piece>();

    private void OnEnable()
    {
        GameManager.OnGameEnd += StopMoving;
        GameManager.OnRevive += MoveEnabled;
    }

    private void Start()
    {
        if (_isInverted)
        {
            transform.eulerAngles = Vector3.up * 180;
        }

        GenerateSeed();
        GeneratePiece();

        _pieceSpeed = Random.Range(_randomSpeed.x, _randomSpeed.y);
    }

    public void CalculateRandomSpeed(float value)
    {
        _randomSpeed += Vector2.one * value;
    }

    void GenerateSeed()
    {
        seed = new int[_seedSize];

        for (int i = 0; i < seed.Length; i++)
        {
            seed[i] = Random.Range(0, _pieces.Length);
        }
    }

    void GeneratePiece()
    {
        for (int i = 0; i < seed.Length; i++)
        {
            Piece newPiece = Instantiate(_pieces[seed[i]], _container);
            newPiece.transform.localPosition += Vector3.right * newXPos;
            _pieceList.Add(newPiece);

            int nextIndex = (i + 1) % seed.Length;
            float gap = Random.Range(_gap.x, _gap.y);
            newXPos += _pieces[seed[i]].HalfWidth + _pieces[seed[nextIndex]].HalfWidth + gap;
        }

        _resetPos = newXPos;
    }

    private void Update()
    {
        MovePieces();
    }

    private void MoveEnabled()
    {
        _isOver = false;
    }

    private void MovePieces()
    {
        if (_isOver) return;

        foreach (var piece in _pieceList)
        {
            piece.transform.Translate(Vector3.right * _pieceSpeed * Time.deltaTime);

            if (piece.transform.localPosition.x >= _resetPos)
            {
                Vector3 newPos = piece.transform.localPosition;
                newPos.x = 0;
                piece.transform.localPosition = newPos;
            }
        }
    }

    private void StopMoving()
    {
        _isOver = true;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnd -= StopMoving;
        GameManager.OnRevive -= MoveEnabled;
    }
}
