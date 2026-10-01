using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GameData : ScriptableObject
{
    [SerializeField] Material[] _platMats;

    [field: SerializeField]
    public FloatingScore GetFloatingScore { get; private set; }

    public Material GetRandomMaterial => _platMats[Random.Range(0, _platMats.Length)];
}