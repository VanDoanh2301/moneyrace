using deVoid.Utils;
using System.Collections.Generic;
using UnityEngine;

public enum GameplayState
{
    Transitioning,
    Running,
    Paused,
    WaitingAction,
    Finished
}

/// <summary>
/// The core gameplay manager
/// You extend this class to create different kind of classify games, check ColorGameplayManager.cs for more details
/// </summary>
public class GameplayManager : MonoBehaviour
{
    public UIManager uiManager;
    public Gate[] gates;
    public Switch gatesSwitch;
    public ItemGenerator generator;
    public FloatField score;
    public GameStateField State;
    public IntField lives;


    [Header("Items generation paramaters")]
    public float initialGenerationRate = 1;
    public float maximumGenerationRate = 1.5f;
    public int maxDifficultyScore = 150;
    public float delayBeforeNewItem = 0.5f;

    [Header("Difficulty levels")]
    public int[] levelsThresholds;
    

    protected Dictionary<int, int> itemsGates = new Dictionary<int, int>();
    protected List<int> itemsIds = new List<int>();
    protected float delay = 0;
    protected int currentLevel = 0;
    protected int generatedItems = 0;
    GameplayState stateBeforePausing;

    Item lastGeneratedItem;
    bool allItemsPassedGate = true;

    
    protected virtual float TimeBetweenItems
    {
        get
        {
            float generationRate = Mathf.Lerp(initialGenerationRate, maximumGenerationRate, score.Value * 1.0f / maxDifficultyScore);
            return 1 / generationRate;
        }
    }

    protected virtual bool ShouldIntroduceNewLevel
    {
        get
        {
            return (currentLevel < levelsThresholds.Length && generatedItems >= levelsThresholds[currentLevel]);
        }
    }
    protected virtual void Awake()
    {
        Application.targetFrameRate = 60;
        for (int i = 0; i < gates.Length; i++)
            gates[i].GateType = i;
        gatesSwitch.Gates = gates;
        State.Value = GameplayState.Transitioning;
    }
    public void TakeScreenShot()
    {
        ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("ddMMyyHHmmss") + ".png");
    }
    public virtual void RestartGameplay()
    {
        Time.timeScale = 1;
        for (int i = 0; i < gates.Length; i++)
            gates[i].ResetGate();
        gatesSwitch.ResetSwitch();
        uiManager.ShowScreen(Screens.Gameplay);
        generatedItems = 0;
        allItemsPassedGate = true;
        StartGame();
    }


    public virtual void StartGame()
    {
        extraLifeUsed = false;
        lives.Value = 0;
        score.Value = 0;
        State.Value = GameplayState.Transitioning;
        itemsGates.Clear();
        itemsIds.Clear();
        for (int i = 0; i < gates.Length; i++)
            gates[i].ItemEntered += OnItemEnteredGate;
        generator.Init();
        currentLevel = 0;
        delay = 0;
        State.Value = GameplayState.Running;
    }

    protected int LastAssignedGate
    {
        get
        {
            int gateIndex = -1;
            if (itemsIds.Count > 0)
            {
                int lastId = itemsIds[itemsIds.Count - 1];
                gateIndex = itemsGates[lastId];
            }
            return gateIndex;
        }
    }

    protected virtual Item AddNewItemType()
    {
        Item item = generator.AddItem();
        item.gameObject.SetActive(true);
        int gateIndex = (LastAssignedGate + 1) % gates.Length;
        Signals.Get<NewItemSignal>().Dispatch(item.ItemId, gates[gateIndex]);
        lives.Value++;
        itemsGates.Add(item.ItemId, gateIndex);
        itemsIds.Add(item.ItemId);
        delay = TimeBetweenItems;
        generatedItems++;
        return item;
    }

    private void Update()
    {
        if (State.Value != GameplayState.Running)
            return;
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            if (ShouldIntroduceNewLevel)
            {
                if (allItemsPassedGate)
                {
                    currentLevel++;
                    AddNewItemType();
                }
            }
            else
            {
                GenerateItem();
                delay = TimeBetweenItems + (ShouldIntroduceNewLevel ? delayBeforeNewItem : 0);
            }
        }
    }

    protected virtual Item GenerateItem()
    {
        int randomIndex = Random.Range(0, itemsIds.Count);
        int randomId = itemsIds[randomIndex];
        Item item = generator.GetItem(randomId);
        item.gameObject.SetActive(true);
        generatedItems++;
        lastGeneratedItem = item;
        allItemsPassedGate = false;
        return item;
    }

    protected virtual void OnItemEnteredGate(IGate gate, Item item)
    {
        if (!itemsGates.ContainsKey(item.ItemId)) return;
        if (item == lastGeneratedItem) allItemsPassedGate = true;
        int gateIndex = System.Array.IndexOf(gates, gate);
        if (gateIndex < 0) return;
        if (itemsGates[item.ItemId] == gateIndex)
        {
            score.Value++;
            if(score.Value>LocalDataStorage.HighScore)
            {
                LocalDataStorage.HighScore = (int)score.Value;
            }
        }
        else
        {
            lives.Value--;
            OnItemEnteredWrongGate(item);
           
        }
    }

    protected virtual void OnItemEnteredWrongGate(Item item)
    {
        if (lives.Value <= 0)
            GameOver();
    }
    bool extraLifeUsed = false;
    protected virtual void GameOver()
    {
        if(!extraLifeUsed)
        {
            Time.timeScale = 0;
            stateBeforePausing = State.Value;
            ShowExtraLifePanel();
        }
        else
        {
            EndGame();
        }
       
    }

    private void ShowExtraLifePanel()
    {
        uiManager.ShowScreen(Screens.ExtraLifeRewardedAd, false, true);
    }

    public void ShowRewardedAdd()
    {
        EndGame();
    }

    public virtual void EndGame()
    {
        for (int i = 0; i < gates.Length; i++)
            gates[i].ItemEntered -= OnItemEnteredGate;
        State.Value = GameplayState.Finished;
        Time.timeScale = 0;
        uiManager.ShowScreen(Screens.GameOver);
    }

    public bool GameRunning { get { return State.Value == GameplayState.Running || State.Value == GameplayState.WaitingAction; } }
    public virtual void Pause()
    {
        if (!GameRunning) return;
        stateBeforePausing = State.Value;
        State.Value = GameplayState.Paused;
        Time.timeScale = 0;
    }
    public virtual void Resume()
    {
        State.Value = stateBeforePausing;
        Time.timeScale = 1;
    }
    public void RestartTheGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}

//first parameter is item type (aka color), second paramter is the gate
public class NewItemSignal : ASignal<int, IGate> { }