using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDatabase : Singleton<ItemDatabase>
{

    Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();

    public Item[] items;

    public void Awake()
    {
        //Item[] items = (Item[]) Resources.FindObjectsOfTypeAll(typeof(Item));

        foreach (Item i in items)
        {
            itemDatabase.Add(i.name, i);
            print("Item added to database: " + i.name);
        }
    }

    public Item GetCopy(string itemName)
    {
        return itemDatabase[itemName].GetCopy();
    }

    public GameObject GetPrefab(string itemName)
    {
        return itemDatabase[itemName].prefab;
    }
}
