using UnityEngine;
using System.Collections;
using TMPro;

public class Tooltip : ImageController
{
    [SerializeField] TMP_Text text;

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
