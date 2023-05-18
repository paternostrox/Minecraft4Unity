using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class SaveMenu : MonoBehaviour
{
    public GameObject saveslot;
    public GameObject noSavesFoundMessage;
    public GameObject ovewritePopup;

    List<GameObject> createdSlots = new List<GameObject>();

    public void OverwriteConfirmation(string slotName)
    {
        Button[] buttons = ovewritePopup.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(delegate { SaveOnSlot(slotName); });
    }

    public void SaveOnSlot(string slotName)
    {
        DeleteSlot(slotName);
        string newSaveName = SaveUtil.GetSaveFileName();
        SaveManager.Main.Save(newSaveName);
        Reset();
    }

    public void CreateSave()
    {
        string newSaveName = SaveUtil.GetSaveFileName();
        SaveManager.Main.Save(newSaveName);
    }

    public void DeleteSlot(string slotName)
    {
        SaveManager.Main.DeleteSave(slotName);
        Reset();
    }

    public void Reset()
    {
        OnDisable();
        OnEnable();
    }

    private void OnEnable()
    {
        string[] saveNames = SaveUtil.GetAllSaveNames();

        if (saveNames.Length > 0)
            noSavesFoundMessage.SetActive(false);

        foreach (string saveName in saveNames)
        {
            GameObject g = Instantiate(saveslot, transform);
            g.GetComponentInChildren<TMP_Text>().text = saveName;
            Button[] buttons = g.GetComponentsInChildren<Button>();
            buttons[0].onClick.AddListener(delegate { OverwriteConfirmation(saveName); });
            buttons[1].onClick.AddListener(delegate { DeleteSlot(saveName); });
            createdSlots.Add(g);
        }
    }

    private void OnDisable()
    {
        foreach (GameObject slot in createdSlots)
        {
            Destroy(slot);
        }
        createdSlots.Clear();
        noSavesFoundMessage.SetActive(true);
    }
}
