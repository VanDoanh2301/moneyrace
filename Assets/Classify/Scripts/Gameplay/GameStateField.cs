using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Fields/Gameplay State Field")]

public class GameStateField : PropertyField<GameplayState>
{
    protected override bool IsChanged(GameplayState value)
    {
        return value != Value;
    }
}
