using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PointClipperComponent : MonoBehaviour
{
    
    void OnEnable()
    {
        PointClipperSystem.AddMesh (GetComponent<MeshFilter>().mesh, transform.position);
    }
    void OnDisable()
    {
        PointClipperSystem.RemoveMesh (GetComponent<MeshFilter>().mesh);
    }
}
