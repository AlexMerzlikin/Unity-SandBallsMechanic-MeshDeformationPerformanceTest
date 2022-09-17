using Core.JobDeformer;
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
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private VertexAttributeDescriptor[] _layout;
        private SubMeshDescriptor _subMeshDescriptor;
        private ProcessMeshDataJob _job;
        private JobHandle _jobHandle;
        private bool _scheduled;
        private bool _hasPoint;

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            CreateMeshData();
            _collider = gameObject.GetComponent<MeshCollider>();
        }

        private void CreateMeshData()
        {
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
            _subMeshDescriptor =
                new SubMeshDescriptor(0, _meshDataArray[0].GetSubMesh(0).indexCount, MeshTopology.Triangles)
                {
                    firstVertex = 0, vertexCount = _meshDataArray[0].vertexCount
                };
        }

        public override void Deform(Vector3 positionToDeform)
        {
            _positionToDeform = transform.InverseTransformPoint(positionToDeform);
            _hasPoint = true;
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
            _meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            var meshData = _meshDataArray[0];
            outputMesh.SetIndexBufferParams(meshData.GetSubMesh(0).indexCount, meshData.indexFormat);
            outputMesh.SetVertexBufferParams(meshData.vertexCount, _layout);
            _job = new ProcessMeshDataJob
            (
                meshData,
                outputMesh,
                _radiusOfDeformation,
                _powerOfDeformation,
                _positionToDeform
            );

            _jobHandle = _job.Schedule(meshData.vertexCount, _innerloopBatchCount);
        }

        private void CompleteJob()
        {
            if (!_scheduled || !_hasPoint)
            {
                return;
            }

            _jobHandle.Complete();
            UpdateMesh(_meshDataArrayOutput[0]);
            _scheduled = false;
            _hasPoint = false;
        }

        private void UpdateMesh(Mesh.MeshData meshData)
        {
            var outputIndexData = meshData.GetIndexData<ushort>();
            _meshDataArray[0].GetIndexData<ushort>().CopyTo(outputIndexData);
            _meshDataArray.Dispose();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,
                _subMeshDescriptor,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontNotifyMeshUsers);
            _mesh.MarkDynamic();
            Mesh.ApplyAndDisposeWritableMeshData(
                _meshDataArrayOutput,
                _mesh,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontNotifyMeshUsers);
            _mesh.RecalculateNormals();
            _collider.sharedMesh = _mesh;
        }
    }
}