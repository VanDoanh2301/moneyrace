using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(FloatField))]

public class FloatFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        float oldValue = (target as FloatField).Value;
        base.OnInspectorGUI();
        float newValue = (target as FloatField).Value;
        if (oldValue != newValue)
        {
            Debug.Log("Changed");
            (target as FloatField).onChange?.Invoke();
        }
        //if(GUILayout.Button("Rmove all listners"))
        //{
        //   int listners= (target as FloatField).onChange.GetPersistentEventCount();
        //    Debug.Log("Rremove " + listners + " Listners");
        //    (target as FloatField).onChange.RemoveAllListeners();
        //}
    }
}
