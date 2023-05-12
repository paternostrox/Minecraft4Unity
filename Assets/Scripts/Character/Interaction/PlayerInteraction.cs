using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    private Camera cam;
    private ItemContainer inventory;
    private System.Random random = new System.Random();

    private void Start()
    {
        cam = Camera.main;
        inventory = GetComponent<ItemContainer>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public bool Interact()
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
