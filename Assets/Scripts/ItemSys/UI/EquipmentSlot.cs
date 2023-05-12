using UnityEngine;
using System.Collections;

public class EquipmentSlot : ItemSlot
{
    public EquipmentType equipmentType;

    public Sprite filler;

    public override void ClearItemRep()
    {
        SetSprite(filler);
        Brighten();
        amountText.enabled = false;
        itemStack = null;
    }
}