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
        _mesh = InitializeMesh();
    }
    void OnDisable()
    {
        _vertices.Dispose();
        _indices.Dispose();
    }
    public int decimator = 1;
    void Update()
    {
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
    Mesh InitializeMesh()
    {
        Debug.Log("init mesh");
        Mesh newMesh = new Mesh();
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;
        Debug.Log("copy stuff");
        // coppy vertices. Should it point to original mesh data?
        _vertices = new NativeArray<Vector3>(originalMesh.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < originalMesh.vertexCount; i++)
        {
            _vertices[i] = originalMesh.vertices[i];
        }
        newMesh.SetVertices(_vertices);
        Debug.Log("verts set");
        
        _indices = new NativeArray<int>(originalMesh.vertexCount / decimator, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (int i = 0; i < originalMesh.vertexCount / decimator; i++)
        {
            _indices[i] = i * decimator;
        }
        newMesh.SetIndices(_indices, MeshTopology.Points, 0);
        Debug.Log("indices set");

        System.Collections.Generic.List<Color32> colors = new System.Collections.Generic.List<Color32>(originalMesh.vertexCount);
        originalMesh.GetColors(colors);
        newMesh.SetColors(colors);

        newMesh.UploadMeshData(false);
        
        GetComponent<MeshFilter>().mesh = newMesh;

        return newMesh;
    }
}
