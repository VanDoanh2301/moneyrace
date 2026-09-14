using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Fields/BoolField")]
public class BoolField : PropertyField<bool>
{
    protected override bool IsChanged(bool value)
    {
        return value != Value;
    }

}
