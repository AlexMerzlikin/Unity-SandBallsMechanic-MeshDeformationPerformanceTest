using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    public struct ArrayMeshDeformerJob : IJobParallelFor
    {
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;
        [ReadOnly] private NativeArray<Vector3> _deformationPoints;

        public NativeArray<Vector3> Vertices;

        public ArrayMeshDeformerJob(
            float radius,
            float power,
            NativeArray<Vector3> vertices,
            NativeArray<Vector3> deformationPoints)
        {
            _radius = radius;
            _power = power;
            Vertices = vertices;
            _deformationPoints = deformationPoints;
        }

        public void Execute(int index)
        {
            var vertex = Vertices[index];
            foreach (var point in _deformationPoints)
            {
                var dist = (vertex - point).sqrMagnitude;
                if (dist < _radius)
                {
                    vertex -= Vector3.up * _power;
                    Vertices[index] = vertex;
                } 
            }
        }
    }
}