using UnityEngine;
using UnityEngine.Events;
public abstract class PropertyField<T> : ScriptableObject
{
    [SerializeField] T value;
    public UnityEvent onChange;
    public T Value
    {
        get
        {
            return value;
        }

        set
        {
            if (IsChanged(value))
            {
                this.value = value;
                onChange?.Invoke();
            }
            else
            {
                this.value = value;
            }
        }
    }
    protected abstract bool IsChanged(T value);
#if UNITY_EDITOR
    [TextArea(1, 5)]
    public string Notes;
#endif
}
