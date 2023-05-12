using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "NPC", menuName = "Item/NPC")]
public class NPC : Item
{
    public override bool Use()
    {
        return SpawnPrefab();
    }
}