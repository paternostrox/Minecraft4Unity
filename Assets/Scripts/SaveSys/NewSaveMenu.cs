using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewSaveMenu : MonoBehaviour
{

    public TMP_InputField newSaveInputField;
    public Button createSaveButton;

    public void CreateSave()
    {
        string newSaveName = newSaveInputField.text;

        SaveManager.Main.Save(newSaveName);
    }

    public void CheckName()
    {
        bool isValid = SaveUtil.ValidateSaveName(newSaveInputField.text);

        if (isValid)
            createSaveButton.interactable = true;
        else
            createSaveButton.interactable = false;
    }

    private void OnEnable()
    {
        newSaveInputField.onValueChanged.AddListener(delegate { CheckName(); });
        newSaveInputField.text = string.Empty;
    }

    private void OnDisable()
    {
        newSaveInputField.onValueChanged.RemoveListener(delegate { CheckName(); });
    }
}
