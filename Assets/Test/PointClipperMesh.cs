using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PointClipperMesh : MonoBehaviour
{
    [SerializeField, Tooltip("Axis aligned bounding box to clip")]
    Bounds clipAABB = new Bounds(Vector3.zero, Vector3.one);
    
    NativeArray<Vector3> _vertices;
    NativeArray<int> _indices;
    Mesh _mesh;

    void OnEnable()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _indices = new NativeArray<int>(_mesh.vertexCount / decimator, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        UpdateMesh();
    }
    void OnDisable()
    {
        // _vertices.Dispose();
        _indices.Dispose();
    }
    public int decimator = 1;
    void Update()
    {
        // dont create a new array here. probably need a native list
        // _indices.Dispose();
        _indices = new NativeArray<int>(_mesh.vertexCount / decimator, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        UpdateMesh();

        // _indices = new NativeArray<int>(_mesh.vertexCount / decimator, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        // for (int i = 0; i < _mesh.vertexCount / decimator; i++)
        // {
        //     _indices[i] = i * decimator;
        // }
        // _mesh.SetIndices(_indices, MeshTopology.Points, 0);


        // var job = new ClipAABBJob { indices = _indices, clipAABBMin = clipAABB.min, clipAABBMax = clipAABB.max };
        // _mesh.SetIndices(_indices, MeshTopology.Points, 0);
    }

    // [BurstCompile]
    // struct ClipAABBJob : IJobParallelFor
    // {
    //     [ReadOnly] public NativeArray<Vector3> verticse;
    //     public NativeArray<int> indices;
    //     public Vector3 clipAABBMin;
    //     public Vector3 clipAABBMax;
	// 	public void Execute(int index)
	// 	{

	// 	}
    // }
    Mesh UpdateMesh()
    {
        for (int i = 0; i < _mesh.vertexCount / decimator; i++)
        {
            _indices[i] = i * decimator;
        }
        _mesh.SetIndices(_indices, MeshTopology.Points, 0);

        return _mesh;
    }
}
