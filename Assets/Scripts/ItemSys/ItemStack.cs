using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemStack
{
    public Item item;
    public int amount;

    public ItemStack(Item item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public ItemStack(ItemStackData data)
    {
        amount = data.amount;
        item = ItemDatabase.Main.GetCopy(data.itemData.itemName);
        item.SetData(data.itemData);
    }
}

[System.Serializable]
public class ItemStackData
{
    public ItemData itemData;

    public int amount;

    public ItemStackData(ItemStack itemStack)
    {
        this.itemData = itemStack.item.GetData();
        this.amount = itemStack.amount;
    }
}