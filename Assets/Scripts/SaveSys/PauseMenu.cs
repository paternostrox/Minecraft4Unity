using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    bool isVisible = false;

    public GameObject pausePanel;
    public GameObject HUDCenter;
    public GameObject[] otherPanels;

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        // TO DO: USE NEW INPUT SYSTEM!!!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isVisible)
                Hide();
            else
                Show();
        }
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
