﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
    public GameObject saveslot;

    List<GameObject> createdSlots = new List<GameObject>();

    public void LoadSlot(string slotName)
    {
        SaveManager.Main.Load(slotName);
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

        foreach (string saveName in saveNames)
        {
            GameObject g = Instantiate(saveslot, transform);
            g.GetComponentInChildren<TMP_Text>().text = saveName;
            Button[] buttons = g.GetComponentsInChildren<Button>();
            buttons[0].onClick.AddListener(delegate { LoadSlot(saveName); });
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
    }
}