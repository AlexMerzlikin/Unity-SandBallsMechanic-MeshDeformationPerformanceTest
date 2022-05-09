using Core.JobDeformer;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.ComputeShaderDeformer
{
    /// <summary>
    /// Same as <see cref="ComputeShaderAsyncGpuReadbackDeformablePlane"/>
    /// But also lerps points between each in the list to compensate very fast mouse movement skipping large areas between frames 
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class LerpComputeShaderAsyncGpuReadbackDeformablePlane : DeformablePlane
    {
        [SerializeField] private ComputeShader _computeShader;
        [SerializeField] private int _lerpedDeformationPointsArraySize = 20;
        [SerializeField] private int _bufferingFrames = 2;

        private Mesh _mesh;
        private ComputeBuffer _computeBuffer;
        private int _kernel;
        private int _dispatchCount;
        private NativeArray<VertexData> _vertexData;
        private AsyncGPUReadbackRequest _request;
        private bool _isDispatched;
        private MeshCollider _meshCollider;
        private NativeList<Vector4> _deformationPointsBuffer;
        private NativeList<Vector4> _deformationPoints;
        private Vector3[] _lerpedDeformationPoints;
        private int _previousInputFrame;
        private Vector3 _previousInput;

        private readonly int _deformationPointsPropertyId = Shader.PropertyToID("_DeformPositions");
        private readonly int _deformationPointsCountPropertyId = Shader.PropertyToID("_DeformPositionsCount");

        public override void Deform(Vector3 positionToDeform)
        {
            var newPoint = transform.InverseTransformPoint(positionToDeform);
            if (_previousInputFrame == 0 || _previousInputFrame + _bufferingFrames <= Time.frameCount)
            {
                _deformationPointsBuffer.Add(newPoint);
                return;
            }

            if (_previousInput == newPoint)
            {
                return;
            }

            var distance = Vector3.Distance(_previousInput, newPoint);
            if (distance < _radiusOfDeformation)
            {
                return;
            }

            var pointsCount = PointsUtility.GetAllPointsBetween(
                _lerpedDeformationPoints,
                _previousInput,
                newPoint,
                _radiusOfDeformation * 0.6f);
            if (pointsCount == 0)
            {
                _deformationPointsBuffer.Add(newPoint);
                return;
            }

            for (var i = 0; i < pointsCount; i++)
            {
                _deformationPointsBuffer.Add(_lerpedDeformationPoints[i]);
            }
        }

        private void Awake()
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                gameObject.SetActive(false);
                return;
            }

            var meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = meshFilter.mesh;

            _deformationPointsBuffer = new NativeList<Vector4>(Allocator.Persistent);
            _deformationPoints = new NativeList<Vector4>(Allocator.Persistent);
            _lerpedDeformationPoints = new Vector3[_lerpedDeformationPointsArraySize];
            SetKernel();
            CreateVertexData();
            SetMeshVertexBufferParams();
            _computeBuffer = CreateComputeBuffer();
            SetComputeShaderValues();
        }

        private void Update()
        {
            Dispatch();
        }

        private void LateUpdate()
        {
            GatherResult();
        }

        private void SetKernel()
        {
            _kernel = _computeShader.FindKernel("CSMain");
            _computeShader.GetKernelThreadGroupSizes(_kernel, out var threadX, out _, out _);
            _dispatchCount = Mathf.CeilToInt(_mesh.vertexCount / threadX + 1);
        }

        private void CreateVertexData()
        {
            var meshVertexCount = _mesh.vertexCount;
            _vertexData = new NativeArray<VertexData>(meshVertexCount, Allocator.Temp);
            var meshVertices = _mesh.vertices;
            var meshNormals = _mesh.normals;
            var meshUV = _mesh.uv;
            for (var i = 0; i < meshVertexCount; ++i)
            {
                var v = new VertexData
                {
                    Position = meshVertices[i],
                    Normal = meshNormals[i],
                    Uv = meshUV[i]
                };
                _vertexData[i] = v;
            }
        }

        private void SetMeshVertexBufferParams()
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            _mesh.SetVertexBufferParams(_mesh.vertexCount, layout);
        }

        private void SetComputeShaderValues()
        {
            _computeShader.SetBuffer(_kernel, "vertexBuffer", _computeBuffer);
            _computeShader.SetFloat("_Force", _powerOfDeformation);
            _computeShader.SetFloat("_Radius", _radiusOfDeformation);
        }

        private ComputeBuffer CreateComputeBuffer()
        {
            var computeBuffer = new ComputeBuffer(_mesh.vertexCount, 32);
            if (_vertexData.IsCreated)
            {
                computeBuffer.SetData(_vertexData);
            }

            return computeBuffer;
        }

        private void Dispatch()
        {
            if (_deformationPointsBuffer.Length == 0 || _isDispatched)
            {
                return;
            }
            _isDispatched = true;

            SetDeformationPoints();

            _computeShader.SetVectorArray(_deformationPointsPropertyId, _deformationPoints.ToArray());
            _computeShader.SetInt(_deformationPointsCountPropertyId, _deformationPoints.Length);
            _computeShader.Dispatch(_kernel, _dispatchCount, 1, 1);
            _request = AsyncGPUReadback.Request(_computeBuffer);
        }

        private void SetDeformationPoints()
        {
            _deformationPoints.AddRange(_deformationPointsBuffer);
            _previousInputFrame = Time.frameCount;
            _previousInput = _deformationPointsBuffer[^1];
            _deformationPointsBuffer.Clear();
        }

        private void GatherResult()
        {
            if (!_isDispatched || !_request.done || _request.hasError)
            {
                return;
            }

            _isDispatched = false;
            _vertexData = _request.GetData<VertexData>();

            _mesh.MarkDynamic();
            _mesh.SetVertexBufferData(_vertexData, 0, 0, _vertexData.Length);
            _meshCollider.sharedMesh = _mesh;
            _deformationPoints.Clear();
        }


        private void CleanUp()
        {
            _computeBuffer?.Release();
            _deformationPointsBuffer.Dispose();
            _deformationPoints.Dispose();
        }

        private void OnDestroy()
        {
            CleanUp();
        }
    }
}