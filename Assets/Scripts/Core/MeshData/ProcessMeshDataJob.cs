using Core.JobDeformer;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Core.MeshData
{
    [BurstCompile]
    public struct ProcessMeshDataJob : IJobParallelFor
    {
        private static readonly Vector3 Up = new Vector3(0, 1, 0);

        public NativeArray<VertexData> VertexData;
        public Mesh.MeshData OutputMesh;
        [ReadOnly] public Mesh.MeshDataArray MeshData;
        [ReadOnly] public float Radius;
        [ReadOnly] public float Power;
        [ReadOnly] public Vector3 Point;

        public void Execute(int index)
        {
            var outputVertices = OutputMesh.GetVertexData<VertexData>();
            var position = VertexData[index].Position;
            var distance = (position - Point).sqrMagnitude;
            var modifier = distance < Radius ? 1 : 0;
            outputVertices[index] = new VertexData
            {
                Position = position - Up * modifier * Power,
                Normal = VertexData[index].Normal,
                Uv = VertexData[index].Uv
            };

            VertexData[index] = outputVertices[index];
        }
    }
}