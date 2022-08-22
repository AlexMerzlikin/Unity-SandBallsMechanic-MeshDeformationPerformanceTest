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
        [ReadOnly] public NativeArray<VertexData> VertexData;
        [ReadOnly] public Mesh.MeshDataArray MeshData;
        [ReadOnly] public float Radius;
        [ReadOnly] public float Power;
        [ReadOnly] public Vector3 Point;

        public void Execute(int index)
        {
            var outputVertices = OutputMesh.GetVertexData<VertexData>();
            var vertexData = VertexData[index];
            var position = vertexData.Position;
            var distance = (position - Point).sqrMagnitude;
            var modifier = distance < Radius ? 1 : 0;
            outputVertices[index] = new VertexData
            {
                Position = position - Up * modifier * Power,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };
        }
    }
}