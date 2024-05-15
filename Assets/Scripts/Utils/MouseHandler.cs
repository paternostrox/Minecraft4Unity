using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHandler : MonoBehaviour
{
    public Transform cursorImageTransform;

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
        lastPauseState = GameController.IsPaused;
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lastPauseState = GameController.IsPaused;
    }

    private void Update()
    {
        if (GameController.IsPaused != lastPauseState)
        {
            if (GameController.IsPaused)
                UnlockMouse();
            else
                LockMouse();
        }
    }
}
