using UnityEngine;
using System.Collections;
using TMPro;

public class UIUtil : Singleton<UIUtil>
{
    public GameObject messageBox;
    public TMP_Text messageBoxText;

    public void ShowMessage(string message, float exibitionTime)
    {
        messageBoxText.text = message;
        messageBox.SetActive(true);
        Invoke("HideMessage", exibitionTime);
    }

    public void HideMessage()
    {
        messageBox.SetActive(false);
    }
}
