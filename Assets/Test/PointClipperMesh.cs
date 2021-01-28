using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PointClipperMesh : MonoBehaviour
{
    [SerializeField, Tooltip("Axis aligned bounding box to clip")]
    Bounds clipAABB = new Bounds(Vector3.zero, Vector3.one);
    
    NativeArray<Vector3> _vertices;
    Mesh _mesh;

    void OnEnable()
    {
        // initialize native arrays with mesh data
        _mesh = GetComponent<MeshFilter>().mesh;
        using(var dataArray = Mesh.AcquireReadOnlyMeshData(_mesh))
        {
            var data = dataArray[0];
            _vertices = new NativeArray<Vector3>(_mesh.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(_vertices);
        }
        Debug.Log("there are " + _vertices.Length + " verts");
    }
    void OnDisable()
    {
        _vertices.Dispose();
    }
    
    void Update()
    {
        NativeList<int> indices = new NativeList<int>(1, Allocator.TempJob);
        var job = new ClipAABBJob { verticse = _vertices, indices = indices, clipAABBMin = clipAABB.min, clipAABBMax = clipAABB.max };
        
        job.Schedule(_vertices.Length, default).Complete();

        // for(int i=0; i<_vertices.Length; i++)
        // {
        //     job.Execute(i);
        // }

        _mesh.SetIndices(indices.AsArray(), MeshTopology.Points, 0);
        indices.Dispose();
    }

    [BurstCompile]
    struct ClipAABBJob : IJobFor
    {
        [ReadOnly] public NativeArray<Vector3> verticse;
        [WriteOnly] public NativeList<int> indices;
        [ReadOnly] public Vector3 clipAABBMin;
        [ReadOnly] public Vector3 clipAABBMax;
		public void Execute(int index)
		{
            Vector3 pt = verticse[index];
            if( pt.x > clipAABBMin.x && pt.x < clipAABBMax.x &&
                pt.y > clipAABBMin.y && pt.y < clipAABBMax.y &&
                pt.z > clipAABBMin.z && pt.z < clipAABBMax.z )
            {
                indices.Add(index);
            }
		}
    }
    // Mesh UpdateMesh()
    // {
    //     for (int i = 0; i < _mesh.vertexCount / decimator; i++)
    //     {
    //         _indices[i] = i * decimator;
    //     }
    //     _mesh.SetIndices(_indices, MeshTopology.Points, 0);

    //     return _mesh;
    // }
}
