using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class NoiseGenerator
{
    //static void RandomVoxel(out Voxel voxel, int3 worldPosition)
    //{
    //    voxel = new Voxel();
    //    int density = -worldPosition.y + 64;

    //    density += (int)(SimplexNoise.Noise.CalcPixel2DFractal(worldPosition.x, worldPosition.z, 0.003f, 1) * 25f);
    //    density += (int)(SimplexNoise.Noise.CalcPixel2DFractal(worldPosition.x, worldPosition.z, 0.03f, 3) * 5f);
    //    density += (int)(SimplexNoise.Noise.CalcPixel2DFractal(worldPosition.x, worldPosition.z, 0.09f, 5) * 1f);

    //    int level = 0;
    //    if (density >= level)
    //    {
    //        voxel = Voxel.Grass;
    //        level += 1;
    //    }

    //    if (density >= level)
    //    {
    //        voxel = Voxel.Dirt;
    //        level += (int) (SimplexNoise.Noise.CalcPixel2DFractal(worldPosition.x, worldPosition.z, 0.01f, 1) * 10f) + 3;
    //    }

    //    if (density >= level)
    //        voxel = Voxel.Stone;
    //}

    static void RandomVoxel(out Voxel voxel, int3 worldPosition)
    {
        voxel = new Voxel();

        float noise = SimplexNoise.Noise.CalcPixel3DFractal(worldPosition.x, worldPosition.y, worldPosition.z, 0.03f, 3);

        if (noise < .65f)
        {
            float n1 = SimplexNoise.Noise.CalcPixel3DFractal(worldPosition.x, worldPosition.y, worldPosition.z, 0.09f, 3);

            if (n1 < .15f)
            {
                // Common resource
                voxel = Voxel.Brick;
                return;
            }
            voxel = Voxel.Stone;
            return;
        }
        if (noise > .9)
        {
            voxel = Voxel.Magic;
        }
    }

    [BurstCompile]
    struct GenerateNoiseJob : IJobParallelFor
    {
        [ReadOnly] public int3 chunkPosition;
        [ReadOnly] public int3 chunkSize;

        [WriteOnly] public NativeArray<Voxel> voxels;

        public void Execute(int index)
        {
            int3 gridPosition = VoxelUtil.To3DIndex(index, chunkSize);
            int3 worldPosition = gridPosition + chunkPosition * chunkSize;

            RandomVoxel(out Voxel voxel, worldPosition);

            voxels[index] = voxel;
        }
    }

    public class NativeVoxelData
    {
        NativeArray<Voxel> nativeVoxels;
        public JobHandle jobHandle;

        public NativeVoxelData(int3 chunkSize)
        {
            int numVoxels = chunkSize.x * chunkSize.y * chunkSize.z;
            nativeVoxels = new NativeArray<Voxel>(numVoxels, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        }

        ~NativeVoxelData()
        {
            jobHandle.Complete();
            Dispose();
        }

        public void Dispose()
        {
            if (nativeVoxels.IsCreated)
                nativeVoxels.Dispose();
        }

        public IEnumerator Generate(Voxel[] voxels, int3 chunkPosition, int3 chunkSize)
        {
            nativeVoxels.CopyFrom(voxels);

            GenerateNoiseJob noiseJob = new GenerateNoiseJob { chunkPosition = chunkPosition, chunkSize = chunkSize, voxels = nativeVoxels };
            jobHandle = noiseJob.Schedule(nativeVoxels.Length, 32);
            JobHandle.ScheduleBatchedJobs();
            int frameCount = 1;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4;
            });
            jobHandle.Complete();

            nativeVoxels.CopyTo(voxels);
            nativeVoxels.Dispose();
        }
    }


}