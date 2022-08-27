using Core.JobDeformer;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.MeshData
{
    [BurstCompile]
    public struct ProcessMeshDataJob : IJobParallelFor
    {
        private static readonly Vector3 Up = new Vector3(0, 1, 0);

        public Mesh.MeshData OutputMesh;
        [ReadOnly] private readonly NativeArray<VertexData> _vertexData;
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;
        [ReadOnly] private readonly Vector3 _point;

        public ProcessMeshDataJob(Mesh.MeshData outputMesh,
            NativeArray<VertexData> vertexData,
            float radius,
            float power,
            Vector3 point)
        {
            OutputMesh = outputMesh;
            _vertexData = vertexData;
            _radius = radius;
            _power = power;
            _point = point;
        }

        public void Execute(int index)
        {
            var outputVertices = OutputMesh.GetVertexData<VertexData>();
            var vertexData = _vertexData[index];
            var position = vertexData.Position;
            var distance = (position - _point).sqrMagnitude;
            var modifier = distance < _radius ? 1 : 0;
            outputVertices[index] = new VertexData
            {
                Position = position - Up * modifier * _power,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };
        }
    }
}