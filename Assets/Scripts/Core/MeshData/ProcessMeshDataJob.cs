using Core.JobDeformer;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Core.MeshData
{
    [BurstCompile]
    public struct ProcessMeshDataJob : IJobParallelFor
    {
        private static readonly float3 Up = new float3(0, 1, 0);

        [ReadOnly] public Mesh.MeshDataArray MeshData;
        public Mesh.MeshData OutputMesh;
        [ReadOnly] public float Radius;
        [ReadOnly] public float Power;
        [ReadOnly] public Vector3 Point;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> TempVertices;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> TempNormals;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> TempUvs;


        public void Execute(int index)
        {
            var outputVerts = OutputMesh.GetVertexData<VertexData>();
            var vertex = TempVertices[index];
            var distance = SqrMagnitude(vertex - (float3) Point);
            var modifier = distance < Radius ? 1 : 0;
            outputVerts[index] = new VertexData
            {
                Position = vertex - Up * modifier * Power,
                Normal = TempNormals[index],
                Uv = TempUvs[index]
            };

            TempVertices[index] = outputVerts[index].Position;
        }

        private static float SqrMagnitude(float3 vector) =>
            (float) (vector.x * (double) vector.x + vector.y * (double) vector.y + vector.z * (double) vector.z);
    }
}