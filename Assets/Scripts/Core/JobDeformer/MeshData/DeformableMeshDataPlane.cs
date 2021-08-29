using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.JobDeformer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class DeformableMeshDataPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<VertexData> _vertexData;
        private NativeArray<VertexData> _vertexDataOutput;
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private Mesh.MeshData _meshDataOutput;
        private VertexAttributeDescriptor[] _layout;

        private bool _scheduled;
        private MeshDataDeformerJob _job;
        private JobHandle _handle;

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = gameObject.GetComponent<MeshCollider>();
            CreateVertexDataArray();
            CreateVertexAttributeDescriptor();
        }

        private void CreateVertexAttributeDescriptor()
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


        public override void Deform(Vector3 positionToDeform)
        {
            _positionToDeform = transform.InverseTransformPoint(positionToDeform);
            var data = _meshDataArray[0];
            _meshDataOutput = CreateMeshDataOutput(data);
            _vertexDataOutput = _meshDataOutput.GetVertexData<VertexData>();
            ModifyVertexData();
        }

        private Mesh.MeshData CreateMeshDataOutput(Mesh.MeshData data)
        {
            _meshDataArrayOutput = Mesh.AllocateWritableMeshData(1);
            var dataOutput = _meshDataArrayOutput[0];
            dataOutput.SetIndexBufferParams(data.GetSubMesh(0).indexCount, data.indexFormat);
            dataOutput.SetVertexBufferParams(data.vertexCount, _layout);
            return dataOutput;
        }

        private void ModifyVertexData()
        {
            ScheduleJob();
        }

        private void LateUpdate()
        {
            CompleteJob();
        }

        private void ScheduleJob()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _job = new MeshDataDeformerJob(
                _radiusOfDeformation,
                _powerOfDeformation,
                _positionToDeform,
                _vertexData,
                _vertexDataOutput);
            _handle = _job.Schedule(_vertexData.Length, 64);
        }

        private void CompleteJob()
        {
            if (!_scheduled)
            {
                return;
            }

            _handle.Complete();
            var outputTris = CreateOutputTriangles(_meshDataOutput, _meshDataArray[0]);
            ApplyMeshData(_meshDataOutput, outputTris);
            _scheduled = false;
        }

        private NativeArray<ushort> CreateOutputTriangles(Mesh.MeshData dataOutput, Mesh.MeshData data)
        {
            var outputTris = dataOutput.GetIndexData<ushort>();
            var indexCount = data.GetSubMesh(0).indexCount;
            var tris = data.GetIndexData<ushort>();
            for (ushort i = 0; i < indexCount; i++)
            {
                outputTris[i] = tris[i];
            }

            return outputTris;
        }

        private void ApplyMeshData(Mesh.MeshData dataOutput, NativeArray<ushort> outputTris)
        {
            dataOutput.subMeshCount = 1;
            dataOutput.SetSubMesh(0, new SubMeshDescriptor(0, outputTris.Length),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            if (_vertexDataOutput[0].Position == Vector3.zero)
            {
                return;
            }
            Mesh.ApplyAndDisposeWritableMeshData(_meshDataArrayOutput, _mesh,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            _collider.sharedMesh = _mesh;
        }

        private void OnDestroy()
        {
            _vertexData.Dispose();
        }
    }
}