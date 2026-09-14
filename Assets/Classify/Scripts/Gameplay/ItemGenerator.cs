using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item Generators that can be used to generate different types of game items using a simple pooling system and different kind of randomization techniques
/// Only random generation is implemented here, you can implement other generation techniques by handling all the cases in Add item function
/// </summary>
public class ItemGenerator : MonoBehaviour
{
    public enum SelectionMode
    {
        Random,
        RandomPeriodically,
        Sequence,
        SquenceLoop
    }
    public SelectionMode selectionMode;
    public Item[] itemsPrefabs;
    List<Item> tempItemsPrefabs = new List<Item>();

    List<Item> currentItemsPrefabs = new List<Item>();


    Dictionary<int, List<Item>> itemsPool = new Dictionary<int, List<Item>>();

    public void Init()
    {
        tempItemsPrefabs.Clear();
        tempItemsPrefabs.AddRange(itemsPrefabs);
        currentItemsPrefabs.Clear();
        ClearPool();
    }

    void ClearPool()
    {
        foreach(var pair in itemsPool)
        {
            if (pair.Value == null) continue;
            for(int i=0;i<pair.Value.Count;i++)
            {
                Destroy(pair.Value[i].gameObject);
            }
            pair.Value.Clear();
        }
    }

    public void DeactivateAll()
    {
        foreach (var pair in itemsPool)
        {
            if (pair.Value == null) continue;
            for (int i = 0; i < pair.Value.Count; i++)
            {
                pair.Value[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Generate an item based on the type (aka id)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual Item GetItem(int id)
    {
        //find unused item in the pool
        if (itemsPool.ContainsKey(id))
        {
            List<Item> items = itemsPool[id];
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].gameObject.activeSelf)
                {
                    items[i].transform.localPosition = Vector3.zero;
                    return items[i];
                }
            }
        }

        //All Items are currently active (used), let's instantiate a new one
        Item itemPrefab = null;
        for (int i = 0; i < currentItemsPrefabs.Count; i++)
            if (currentItemsPrefabs[i].ItemId == id)
                itemPrefab = currentItemsPrefabs[i];
        if (itemPrefab == null)
        {
            Debug.LogWarning($"No prefab with the id: {id} in the current selected prefabs!");
            return null;
        }
        Item item = Instantiate(itemPrefab, transform.position, Quaternion.identity, transform);
        item.gameObject.SetActive(false);
        if (!itemsPool.ContainsKey(id))
        {
            itemsPool.Add(id, new List<Item>());
        }
        itemsPool[id].Add(item);
        return item;

    }

    /// <summary>
    /// Add new item category to the list of categories (pools) that can be generated
    /// </summary>
    /// <returns></returns>
    public Item AddItem()
    {
        switch(selectionMode)
        {
            case SelectionMode.Random:
                if (tempItemsPrefabs.Count == 0)
                    return null;
                int randomIndex = Random.Range(0, tempItemsPrefabs.Count);
                Item itemPrefab = tempItemsPrefabs[randomIndex];
                tempItemsPrefabs.RemoveAt(randomIndex);
                currentItemsPrefabs.Add(itemPrefab);
                return GetItem(itemPrefab.ItemId);

                break;
            case SelectionMode.RandomPeriodically:
                throw new System.NotImplementedException();
                break;
            case SelectionMode.Sequence:
                throw new System.NotImplementedException();
                break;
            case SelectionMode.SquenceLoop:
                throw new System.NotImplementedException();
                break;
            default:
                return null;

        }
    }
}
