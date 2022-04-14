using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    /// <summary>
    /// Modifies Vertices array using a single deformation point
    /// </summary>
    public struct MeshDeformerJob : IJobParallelFor
    {
        [ReadOnly] private readonly Vector3 _center;
        [ReadOnly] private readonly float _radius;
        [ReadOnly] private readonly float _power;

        public NativeArray<Vector3> Vertices;

        public MeshDeformerJob(
            Vector3 center,
            float radius,
            float power,
            NativeArray<Vector3> vertices)
        {
            _center = center;
            _radius = radius;
            _power = power;
            Vertices = vertices;
        }

        public void Execute(int index)
        {
            var vertex = Vertices[index];
            var dist = (vertex - _center).sqrMagnitude;
            if (dist < _radius)
            {
                vertex -= Vector3.up * _power;
                Vertices[index] = vertex;
            }
        }
    }
}