using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

public class PointClipperSystem : MonoBehaviour
{
    [SerializeField, Tooltip("Axis aligned bounding box to clip")]
    Bounds clipAABB = new Bounds(Vector3.zero, Vector3.one);

    static HashSet<Mesh> _mesheEntitiesSet = new HashSet<Mesh>();
    static Dictionary<Mesh, NativeArray<float3>> _verticesDict = new Dictionary<Mesh, NativeArray<float3>>();
    static Dictionary<Mesh, NativeList<int>> _indicesDict = new Dictionary<Mesh, NativeList<int>>();

    List<JobHandle> jobHandles = new List<JobHandle>();

    public static void AddMesh(Mesh m)
    {
        if (!_mesheEntitiesSet.Contains(m))
        {
            using(var dataArray = Mesh.AcquireReadOnlyMeshData(m))
            {
                var data = dataArray[0];
                NativeArray<float3> vertices = new NativeArray<float3>(m.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                data.GetVertices(vertices.Reinterpret<Vector3>());
                _verticesDict.Add(m, vertices);
            }
            NativeList<int> indices = new NativeList<int>(m.vertexCount, Allocator.Persistent);
            _indicesDict.Add(m, indices);
            _mesheEntitiesSet.Add(m);
        }
    }

    public static void RemoveMesh(Mesh m)
    {
        if (_mesheEntitiesSet.Contains(m))
        {
            _verticesDict[m].Dispose();
            _verticesDict.Remove(m);
            _indicesDict[m].Dispose();
            _indicesDict.Remove(m);
            _mesheEntitiesSet.Remove(m);
        }
    }

    void Update()
    {
        jobHandles.Clear();
        foreach (Mesh m in _mesheEntitiesSet)
        {
            _indicesDict[m].Clear();
            var job = new ClipAABBJob { verticse = _verticesDict[m], indices = _indicesDict[m], clipAABBMin = clipAABB.min, clipAABBMax = clipAABB.max };
            jobHandles.Add(job.Schedule(_verticesDict[m].Length, default));
        }
    }

    void LateUpdate()
    {
        foreach(JobHandle handle in jobHandles)
        {
            handle.Complete();
        }
        foreach (Mesh m in _mesheEntitiesSet)
        {
            m.SetIndices(_indicesDict[m].AsArray(), MeshTopology.Points, 0);
        }
    }

    [BurstCompile]
    struct ClipAABBJob : IJobFor
    {
        [ReadOnly] public NativeArray<float3> verticse;
        [WriteOnly] public NativeList<int> indices;
        [ReadOnly] public float3 clipAABBMin;
        [ReadOnly] public float3 clipAABBMax;
		public void Execute(int index)
		{
            float3 pt = verticse[index];
            if( pt.x > clipAABBMin.x && pt.x < clipAABBMax.x &&
                pt.y > clipAABBMin.y && pt.y < clipAABBMax.y &&
                pt.z > clipAABBMin.z && pt.z < clipAABBMax.z )
            {
                indices.Add(index);
            }
		}
    }

}
