using UnityEngine;
using System.Collections;

[System.Serializable]
public class WorldPlacementData
{
    public float[] position = new float[3];
    public float[] rotation = new float[3];
    public float[] scale = new float[3];

    public WorldPlacementData(Transform transform)
    {
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        position[2] = transform.position.z;

        Vector3 rotEuler = transform.rotation.eulerAngles;
        rotation[0] = rotEuler.x;
        rotation[1] = rotEuler.y;
        rotation[2] = rotEuler.z;

        scale[0] = transform.localScale.x;
        scale[1] = transform.localScale.y;
        scale[2] = transform.localScale.z;
    }
}
