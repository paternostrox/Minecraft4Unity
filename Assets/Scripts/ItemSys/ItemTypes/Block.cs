using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Block", menuName = "Item/Block")]
public class Block : Item
{
    public Voxel voxelType;

    public override bool Use()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Consts.InteractionDistance, Consts.VoxelMask))
        {
            TerrainManager.Main.SetVoxel(hit.point - ray.direction * 0.01f, voxelType);
            return true;
        }
        return false;
    }
}
