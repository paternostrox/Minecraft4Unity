using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class SaveMenu : MonoBehaviour
{
    public GameObject saveslot;

    List<GameObject> createdSlots = new List<GameObject>();

    public void SaveOnSlot(string slotName)
    {
        SaveManager.Main.Save(slotName);
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
            buttons[0].onClick.AddListener(delegate { SaveOnSlot(saveName); });
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
