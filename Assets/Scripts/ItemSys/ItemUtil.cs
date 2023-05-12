using UnityEngine;
using System.Collections;

public class PlacementData
{
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    public Vector3 volumeCenter = Vector3.zero;
}

public static class ItemUtil
{

    public static readonly Vector3 Corner2TopMid = Vector3.up + Vector3.right * .5f + Vector3.forward * .5f;

    public static readonly Vector3 CollisionAllowance = new Vector3(.001f, .001f, .001f);

    // Snaps to center, edge or corner of a square
    public static PlacementData Snap(Vector3 rayDir, Vector3 worldPosition, Vector3 volume, ItemPivot itemPivot, SnapType snapType) // OK!
    {
        PlacementData placementData = new PlacementData();

        switch (snapType)
        {
            case SnapType.Center:
                placementData.position.x = Mathf.Floor(worldPosition.x) + .5f;
                placementData.position.z = Mathf.Floor(worldPosition.z) + .5f;

                break;

            case SnapType.Edge:
                placementData.position.x = Mathf.Round(worldPosition.x);
                placementData.position.z = Mathf.Round(worldPosition.z);

                float deltaX = worldPosition.x - placementData.position.x;
                float deltaZ = worldPosition.z - placementData.position.z;

                if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaZ))
                {
                    placementData.position += Vector3.right * .5f * Mathf.Sign(deltaX);
                    //posRot.rotation = Quaternion.Euler(0f, 90f, 0f);
                }
                else
                {
                    placementData.position += Vector3.forward * .5f * Mathf.Sign(deltaZ);
                }
                break;

            case SnapType.Corner:
                placementData.position.x = Mathf.Round(worldPosition.x);
                placementData.position.z = Mathf.Round(worldPosition.z);
                break;
        }

        // VolumeCenter X and Z coordinates are the same as Position
        placementData.volumeCenter = placementData.position;

        float posY = 0f, volY = 0f;

        switch (itemPivot)
        {
            case ItemPivot.Bottom:
                posY = Mathf.Floor(worldPosition.y); // Should be lowest point of object
                volY = posY + (volume.y / 2f);
                break;
            case ItemPivot.MidCenter:
                posY = Mathf.Floor(worldPosition.y) + .5f; // Should be middle of object (snap to center)
                volY = posY;
                break;
            case ItemPivot.MidCorner:
                placementData.position.y = Mathf.Round(worldPosition.y); // Should be middle of object (snap to corner)
                volY = posY;
                break;
            case ItemPivot.Top:
                placementData.position.y = Mathf.Ceil(worldPosition.y); // Should be highest point of object
                volY = posY - (volume.y / 2f);
                break;
        }

        placementData.position.y = posY;
        placementData.volumeCenter.y = volY;

        float angle = 0f;
        if (!rayDir.Equals(Vector3.zero))
        {
            angle = Mathf.Atan2(rayDir.x, rayDir.z) * Mathf.Rad2Deg; // Z & X or X & Z?
            angle = Mathf.Round(angle / 90f) * 90f;
        }
        else
        {
            angle = GetRandomRot();
        }

        placementData.rotation = Quaternion.Euler(0f, angle, 0f);
        return placementData;
    }

    public static float GetRandomRot()
    {
        int a = UnityEngine.Random.Range(0, 4);
        switch (a)
        {
            case 0:
                return 0f;
            case 1:
                return 90f;
            case 2:
                return 180f;
            case 3:
                return 270f;
        }
        return 0f;
    }

    public static Vector3 FindNearestEmpty(Vector3 position)
    {
        throw new System.NotImplementedException();
    }
}