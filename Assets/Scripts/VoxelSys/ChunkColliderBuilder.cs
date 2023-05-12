using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ChunkColliderBuilder : Singleton<ChunkColliderBuilder>
{

    struct MeshNode
    {
        public Chunk chunk;
        public Mesh mesh;
    }

    List<MeshNode> meshes = new List<MeshNode>();
    NativeArray<int> meshIds;
    JobHandle jobHandle;

    public void Enqueue(Chunk chunk, Mesh mesh)
    {
        meshes.Add(new MeshNode { chunk = chunk, mesh = mesh });
    }

    void Start()
    {
        StartCoroutine(nameof(BakeUpdator));
    }

    void OnDestroy()
    {
        jobHandle.Complete();

        if (meshIds.IsCreated)
            meshIds.Dispose();
    }

    IEnumerator BakeUpdator()
    {
        int counter = 0;
        while (true)
        {
            if (meshes.Count == 0)
            {
                counter = 0;
                yield return null;
                continue;
            }

            if (counter < 4 && meshes.Count < 5)
            {
                counter++;
                yield return null;
                continue;
            }

            meshIds = new NativeArray<int>(meshes.Count, Allocator.TempJob);

            for (int i = 0; i < meshes.Count; ++i)
            {
                meshIds[i] = meshes[i].mesh.GetInstanceID();
            }

            BakeJob bakeJob = new BakeJob { meshIds = meshIds };
            jobHandle = bakeJob.Schedule(meshIds.Length, 32);
            JobHandle.ScheduleBatchedJobs();
            int frameCount = 1;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4;
            });
            jobHandle.Complete();
            meshIds.Dispose();

            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].chunk.SetSharedMesh(meshes[i].mesh);
            }

            meshes.Clear();
            counter = 0;
            ChunkObjectSpawner.Main.Itemize();
            yield return null;
        }
    }

    [BurstCompile]
    public struct BakeJob : IJobParallelFor
    {
        public NativeArray<int> meshIds;

        public void Execute(int index)
        {
            Physics.BakeMesh(meshIds[index], false);
        }
    }
}