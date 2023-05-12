using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHandler : MonoBehaviour
{
    public Transform cursorImageTransform;
    public Image cursorImage;

    public bool disableOnStart;

    bool lastPauseState;

    private void Start()
    {
        if (disableOnStart)
        {
            Cursor.visible = false;
            LockMouse();
        }
    }

    public void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        cursorImage.enabled = true;
        lastPauseState = GameManager.IsPaused;
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cursorImage.enabled = false;
        lastPauseState = GameManager.IsPaused;
    }

    private void Update()
    {
        if (GameManager.IsPaused != lastPauseState)
        {
            if (GameManager.IsPaused)
                UnlockMouse();
            else
                LockMouse();
        }
        if (GameManager.isPaused)
        {
            cursorImageTransform.position = Input.mousePosition;
        }
    }
}
