using UnityEngine;
using System.Collections.Generic;
using deVoid.Utils;

public class ColorsDirectionIndicator : MonoBehaviour
{
    public GameObject baseLine;
    public GameObject[] directions;
    List<SpriteRenderer> allSprites;
    void Awake()
    {
        Signals.Get<NewItemSignal>().AddListener(NewColorAdded);
        Signals.Get<SwitchOpenGate>().AddListener(GateOpened);
        allSprites = new List<SpriteRenderer>();
        allSprites.AddRange(baseLine.GetComponentsInChildren<SpriteRenderer>());
        for(int i=0;i<directions.Length;i++)
        {
            allSprites.AddRange(directions[i].GetComponentsInChildren<SpriteRenderer>());

        }
    }
    int currentGate = -1;
    private void NewColorAdded(int colorId,IGate gate)
    {
        Color color = ColorItemUitilites.IntIdToColor(colorId);
        SetSpritesColor(color);
        baseLine.gameObject.SetActive(true);
        directions[gate.GateType].gameObject.SetActive(true);
        currentGate = gate.GateType;
    }
    void Hide()
    {
        baseLine.gameObject.SetActive(false);
        for(int i=0;i<directions.Length;i++)
        {
            directions[i].gameObject.SetActive(false);
        }
    }
    void SetSpritesColor(Color color)
    {
        for(int i=0;i<allSprites.Count;i++)
        {
            allSprites[i].color = color;
        }
    }
    void GateOpened(int gateIndex)
    {
        if(gateIndex==currentGate)
        {
            Hide();
        }
    }
    void OnDestroy()
    {
        Signals.Get<NewItemSignal>().RemoveListener(NewColorAdded);
        Signals.Get<SwitchOpenGate>().RemoveListener(GateOpened);

    }
}
