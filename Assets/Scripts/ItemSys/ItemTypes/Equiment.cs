using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EquipmentType : byte { Weapon, Tool, Head, Torso, Legs, Feet, Cloak, Necklace, Ring, Lightsource };

[System.Serializable]
public class Enchantment
{
    public string attributeName;
    public float value;
    public string description;
}

public class Equipment : Item
{
    public EquipmentType type;

    public int meleeDamage;

    public int rangedDamage;

    public int armor;

    public List<Enchantment> enchantments = new List<Enchantment>();

    // Copy gets created in case of player enchantment.
    public Equipment Enchant(Enchantment enchantment)
    {
        Equipment e = Instantiate(this);
        id = System.Guid.NewGuid().ToString();
        e.isUnique = true;
        e.enchantments.Add(enchantment);
        return e;
    }

    public override string ToString()
    {
        throw new NotImplementedException();
    }

    public override ItemData GetData()
    {
        return new EquipmentData(this);
    }

    public override void SetData(ItemData data)
    {
        if (!(data is EquipmentData))
            throw new Exception("Feeded wrong itemdata type.");

        EquipmentData ed = (EquipmentData)data;

        this.name = ed.itemName;
        this.meleeDamage = ed.meleeDamage;
        this.rangedDamage = ed.rangedDamage;
        this.armor = ed.armor;
        this.enchantments = new List<Enchantment>(ed.enchantments);
        // WIP
    }

    public override bool Use()
    {
        return false;
    }
}

[System.Serializable]
public class EquipmentData : ItemData
{
    public int meleeDamage, rangedDamage, armor;

    public Enchantment[] enchantments;

    public EquipmentData(Equipment equipment) : base(equipment)
    {
        this.meleeDamage = equipment.meleeDamage;
        this.rangedDamage = equipment.rangedDamage;
        this.armor = equipment.armor;
        this.enchantments = equipment.enchantments.ToArray();
    }
}
