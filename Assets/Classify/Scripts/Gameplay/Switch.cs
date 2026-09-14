using UnityEngine;
using deVoid.Utils;

/// <summary>
/// The core switch class that switch the gates, extend this class for more specific implementation, check ColorRotatorSwitch for more details
/// </summary>
public class Switch : MonoBehaviour
{
    [HideInInspector] public Gate[] Gates;
    public GameStateField gameplayState;
    public GameplayManager gameplayManager;

    protected int currentOpenedGate = -1;

    public void ToggleGate()
    {
        if (!gameplayManager.GameRunning) return;
        currentOpenedGate = (currentOpenedGate + 1) % Gates.Length;
        OnOpeningGate(currentOpenedGate);
    }
    public void OpenGate(int index)
    {
        if (index < 0 || index > Gates.Length || index == currentOpenedGate) return;
        currentOpenedGate = index;
        OnOpeningGate(currentOpenedGate);
    }

    protected virtual void OnOpeningGate(int gateIndex)
    {
        Gates[currentOpenedGate].Open();
        Signals.Get<SwitchOpenGate>().Dispatch(gateIndex);
    }

    public virtual void ResetSwitch()
    {
        currentOpenedGate = -1; ;
    }
}

/// <summary>
/// A signal that will be fired whenver a gate get switched
/// </summary>
public class SwitchOpenGate : ASignal<int> { }
