using UnityEngine;
using Pcx;

[ExecuteInEditMode]
public class PointCliper : MonoBehaviour
{
    [SerializeField] PointCloudData _sourceData = null;
    [SerializeField] ComputeShader _computeShader = null;

    [SerializeField] Bounds clipBounds;

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
        
        var kernel = _computeShader.FindKernel("Main");
        _computeShader.SetVector ("clipMin", clipBounds.min);
        _computeShader.SetVector ("clipMax", clipBounds.max);
        _computeShader.SetBuffer(kernel, "SourceBuffer", sourceBuffer);
        _computeShader.SetBuffer(kernel, "OutputBuffer", _pointBuffer);
        _computeShader.Dispatch(kernel, sourceBuffer.count / 128, 1, 1);


        GetComponent<PointCloudRenderer>().sourceBuffer = _pointBuffer;
    }
}
