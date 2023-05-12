using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class ChunkMeshBuilder
{
    public static readonly int2 AtlasSize = VoxelUtil.AtlasSize;
    //private static readonly int nonBlocks = VoxelUtil.nonBlocks;

    public static void InitializeShaderParameter()
    {
        Shader.SetGlobalInt("_AtlasX", AtlasSize.x);
        Shader.SetGlobalInt("_AtlasY", AtlasSize.y);
        Shader.SetGlobalVector("_AtlasRec", new Vector4(1.0f / AtlasSize.x, 1.0f / AtlasSize.y));
    }

    public enum SimplifyingMethod
    {
        Culling,
        GreedyOnlyHeight,
        Greedy
    };

    public class NativeMeshData
    {

        NativeArray<Voxel> nativeVoxels;
        public NativeArray<float3> nativeVertices;
        public NativeArray<float3> nativeNormals;
        public NativeArray<int> nativeIndices;
        public NativeArray<float4> nativeUVs;
        public JobHandle jobHandle;
        NativeCounter faceCounter;

        public NativeArray<int3> nativeGroundVoxels;
        public NativeCounter groundCounter;

        public NativeMeshData(int3 chunkSize)
        {
            int numVoxels = chunkSize.x * chunkSize.y * chunkSize.z;
            int maxVertices = 12 * numVoxels;
            int maxIndices = 18 * numVoxels;

            nativeVoxels = new NativeArray<Voxel>(numVoxels, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeVertices = new NativeArray<float3>(maxVertices, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeNormals = new NativeArray<float3>(maxVertices, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeUVs = new NativeArray<float4>(maxVertices, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeIndices = new NativeArray<int>(maxIndices, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            faceCounter = new NativeCounter(Allocator.TempJob);

            nativeGroundVoxels = new NativeArray<int3>(numVoxels / 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            groundCounter = new NativeCounter(Allocator.TempJob);
        }

        ~NativeMeshData()
        {
            jobHandle.Complete();
            Dispose();
        }

        public void Dispose()
        {
            if (nativeVoxels.IsCreated)
                nativeVoxels.Dispose();

            if (nativeVertices.IsCreated)
                nativeVertices.Dispose();

            if (nativeNormals.IsCreated)
                nativeNormals.Dispose();

            if (nativeIndices.IsCreated)
                nativeIndices.Dispose();

            if (faceCounter.IsCreated)
                faceCounter.Dispose();

            if (nativeUVs.IsCreated)
                nativeUVs.Dispose();

            if (nativeGroundVoxels.IsCreated)
                nativeGroundVoxels.Dispose();

            if (groundCounter.IsCreated)
                groundCounter.Dispose();
        }

        public IEnumerator ScheduleMeshingJob(Voxel[] voxels, int3 chunkSize, bool argent = false)
        {
            nativeVoxels.CopyFrom(voxels);
            ScheduleGreedyJob(nativeVoxels, chunkSize);

            int frameCount = 0;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4 || argent; // Seems very important.
            });

            jobHandle.Complete();
        }

        public void GetMeshInformation(out int verticeSize, out int indicesSize)
        {
            verticeSize = faceCounter.Count * 4;
            indicesSize = faceCounter.Count * 6;
        }

        void ScheduleGreedyJob(NativeArray<Voxel> voxels, int3 chunkSize)
        {
            VoxelGreedyMeshingJob voxelMeshingJob = new VoxelGreedyMeshingJob
            {
                voxels = voxels,
                chunkSize = chunkSize,
                vertices = nativeVertices,
                normals = nativeNormals,
                uvs = nativeUVs,
                indices = nativeIndices,
                faceCounter = faceCounter,
                groundVoxels = nativeGroundVoxels,
                groundCounter = groundCounter
            };

            jobHandle = voxelMeshingJob.Schedule();
            JobHandle.ScheduleBatchedJobs();
        }
    }

    [BurstCompile]
    struct VoxelGreedyMeshingJob : IJob
    {
        [ReadOnly] public NativeArray<Voxel> voxels;
        [ReadOnly] public int3 chunkSize;

        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float3> vertices;

        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float3> normals;

        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<float4> uvs;

        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<int> indices;

        [WriteOnly] public NativeCounter faceCounter;


        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<int3> groundVoxels;

        [WriteOnly] public NativeCounter groundCounter;

        struct Empty { }

        // Left here
        public void Execute()
        {
            // For each dir
            for (int direction = 0; direction < 6; direction++)
            {
                // Contains inspected blocks.
                NativeParallelHashMap<int3, Empty> inspected = new NativeParallelHashMap<int3, Empty>(chunkSize[VoxelUtil.DirectionAlignedX[direction]] * chunkSize[VoxelUtil.DirectionAlignedY[direction]], Allocator.Temp);

                // Goes through depth of the chunk (z) swiping on x and y axis.
                for (int depth = 0; depth < chunkSize[VoxelUtil.DirectionAlignedZ[direction]]; depth++)
                {
                    for (int x = 0; x < chunkSize[VoxelUtil.DirectionAlignedX[direction]]; x++)
                    {
                        for (int y = 0; y < chunkSize[VoxelUtil.DirectionAlignedY[direction]];)
                        {
                            // Get block grid position and block itself
                            int3 gridPosition = new int3 { [VoxelUtil.DirectionAlignedX[direction]] = x, [VoxelUtil.DirectionAlignedY[direction]] = y, [VoxelUtil.DirectionAlignedZ[direction]] = depth };

                            Voxel voxel = voxels[VoxelUtil.To1DIndex(gridPosition, chunkSize)];

                            if (voxel == Voxel.Air)
                            {
                                y++;
                                continue;
                            }

                            if (inspected.ContainsKey(gridPosition))
                            {
                                y++;
                                continue;
                            }

                            // Block is not air and wasn't inspected before!

                            // Get neighbour in current direction.
                            int3 neighborPosition = gridPosition + VoxelUtil.VoxelDirectionOffsets[direction];

                            if (!IsTransparent(voxels, neighborPosition, chunkSize))
                            {
                                // If neighbour is not transparent, no face needed
                                y++;
                                continue;
                            }

                            // Neighbour is transparent so face must be created!

                            // Adds block to inspected set
                            inspected.TryAdd(gridPosition, new Empty());

                            // Expands quad as much as possible
                            int height;
                            for (height = 1; height + y < chunkSize[VoxelUtil.DirectionAlignedY[direction]]; height++)
                            {
                                int3 nextPosition = gridPosition;
                                nextPosition[VoxelUtil.DirectionAlignedY[direction]] += height;

                                Voxel nextVoxel = voxels[VoxelUtil.To1DIndex(nextPosition, chunkSize)];

                                if (nextVoxel != voxel)
                                    break;

                                if (inspected.ContainsKey(nextPosition))
                                    break;

                                inspected.TryAdd(nextPosition, new Empty());
                            }

                            bool isDone = false;
                            int width;
                            for (width = 1; width + x < chunkSize[VoxelUtil.DirectionAlignedX[direction]]; width++)
                            {
                                for (int dy = 0; dy < height; dy++)
                                {
                                    int3 nextPosition = gridPosition;
                                    nextPosition[VoxelUtil.DirectionAlignedX[direction]] += width;
                                    nextPosition[VoxelUtil.DirectionAlignedY[direction]] += dy;

                                    Voxel nextVoxel = voxels[VoxelUtil.To1DIndex(nextPosition, chunkSize)];

                                    if (nextVoxel != voxel || inspected.ContainsKey(nextPosition))
                                    {
                                        isDone = true;
                                        break;
                                    }
                                }

                                if (isDone)
                                {
                                    break;
                                }

                                for (int dy = 0; dy < height; dy++)
                                {
                                    int3 nextPosition = gridPosition;
                                    nextPosition[VoxelUtil.DirectionAlignedX[direction]] += width;
                                    nextPosition[VoxelUtil.DirectionAlignedY[direction]] += dy;
                                    inspected.TryAdd(nextPosition, new Empty());
                                }
                            }

                            AddQuadByDirection(direction, voxel, width, height, gridPosition, faceCounter.Increment(), vertices, normals, uvs, indices, groundVoxels, groundCounter);
                            y += height;
                        }
                    }

                    inspected.Clear();
                }
                inspected.Dispose();
            }
        }
    }

    public static bool IsTransparent(NativeArray<Voxel> voxels, int3 position, int3 chunkSize)
    {
        if (!VoxelUtil.BoundaryCheck(position, chunkSize))
            return true;

        return voxels[VoxelUtil.To1DIndex(position, chunkSize)] == Voxel.Air;
    }

    static unsafe void AddQuadByDirection(int direction, Voxel data, float width, float height, int3 gridPosition, int faceIndex, NativeArray<float3> vertices, NativeArray<float3> normals, NativeArray<float4> uvs, NativeArray<int> indices, NativeArray<int3> groundVoxels, NativeCounter groundCounter)
    {
        int firstVertexIndex = faceIndex * 4;
        for (int i = 0; i < 4; i++)
        {
            float3 vertex = VoxelUtil.CubeVertices[VoxelUtil.CubeFaces[i + direction * 4]];
            vertex[VoxelUtil.DirectionAlignedX[direction]] *= width;
            vertex[VoxelUtil.DirectionAlignedY[direction]] *= height;

            // Determines which texture get picked from atlas
            int atlasIndex = (int)data * 6 + direction; // Direction = 0,1,4,5:sides 2:up 3:down
            int2 atlasPosition = new int2 { x = atlasIndex % AtlasSize.x, y = atlasIndex / AtlasSize.x };

            float4 uv = new float4 { x = VoxelUtil.CubeUVs[i].x * width, y = VoxelUtil.CubeUVs[i].y * height, z = atlasPosition.x, w = atlasPosition.y };

            vertices[firstVertexIndex + i] = vertex + gridPosition;
            normals[firstVertexIndex + i] = VoxelUtil.VoxelDirectionOffsets[direction];
            uvs[firstVertexIndex + i] = uv;
        }
        // For each face -> 2 tris -> 6 indices
        int firstIndiceIndex = faceIndex * 6;
        // Figure out wtf was happening here before
        for (int i = 0; i < 6; i++)
        {
            // Compare the light sum of each pair of vertices.
            indices[firstIndiceIndex + i] = VoxelUtil.CubeIndices[direction * 6 + i] + firstVertexIndex;
        }

        if (direction == 2 && gridPosition.y < Consts.ChunkSizeVertical - 1)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    groundVoxels[groundCounter.Increment()] = new int3(gridPosition.x + i, gridPosition.y, gridPosition.z + j);
                }
            }
        }
    }
}