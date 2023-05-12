using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public partial class Chunk : MonoBehaviour
{
    TerrainManager generator;
    public Vector3Int chunkPosition;
    Vector3Int chunkSize;
    bool argent; // ?
    Voxel[] voxels;
    Coroutine meshUpdater;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public event Func<bool> CanUpdate;

    NoiseGenerator.NativeVoxelData voxelData;
    ChunkMeshBuilder.NativeMeshData meshData;

    public bool Dirty { get; private set; }
    public bool Updating => meshUpdater != null;
    public bool Initialized { get; private set; }
    public bool Itemized { get; set; }
    public Voxel[] Voxels => voxels;

    public int3[] groundVoxels;

    // Awake is called immediately after instantiation.
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh { indexFormat = IndexFormat.UInt32 }; // Allows bigger meshes to be rendered.
        CanUpdate = () => true; // This is replaced by a delegate before Update happens. No idea why this exists.
    }

    void OnDestroy()
    {
        voxelData?.jobHandle.Complete();
        voxelData?.Dispose();
        meshData?.jobHandle.Complete();
        meshData?.Dispose();
        DestroyAllItems();
    }

    void Start()
    {
        meshFilter.mesh = mesh;
    }

    public void Init(Vector3Int position, TerrainManager parent)
    {
        chunkPosition = position;
        generator = parent;

        meshRenderer.material = generator.ChunkMaterial;
        chunkSize = generator.ChunkSize;

        if (TryInitByLoad())
        {
            return; // Do something else?
        }

        StartCoroutine(nameof(InitByNoise));
    }

    IEnumerator InitByNoise()
    {
        int numVoxels = chunkSize.x * chunkSize.y * chunkSize.z;
        voxels = new Voxel[numVoxels];
        voxelData = new NoiseGenerator.NativeVoxelData(VoxelUtil.ToInt3(chunkSize));
        yield return voxelData.Generate(voxels, VoxelUtil.ToInt3(chunkPosition), VoxelUtil.ToInt3(chunkSize));
        Dirty = true;
        Initialized = true;
    }

    void Update()
    {
        if (!Initialized)
            return;

        if (Updating)
            return;

        if (!Dirty)
            return;

        if (CanUpdate == null || !CanUpdate())
            return;

        meshUpdater = StartCoroutine(nameof(UpdateMesh));
    }

    IEnumerator UpdateMesh()
    {
        if (Updating)
            yield break;

        if (!generator.CanUpdate)
            yield break;

        generator.UpdatingChunks++;

        int3 chunkSizeInt3 = VoxelUtil.ToInt3(chunkSize);

        // Was used for lighting job, no longer necessary 
        //List<Voxel[]> neighborVoxels = generator.GetNeighborVoxels(chunkPosition, 1);

        meshData?.Dispose();
        meshData = new ChunkMeshBuilder.NativeMeshData(VoxelUtil.ToInt3(chunkSize));

        // Light data is no longer passed to meshing job.
        yield return meshData.ScheduleMeshingJob(voxels, VoxelUtil.ToInt3(chunkSize), argent);

        meshData.GetMeshInformation(out int verticeSize, out int indicesSize);

        if (verticeSize > 0 && indicesSize > 0)
        {
            mesh.Clear();
            mesh.SetVertices(meshData.nativeVertices, 0, verticeSize);
            mesh.SetNormals(meshData.nativeNormals, 0, verticeSize);
            mesh.SetUVs(0, meshData.nativeUVs, 0, verticeSize);
            mesh.SetIndices(meshData.nativeIndices, 0, indicesSize, MeshTopology.Triangles, 0); // Triangles

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (argent)
                SetSharedMesh(mesh); // When setting a single voxel, the collider doesn't need rebuilding?
            else
                ChunkColliderBuilder.Main.Enqueue(this, mesh);

            groundVoxels = new int3[meshData.groundCounter.Count];
            Array.Copy(meshData.nativeGroundVoxels.ToArray(), groundVoxels, meshData.groundCounter.Count);

            if (!Itemized)
            {
                ChunkObjectSpawner.Main.Enqueue(this);
                // ground positions are kept, but we don't know which are occupied (that's okay)
            }
        }

        meshData.Dispose();
        Dirty = false;
        argent = false;
        gameObject.layer = LayerMask.NameToLayer("Voxel");
        meshUpdater = null;
        generator.UpdatingChunks--;
    }

    public void SetSharedMesh(Mesh bakedMesh)
    {
        meshCollider.sharedMesh = bakedMesh;
    }

    public bool GetVoxel(Vector3Int gridPosition, out Voxel voxel)
    {
        if (!Initialized)
        {
            voxel = VoxelUtil.Empty;
            return false;
        }

        if (!VoxelUtil.BoundaryCheck(gridPosition, chunkSize))
        {
            voxel = VoxelUtil.Empty;
            return false;
        }

        voxel = voxels[VoxelUtil.To1DIndex(gridPosition, chunkSize)];
        return true;
    }

    public bool SetVoxel(Vector3Int gridPosition, Voxel type)
    {
        if (!Initialized)
        {
            return false;
        }

        if (!VoxelUtil.BoundaryCheck(gridPosition, chunkSize))
        {
            return false;
        }

        voxels[VoxelUtil.To1DIndex(gridPosition, chunkSize)] = type;
        Dirty = true;
        argent = true;
        modified = true;
        return true;
    }

    public void NeighborChunkIsChanged()
    {
        Dirty = true;
        argent = true;
    }

    void OnDrawGizmos()
    {
        if (!Initialized)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + new Vector3(chunkSize.x / 2f, chunkSize.y / 2f, chunkSize.z / 2f), chunkSize);
        }
        else if (Initialized && Dirty)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + new Vector3(chunkSize.x / 2f, chunkSize.y / 2f, chunkSize.z / 2f), chunkSize);
        }
    }
}
