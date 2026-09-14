using System;
using UnityEngine;
 
/// <summary>
/// The core gate functionality that can be extended for different kind of classification games, check ColorGate for more details about specific use case
/// </summary>
public class Gate : MonoBehaviour, IGate
{
    public GateState Currentstate { get; private set; }

    public event Action<IGate, Item> ItemEntered;

    public int GateType
    {
        get; set;
    }

    public virtual void ResetGate()
    {
        Currentstate = GateState.Closed;
    }
    public virtual void Open()
    {
        if (Currentstate == GateState.Opened) return;
        Currentstate = GateState.Opened;

    }
    public virtual void Close()
    {
        if (Currentstate == GateState.Closed) return;
        Currentstate = GateState.Closed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            OnItemEntered(item);
        }
    }

    protected virtual void OnItemEntered(Item item)
    {
        ItemEntered?.Invoke(this, item);
    }
}

public enum GateState : byte
{
    Closed = 0,
    Opened = 1
}
public interface IGate
{
    GateState Currentstate { get; }
    int GateType { get; set; }
    void Open();
    void Close();
    void ResetGate();
    event Action<IGate, Item> ItemEntered;
}
