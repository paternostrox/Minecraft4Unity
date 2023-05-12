using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// Slot handling is a mess, and it starts in this class
// It works but it should really be handled in a better way.
public class ContainerManager : Singleton<ContainerManager>
{
    public Inventory playerInventory;
    ItemStash currentOpenStash;

    public Dictionary<string, ItemSlot> UISlots = new Dictionary<string, ItemSlot>();

    [SerializeField] ItemSlot handSlot;
    string lastSlotID;

    string[] hotbarSlotIDs;
    int selectedHotbarSlotIndex = 0; // Currently selected hotbar slot
    public RectTransform selectionIndicatorUI;

    private void Start()
    {
        // Define slots
        List<string> stashSlotIDs = new List<string>();
        List<string> inventorySlotIDs = new List<string>();
        List<string> hotbarSlotIDs = new List<string>();
        ItemSlot[] slots = GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in slots)
        {
            string name = slot.gameObject.name;
            UISlots.Add(name, slot);
            switch (name[0])
            {
                case 'S':
                    stashSlotIDs.Add(name);
                    break;
                case 'H':
                    hotbarSlotIDs.Add(name);
                    inventorySlotIDs.Add(name);
                    break;
                case 'B':
                    inventorySlotIDs.Add(name);
                    break;
                default:
                    break;
            }
        }
        ItemStash.slotIDs = stashSlotIDs.ToArray();
        Inventory.slotIDs = inventorySlotIDs.ToArray();
        this.hotbarSlotIDs = hotbarSlotIDs.ToArray();
        RefreshInventory(); // FreshPrince™
    }

    bool isVisible = false;
    private void Update()
    {
        // TO DO: USE NEW INPUT SYSTEM!!!
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isVisible)
                Hide();
            else
                Show();
        }
        CheckChangeSelection();
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isVisible)
        {
            UseHotbarItem(selectedHotbarSlotIndex);
        }
    }

    public void CheckChangeSelection()
    {
        float wheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (!(wheelInput.Equals(0f)))
        {
            selectedHotbarSlotIndex = (selectedHotbarSlotIndex + (int)Mathf.Sign(wheelInput)) % hotbarSlotIDs.Length;
            if (selectedHotbarSlotIndex < 0) selectedHotbarSlotIndex += hotbarSlotIDs.Length;
            selectionIndicatorUI.anchoredPosition = new Vector3(-1f + selectedHotbarSlotIndex * 17f, .25f, 0f);
        }
    }

    public void UseHotbarItem(int slotIndex)
    {
        UseItem(hotbarSlotIDs[slotIndex]);
    }

    public void UseItem(string slotID)
    {
        GetContainerBySlot(slotID).UseItem(slotID);
    }

    public string GetSelectedSlotID()
    {
        return hotbarSlotIDs[selectedHotbarSlotIndex];
    }

    public void OpenStash(ItemStash stash)
    {
        currentOpenStash = stash;
        RefreshStash();
    }

    public bool TryAlterContainer(ItemStack itemStack, char slotGroup = 'H')
    {
        if (slotGroup == 'S')
            return currentOpenStash.TryAlter(itemStack);

        return playerInventory.TryAlter(itemStack);
    }

    public void OverwriteSlot(string slotID, ItemStack item)
    {
        GetContainerBySlot(slotID).OverwriteSlot(slotID, item);
    }

    public ItemStack PeekSlot(string slotID)
    {
        return GetContainerBySlot(slotID).Peek(slotID);
    }

    public void PickSlot(ItemSlot itemSlot, bool rightButton = false)
    {
        ItemStack handStack;
        ItemStack slotStack;

        if (handSlot.itemStack == null)
        {
            if (itemSlot.itemStack == null)
                return;

            // Hand is empty and slot has item, must pick item
            handStack = new ItemStack(itemSlot.itemStack.item, 0);
            slotStack = new ItemStack(itemSlot.itemStack.item, 0);

            if (rightButton)
            {
                // Right button was used, pick half if possible
                if (itemSlot.itemStack.amount < 2)
                    return;

                int halfAmount = itemSlot.itemStack.amount / 2;
                handStack.amount = halfAmount;
                slotStack.amount = itemSlot.itemStack.amount - halfAmount;
            }
            else    // Left button was used, pick whole amount into hand
            {
                handStack.amount = itemSlot.itemStack.amount;
            }
        }
        else
        {
            // Hand has item, must drop
            handStack = new ItemStack(itemSlot.itemStack != null ? itemSlot.itemStack.item : null, 0);
            slotStack = new ItemStack(handSlot.itemStack.item, 0);
            bool sameItemType = itemSlot.itemStack != null && itemSlot.itemStack.item == handSlot.itemStack.item;
            int slotAmount = sameItemType ?
                itemSlot.itemStack.amount : 0;

            if (rightButton)
            {
                // Right button was used, so place only one if possible
                if (itemSlot.itemStack != null && (!sameItemType || slotAmount + 1 > itemSlot.itemStack.item.maxStack))
                    return;

                handStack.item = handSlot.itemStack.item;
                slotStack.amount = slotAmount + 1;
                handStack.amount = handSlot.itemStack.amount - 1;
            }
            else
            {
                // Left button was pressed, place as much as possible or switch
                if (sameItemType)
                {
                    int spaceLeft = itemSlot.itemStack.item.maxStack - slotAmount;
                    int addAmount = Mathf.Min(spaceLeft, handSlot.itemStack.amount);
                    slotStack.amount = slotAmount + addAmount;
                    handStack.amount = handSlot.itemStack.amount - addAmount;
                }
                else
                {
                    slotStack.amount = handSlot.itemStack.amount;
                    handStack.amount = itemSlot.itemStack != null ? itemSlot.itemStack.amount : 0;
                }
            }
        }

        OverwriteHandSlot(handStack);
        OverwriteSlot(itemSlot.ID, slotStack);
        lastSlotID = itemSlot.ID;
    }

    public void CancelPickSlot()
    {
        if (handSlot.itemStack != null)
        {
            ItemSlot lastSlot = UISlots[lastSlotID];
            PickSlot(lastSlot);
        }
    }

    public ItemContainer GetContainerBySlot(string slotID)
    {

        // This is kinda arbitrary, think of another way of doing this without manually setting each one
        switch (slotID[0])
        {
            case 'S':
                return currentOpenStash;
            case 'E': // 'E', 'B' or 'H' 
            case 'H':
            case 'B':
                return playerInventory;
            default:
                throw new System.Exception("Slot has unknown container type.");
        }
    }


    #region UI Control Stuff

    public GraphicRaycaster graphicRaycaster;

    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject stashPanel;

    Coroutine dragFollowRoutine, tooltipFollowRoutine;
    [SerializeField] Tooltip tooltip;
    [SerializeField] Vector3 tooltipOffset = new Vector3(1f, -1f, 0);

    public void Show(bool showStash = false)
    {
        inventoryPanel.SetActive(true);
        if (showStash)
            stashPanel.SetActive(true);
        isVisible = true;
        GameManager.IsPaused = true;
    }

    public void Hide()
    {
        inventoryPanel.SetActive(false);
        stashPanel.SetActive(false);
        HideTooptip();
        CancelPickSlot();
        isVisible = false;
        GameManager.IsPaused = false;
    }
    public void RefreshInventory()
    {
        ItemStack item;
        foreach (KeyValuePair<string, ItemSlot> slot in UISlots)
        {
            switch (slot.Key[0])
            {
                case 'S':
                    break;
                default: // 'E' or 'B'
                    item = playerInventory.Peek(slot.Key);
                    if (item != null)
                        slot.Value.SetItemRep(item);
                    else
                        slot.Value.ClearItemRep();
                    break;
            }
        }
    }

    public void RefreshStash()
    {
        foreach (KeyValuePair<string, ItemSlot> slot in UISlots)
        {
            if (slot.Key[0] == 'S')
            {
                ItemStack itemStack = currentOpenStash.Peek(slot.Key);
                if (itemStack != null)
                    slot.Value.SetItemRep(itemStack);
                else
                    slot.Value.ClearItemRep();
            }
        }
    }

    public void UpdateSlotUI(string slotID, ItemStack itemStack)
    {
        if (itemStack != null && itemStack.amount > 0)
            UISlots[slotID].SetItemRep(itemStack);
        else
            UISlots[slotID].ClearItemRep();
    }

    public void OverwriteHandSlot(ItemStack itemStack)
    {
        if (dragFollowRoutine != null)
            StopCoroutine(dragFollowRoutine);

        if (itemStack.item != null && itemStack.amount > 0)
        {
            handSlot.SetItemRep(itemStack);
            dragFollowRoutine = StartCoroutine(ImageFollowCursor(handSlot, Vector3.zero));
            return;
        }
        handSlot.ClearItemRep();
    }

    public void ShowTooltip(ItemSlot itemSlot)
    {
        if (handSlot.itemStack == null)
        {
            tooltip.SetText(itemSlot.itemStack.item.ToString());
            tooltip.gameObject.SetActive(true);
            tooltipFollowRoutine = StartCoroutine(ImageFollowCursor(tooltip, tooltipOffset));
        }
    }

    public void HideTooptip()
    {
        if (tooltipFollowRoutine != null)
            StopCoroutine(tooltipFollowRoutine);
        tooltip.gameObject.SetActive(false);
    }

    IEnumerator ImageFollowCursor(ImageController imageController, Vector3 offset)
    {
        for (; ; )
        {
            imageController.SetPosition(Input.mousePosition + offset);
            yield return null;
        }
    }

    #endregion
}
