using System.Collections.Generic;
using Core.JobDeformer;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.ComputeShaderDeformer
{
    /// <summary>
    /// Modifies a mesh by:
    /// 1. Updating NativeArray<VertexData> in the compute shader
    /// 2. Getting the result via AsyncGPUReadback
    /// 3. Using mesh.SetVertexBufferData<VertexData> to set positions, normals, and UVs in a single call
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class ComputeShaderAsyncGpuReadbackDeformablePlane : DeformablePlane
    {
        [SerializeField] private ComputeShader _computeShader;

        private Mesh _mesh;
        private ComputeBuffer _computeBuffer;
        private int _kernel;
        private int _dispatchCount;
        private NativeArray<VertexData> _vertexData;
        private AsyncGPUReadbackRequest _request;
        private bool _isDispatched;
        private MeshCollider _meshCollider;
        private readonly List<Vector4> _deformationPoints = new List<Vector4>(30);
        private readonly int _deformationPointsPropertyId = Shader.PropertyToID("_DeformPositions");
        private readonly int _deformationPointsCountPropertyId = Shader.PropertyToID("_DeformPositionsCount");

        public override void Deform(Vector3 positionToDeform)
        {
            var point = transform.InverseTransformPoint(positionToDeform);
            _deformationPoints.Add(point);
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
            if (_deformationPoints.Count == 0 || _isDispatched)
            {
                return;
            }

            _isDispatched = true;
            _computeShader.SetVectorArray(_deformationPointsPropertyId, _deformationPoints.ToArray());
            _computeShader.SetInt(_deformationPointsCountPropertyId, _deformationPoints.Count);
            _computeShader.Dispatch(_kernel, _dispatchCount, 1, 1);
            _deformationPoints.Clear();
            _request = AsyncGPUReadback.Request(_computeBuffer);
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

            _request = AsyncGPUReadback.Request(_computeBuffer);
        }


        private void CleanUp()
        {
            _computeBuffer?.Release();
        }

        private void OnDestroy()
        {
            CleanUp();
        }
    }
}