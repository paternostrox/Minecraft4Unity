using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : ItemContainer
{
    public static string[] slotIDs;
    public override string[] SlotIDs { get => slotIDs; }
    public override int Capacity { get => slotIDs.Length - slotIDs.Count(id => id[0] == 'E'); }

    [SerializeField] PlayerStats playerStats;

    public void IncreaseStats(Equipment equipment)
    {
        playerStats.Armor += equipment.armor;
        playerStats.BaseMeleeDamage += equipment.meleeDamage;
        playerStats.BaseRangedDamage += equipment.rangedDamage;

        // TO DO: HANDLE ENCHANTMENT HERE
    }

    public void DecreaseStats(Equipment equipment)
    {
        playerStats.Armor -= equipment.armor;
        playerStats.BaseMeleeDamage -= equipment.meleeDamage;
        playerStats.BaseRangedDamage -= equipment.rangedDamage;

        // TO DO: HANDLE ENCHANTMENT HERE
    }
}
