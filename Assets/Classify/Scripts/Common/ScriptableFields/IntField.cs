using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Fields/IntField")]
public class IntField : PropertyField<int>
{
    protected override bool IsChanged(int value)
    {
        return value != Value;
    }
}
