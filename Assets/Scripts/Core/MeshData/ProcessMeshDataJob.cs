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

        [ReadOnly] private readonly Mesh.MeshData _outputData;
        [ReadOnly] private readonly Mesh.MeshData _vertexData;
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;
        [ReadOnly] private readonly Vector3 _point;

        public ProcessMeshDataJob(Mesh.MeshData vertexData,
            Mesh.MeshData outputData,
            float radius,
            float power,
            Vector3 point)
        {
            _vertexData = vertexData;
            _outputData = outputData;
            _radius = radius;
            _power = power;
            _point = point;
        }

        public void Execute(int index)
        {
            var vertexData = _vertexData.GetVertexData<VertexData>()[index];
            var outputVertexData = _outputData.GetVertexData<VertexData>();
            var position = vertexData.Position;
            var distance = (position - _point).sqrMagnitude;
            var modifier = distance < _radius ? 1 : 0;
            outputVertexData[index] = new VertexData
            {
                Position = position - Up * modifier * _power,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };
        }
    }
}