using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// This class is used for Events that have one bool argument.
/// </summary>

[CreateAssetMenu(menuName = "Events/Bool Event Channel")]
public class BoolEventChannelSO : ScriptableObject
{
	public UnityAction<bool> OnEventRaised;
	public void RaiseEvent(bool value)
	{
		OnEventRaised.Invoke(value);
	}
}
