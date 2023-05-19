using UnityEngine;
using System.Collections;

public class InteractionManager : Singleton<InteractionManager>
{
    WorldObject currentWorldObject;

    [SerializeField] GameObject itemGiverPanel;
    [SerializeField] GameObject itemInteractionPanel;

    // Item Specific Menus
    // SomeItemMenu

    private void Start()
    {
        Hide();
    }

    public void Hide()
    {
        // Hide all Panels
        itemGiverPanel.SetActive(false);
        itemInteractionPanel.SetActive(false);
        GameController.IsPaused = false;
    }
}
