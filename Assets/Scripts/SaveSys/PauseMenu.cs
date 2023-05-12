using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    bool isVisible = false;

    public GameObject pausePanel;
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
        foreach (GameObject panel in otherPanels)
        {
            panel.SetActive(false);
        }
        isVisible = true;
        GameManager.IsPaused = true;
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
        foreach (GameObject panel in otherPanels)
        {
            panel.SetActive(false);
        }
        isVisible = false;
        GameManager.IsPaused = false;
    }
}
