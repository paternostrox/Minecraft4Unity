using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private Camera cam;
    private ItemContainer inventory;
    private System.Random random = new System.Random();


    private PlayerInput m_PlayerInput;

    void Awake()
    {
        cam = Camera.main;

        inventory = GetComponent<ItemContainer>();

        m_PlayerInput = GetComponent<PlayerInput>();
        var interactAction = m_PlayerInput.actions["Interact"];
        interactAction.performed += OnInteractAction;
    }

    void OnInteractAction(InputAction.CallbackContext context)
    {
        if (GameController.IsPaused)
            return;
        Interact();
    }

    bool Interact()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Consts.InteractionDistance, Consts.InteractableMask))
        {
            WorldObject worldItem = hit.collider.GetComponent<WorldObject>();
            worldItem.Interact();
        }
        return false;
    }
}
