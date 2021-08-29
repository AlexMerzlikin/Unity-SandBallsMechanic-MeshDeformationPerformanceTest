using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    public struct MeshDataDeformerJob : IJobParallelFor
    {
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;
        [ReadOnly] private readonly Vector3 _point;

        private NativeArray<VertexData> _vertexData;
        private NativeArray<VertexData> _vertexDataOutput;

        public MeshDataDeformerJob(
            float radius,
            float power,
            Vector3 point,
            NativeArray<VertexData> vertexData,
            NativeArray<VertexData> vertexDataOutput)
        {
            _radius = radius;
            _power = power;
            _point = point;
            _vertexData = vertexData;
            _vertexDataOutput = vertexDataOutput;
        }

        public void Execute(int index)
        {
            var vertexData = _vertexData[index];
            var distance = (vertexData.Position - _point).sqrMagnitude;
            var modifier = distance < _radius ? 1 : 0;
            var v = new VertexData
            {
                Position = vertexData.Position - Vector3.up * modifier * _power,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };

            _vertexDataOutput[index] = v;
            _vertexData[index] = v;
        }
    }
}