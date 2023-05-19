using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
    public GameObject saveslot;
    public Transform saveContainer;
    public GameObject noSavesFoundMessage;

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
        string[] saveNames = SaveUtil.GetAllSaveNames(true);

        if (saveNames.Length > 0)
            noSavesFoundMessage.SetActive(false);

        foreach (string saveName in saveNames)
        {
            GameObject g = Instantiate(saveslot, saveContainer);
            g.GetComponentInChildren<TMP_Text>().text = saveName;
            Button[] buttons = g.GetComponentsInChildren<Button>();
            buttons[0].onClick.AddListener(delegate { LoadSlot(saveName); });

            if (saveName == "autosave")
                buttons[1].gameObject.SetActive(false);
            else
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
