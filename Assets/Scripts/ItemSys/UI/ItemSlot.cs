using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class ItemSlot : ImageController, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] protected TMP_Text amountText;
    public ItemStack itemStack;

    public string ID { get { return gameObject.name; } }

    public virtual void SetItemRep(ItemStack itemStack)
    {
        SetSprite(itemStack.item.thumbnail); // Null ref here
        amountText.text = itemStack.amount.ToString();

        if (itemStack.amount != 1)
            amountText.enabled = true;
        else
            amountText.enabled = false;

        this.itemStack = itemStack;
        Brighten();
        Show();
    }

    public virtual void ClearItemRep()
    {
        this.itemStack = null;
        Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemStack != null)
            ContainerManager.Main.ShowTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemStack != null)
            ContainerManager.Main.HideTooptip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ContainerManager.Main.PickSlot(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ContainerManager.Main.PickSlot(this, true);
        }
    }
}
