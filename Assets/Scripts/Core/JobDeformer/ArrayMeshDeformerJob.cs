using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Core.JobDeformer
{
    [BurstCompile]
    public struct ArrayMeshDeformerJob : IJobParallelFor
    {
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;
        [ReadOnly] private NativeArray<float4> _deformationPoints;
        [ReadOnly] private readonly float4 _up;
        public NativeArray<float4> Vertices;

        public ArrayMeshDeformerJob(
            float radius,
            float power,
            NativeArray<float4> vertices,
            NativeArray<float4> deformationPoints)
        {
            _radius = radius;
            _power = power;
            Vertices = vertices;
            _deformationPoints = deformationPoints;
            _up = new float4(0, 1, 0, 0);
        }

        public void Execute(int index)
        {
            var vertex = Vertices[index];
            foreach (var point in _deformationPoints)
            {
                var dist = SquareMagnitude((vertex - point));
                if (dist < _radius)
                {
                    vertex -= _up * _power;
                    Vertices[index] = vertex;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SquareMagnitude(float4 vector)
        {
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }
    }
}