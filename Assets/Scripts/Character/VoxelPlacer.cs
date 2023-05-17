using UnityEngine;

public class VoxelPlacer : MonoBehaviour
{
    public GameObject blockSelection;

    public bool highlightVoxel = false;

    public bool canPlace = true;

    public bool canGrab = true;

    void Update()
    {
        // Place selector on voxel player is currently looking
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, Consts.InteractionDistance, Consts.VoxelMask))
        {
            if (highlightVoxel)
            {
                PlacementData posRot = ItemUtil.Snap(ray.direction, hit.point + ray.direction * 0.01f, Vector3Int.one, ItemPivot.MidCenter, SnapType.Center);
                blockSelection.transform.position = posRot.position;
                blockSelection.SetActive(true);
            }

            // Set voxel if necessary
            if (!GameManager.IsPaused)
            {
                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    TerrainManager.Main.SetVoxel(hit.point - ray.direction * 0.01f, Voxel.Stone);
                }

                if (Input.GetMouseButtonDown(1) && canGrab)
                {
                    Voxel voxel;
                    TerrainManager.Main.GetVoxel(hit.point + ray.direction * 0.01f, out voxel);
                    if (voxel != Voxel.Air)
                    {
                        TerrainManager.Main.SetVoxel(hit.point + ray.direction * 0.01f, Voxel.Air);
                        ItemStack itemStack = new ItemStack(ItemDatabase.Main.GetCopy(voxel.ToString()));
                        ContainerManager.Main.TryAlterContainer(itemStack);
                    }
                }
            }
        }
        else
        {
            blockSelection.SetActive(false);
        }
    }
}