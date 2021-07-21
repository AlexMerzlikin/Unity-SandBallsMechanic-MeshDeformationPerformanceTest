using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.JobDeformer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class DeformableMeshDataSingleThread : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<VertexData> _vertexData;
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private VertexAttributeDescriptor[] _layout;

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = gameObject.GetComponent<MeshCollider>();
            CreateVertexDataArray();
            CreateVertexAttributeDescriptor();
        }

        public override void Deform(Vector3 positionToDeform)
        {
            _positionToDeform = transform.InverseTransformPoint(positionToDeform);
            var data = _meshDataArray[0];
            var dataOutput = CreateMeshDataOutput(data);

            var vertexData = dataOutput.GetVertexData<VertexData>();
            ModifyVertexData(vertexData);
            var outputTris = CreateOutputTriangles(dataOutput, data);

            ApplyMeshData(dataOutput, outputTris);
        }

        private Mesh.MeshData CreateMeshDataOutput(Mesh.MeshData data)
        {
            _meshDataArrayOutput = Mesh.AllocateWritableMeshData(1);
            var dataOutput = _meshDataArrayOutput[0];
            dataOutput.SetIndexBufferParams(data.GetSubMesh(0).indexCount, data.indexFormat);
            dataOutput.SetVertexBufferParams(data.vertexCount, _layout);
            return dataOutput;
        }

        private void ApplyMeshData(Mesh.MeshData dataOutput, NativeArray<ushort> outputTris)
        {
            dataOutput.subMeshCount = 1;
            dataOutput.SetSubMesh(0, new SubMeshDescriptor(0, outputTris.Length),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            Mesh.ApplyAndDisposeWritableMeshData(_meshDataArrayOutput, _mesh,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            _collider.sharedMesh = _mesh;
        }

        private void ModifyVertexData(NativeArray<VertexData> vertexData)
        {
            for (var i = 0; i < _vertexData.Length; i++)
            {
                var distance = (_vertexData[i].Position - _positionToDeform).sqrMagnitude;
                var modifier = distance < _radiusOfDeformation ? 1 : 0;
                var v = new VertexData
                {
                    Position = _vertexData[i].Position - Vector3.up * modifier * _powerOfDeformation,
                    Normal = _vertexData[i].Normal,
                    Uv = _vertexData[i].Uv
                };
                _vertexData[i] = v;
                vertexData[i] = v;
            }
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

        private void OnDestroy()
        {
            _meshDataArray.Dispose();
        }
    }
}