using deVoid.Utils;
using UnityEngine;

/// <summary>
/// A gate controller for color classification gameplay
/// </summary>
public class ColorGate : Gate
{
    public SpriteRenderer[] ColorsIndicators;
    int colorsCount = 0;
    public void AddColor(Color color)
    {
        colorsCount++;
        if(colorsCount<=ColorsIndicators.Length)
        {
            ColorsIndicators[colorsCount - 1].color = color;
            ColorsIndicators[colorsCount - 1].gameObject.SetActive(true);
        }
    }
    public override void ResetGate()
    {
        base.ResetGate();
        for(int i=0;i<ColorsIndicators.Length;i++)
        {
            ColorsIndicators[i].gameObject.SetActive(false);
        }
        colorsCount = 0;
    }
    void Awake()
    {
        Signals.Get<NewItemSignal>().AddListener(NewColorAdded);
    }

    private void NewColorAdded(int colorId, IGate gate)
    {
        if(gate == this) AddColor(ColorItemUitilites.IntIdToColor(colorId));
    }

    void OnDestroy()
    {
        Signals.Get<NewItemSignal>().RemoveListener(NewColorAdded);
    }
}
