using UnityEngine;
using Pcx;

[ExecuteInEditMode]
public class PointCliper : MonoBehaviour
{
    [SerializeField] PointCloudData _sourceData = null;
    [SerializeField] ComputeShader _computeShader = default;

    [SerializeField, Tooltip("Axis aligned bounding box to clip")]
    Bounds clipAABB = new Bounds(Vector3.zero, Vector3.one);

    // TODO: support clipping planes
    // https://gamedevelopment.tutsplus.com/tutorials/understanding-sutherland-hodgman-clipping-for-physics-engines--gamedev-11917
    
    static readonly int
        clipAABBMinID = Shader.PropertyToID("_clipAABBMin"),
        clipAABBMaxID = Shader.PropertyToID("_clipAABBMax"),
        sourceBufferID = Shader.PropertyToID("_sourceBuffer"),
        outputBufferID = Shader.PropertyToID("_outputBuffer");

    ComputeBuffer _pointBuffer;
    void Update()
    {
        if (_sourceData == null) return;

        var sourceBuffer = _sourceData.computeBuffer;

        if (_pointBuffer == null || _pointBuffer.count != sourceBuffer.count)
        {
            if (_pointBuffer != null) _pointBuffer.Release();
            _pointBuffer = new ComputeBuffer(sourceBuffer.count, PointCloudData.elementSize, ComputeBufferType.Append);
        }
        
        _pointBuffer.SetCounterValue(0);
        
        var kernel = _computeShader.FindKernel("ClipAABB");
        _computeShader.SetVector (clipAABBMinID, clipAABB.min);
        _computeShader.SetVector (clipAABBMaxID, clipAABB.max);
        _computeShader.SetBuffer (kernel, sourceBufferID, sourceBuffer);
        _computeShader.SetBuffer (kernel, outputBufferID, _pointBuffer);
        
        _computeShader.Dispatch(kernel, sourceBuffer.count / 128, 1, 1);


        GetComponent<PointCloudRenderer>().sourceBuffer = _pointBuffer;
    }
}
