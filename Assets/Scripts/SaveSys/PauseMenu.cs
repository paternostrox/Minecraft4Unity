using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    bool isVisible = false;

    public GameObject pausePanel;
    public GameObject HUDCenter;
    public GameObject[] otherPanels;

    private PlayerInput m_PlayerInput;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        var pauseAction = m_PlayerInput.actions["Pause"];
        pauseAction.performed += OnPauseAction;
    }

    private void Start()
    {
        Hide();
    }

    void OnPauseAction(InputAction.CallbackContext context)
    {
        if (isVisible)
            Hide();
        else
            Show();
    }


    public void Show()
    {
        pausePanel.SetActive(true);
        HUDCenter.SetActive(false);
        foreach (GameObject panel in otherPanels)
        {
            panel.SetActive(false);
        }
        isVisible = true;
        GameController.IsPaused = true;
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
        HUDCenter.SetActive(true);
        foreach (GameObject panel in otherPanels)
        {
            panel.SetActive(false);
        }
        isVisible = false;
        GameController.IsPaused = false;
    }
}
