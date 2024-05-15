using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class VoxelPlacer : MonoBehaviour
{
    public GameObject blockSelection;

    public bool highlightVoxel = false;

    public bool canPlace = true;

    public bool canGrab = true;

    private PlayerInput m_PlayerInput;
    // private InputAction m_PlaceAction, m_GrabAction;
    private bool m_LastVoxelPosHit = false;
    private Vector3 m_LastVoxelPos, m_LastVoxelPos2;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        var placeAction = m_PlayerInput.actions["Place"];
        placeAction.performed += OnPlaceAction;
        var grabAction = m_PlayerInput.actions["Grab"];
        grabAction.performed += OnGrabAction;
    }

    void OnPlaceAction(InputAction.CallbackContext context)
    {
        if (GameController.IsPaused)
            return;
        if (!canPlace)
            return;
        if (!m_LastVoxelPosHit)
            return;
        TerrainManager.Main.SetVoxel(m_LastVoxelPos, Voxel.Stone);
    }

    void OnGrabAction(InputAction.CallbackContext context)
    {
        if (GameController.IsPaused)
            return;
        if (!canGrab)
            return;
        if (!m_LastVoxelPosHit)
            return;

        Voxel voxel;
        TerrainManager.Main.GetVoxel(m_LastVoxelPos2, out voxel);
        if (voxel != Voxel.Air)
        {
            TerrainManager.Main.SetVoxel(m_LastVoxelPos2, Voxel.Air);
            ItemStack itemStack = new ItemStack(ItemDatabase.Main.GetCopy(voxel.ToString()));
            ContainerManager.Main.TryAlterContainer(itemStack);
        }
    }

    void Update()
    {
        // Place selector on voxel player is currently looking
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, Consts.InteractionDistance, Consts.VoxelMask))
        {
            m_LastVoxelPosHit = true;
            m_LastVoxelPos = hit.point - ray.direction * 0.01f;
            m_LastVoxelPos2 = hit.point + ray.direction * 0.01f;

            if (highlightVoxel && blockSelection)
            {
                PlacementData posRot = ItemUtil.Snap(ray.direction, m_LastVoxelPos2, Vector3Int.one, ItemPivot.MidCenter, SnapType.Center);
                blockSelection.transform.position = posRot.position;
                blockSelection.SetActive(true);
            }
        }
        else
        {
            m_LastVoxelPosHit = false;
            blockSelection?.SetActive(false);
        }
    }
}