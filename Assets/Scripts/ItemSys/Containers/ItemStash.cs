using UnityEngine;
using System.Collections;

public class ItemStash : ItemContainer
{
    public static string[] slotIDs;

    public override string[] SlotIDs { get => slotIDs; }

    public override int Capacity { get => slotIDs.Length; }
}
