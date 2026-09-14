using UnityEngine;
using DG.Tweening;

/// <summary>
/// Specific implementation for the Color classification gameplay
/// here the switch rotate its arm 180 degree to open a specific gate and close the other one
/// </summary>
public class ColorRotatorSwitch : Switch
{
    public Transform arm;
    public float flippingTime = 0.1f;
    void Start()
    {
        OpenGate(0);
    }
    protected override void OnOpeningGate(int gateIndex)
    {
        base.OnOpeningGate(gateIndex);
        float angle = gateIndex == 0 ? 135 : 45;
        arm.DORotate(new Vector3(0, 0, angle), flippingTime, RotateMode.Fast);
    }
    public override void ResetSwitch()
    {
        base.ResetSwitch();
        OpenGate(0);
    }
}
