using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Fields/FloatField")]
public class FloatField : PropertyField<float>
{
    protected override bool IsChanged(float value)
    {
        return Mathf.Abs(value - Value) > .00001f;
    }
}
