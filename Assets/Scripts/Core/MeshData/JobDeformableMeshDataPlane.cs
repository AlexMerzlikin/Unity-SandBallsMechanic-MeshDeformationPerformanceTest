using Core.JobDeformer;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.MeshData
{
    /// <summary>
    /// Jobified mesh deformation using MeshData API
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class JobDeformableMeshDataPlane : DeformablePlane
    {
        [SerializeField] private int _innerloopBatchCount = 64;

        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<VertexData> _vertexData;
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private VertexAttributeDescriptor[] _layout;
        private SubMeshDescriptor _subMeshDescriptor;
        private ProcessMeshDataJob _job;
        private JobHandle _jobHandle;
        private bool _scheduled;
        private bool _hasPoint;
        private NativeArray<ushort> _sourceIndexData;
        private NativeArray<ushort> _outputIndexData;

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            CreateVertexDataArray();
            _meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            _layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            _sourceIndexData = _meshDataArray[0].GetIndexData<ushort>();
            _subMeshDescriptor =
                new SubMeshDescriptor(0, _meshDataArray[0].GetSubMesh(0).indexCount, MeshTopology.Triangles)
                {
                    firstVertex = 0, vertexCount = _meshDataArray[0].vertexCount
                };
            _collider = gameObject.GetComponent<MeshCollider>();
        }

        public override void Deform(Vector3 positionToDeform)
        {
            _positionToDeform = transform.InverseTransformPoint(positionToDeform);
            _hasPoint = true;
        }

        private void CreateVertexDataArray()
        {
            _vertexData = new NativeArray<VertexData>(_mesh.vertexCount, Allocator.Persistent);
            for (var i = 0; i < _mesh.vertexCount; ++i)
            {
                var v = new VertexData
                {
                    Position = _mesh.vertices[i],
                    Normal = _mesh.normals[i],
                    Uv = _mesh.uv[i]
                };
                _vertexData[i] = v;
            }
        }

        private void Update()
        {
            ScheduleJob();
        }

        private void LateUpdate()
        {
            CompleteJob();
        }

        private void ScheduleJob()
        {
            if (_scheduled || !_hasPoint)
            {
                return;
            }

            _scheduled = true;
            _meshDataArrayOutput = Mesh.AllocateWritableMeshData(1);
            var outputMesh = _meshDataArrayOutput[0];
            var meshData = _meshDataArray[0];
            outputMesh.SetIndexBufferParams(meshData.GetSubMesh(0).indexCount, meshData.indexFormat);
            outputMesh.SetVertexBufferParams(meshData.vertexCount, _layout);
            _job = new ProcessMeshDataJob
            {
                Point = _positionToDeform,
                Radius = _radiusOfDeformation,
                Power = _powerOfDeformation,
                MeshData = _meshDataArray,
                VertexData = _vertexData,
                OutputMesh = outputMesh
            };

            _jobHandle = _job.Schedule(_job.MeshData[0].vertexCount, _innerloopBatchCount);
        }

        private void CompleteJob()
        {
            if (!_scheduled || !_hasPoint)
            {
                return;
            }

            _jobHandle.Complete();
            _outputIndexData = _job.OutputMesh.GetIndexData<ushort>();
            _sourceIndexData.CopyTo(_outputIndexData);
            _job.OutputMesh.subMeshCount = 1;
            _job.OutputMesh.SetSubMesh(0,
                _subMeshDescriptor,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            Mesh.ApplyAndDisposeWritableMeshData(
                _meshDataArrayOutput,
                _mesh,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            _collider.sharedMesh = _mesh;
            _scheduled = false;
            _hasPoint = false;
        }

        private void OnDestroy()
        {
            _vertexData.Dispose();
            _meshDataArray.Dispose();
        }
    }
}