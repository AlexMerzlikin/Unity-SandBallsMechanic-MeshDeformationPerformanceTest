using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.ComputeShaderDeformer
{
    public class ComputeShaderAsyncGpuReadbackDeformablePlane : DeformablePlane
    {
        public ComputeShader computeShader;
        public MeshFilter mf;
        public MeshCollider mc;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct VertexData
        {
            public Vector3 pos;
            public Vector3 nor;
            public Vector2 uv;
        }

        private Mesh mesh;
        private ComputeBuffer cBuffer;
        private int _kernel;
        private int dispatchCount = 0;
        private NativeArray<VertexData> vertData;
        private AsyncGPUReadbackRequest request;
        private bool _isDispatched;

        public override void Deform(Vector3 positionToDeform)
        {
            var point = transform.InverseTransformPoint(positionToDeform);
            computeShader.SetFloats("_DeformPosition", point.x, point.y, point.z);
            computeShader.Dispatch(_kernel, dispatchCount, 1, 1);

            if (_isDispatched)
            {
                return;
            }
            
            _isDispatched = true;
            request = AsyncGPUReadback.Request(cBuffer);
        }

        private void Awake()
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                gameObject.SetActive(false);
                return;
            }

            var meshFilter = GetComponent<MeshFilter>();
            mc = GetComponent<MeshCollider>();
            mesh = meshFilter.mesh;

            _kernel = computeShader.FindKernel("CSMain");
            uint threadX = 0;
            uint threadY = 0;
            uint threadZ = 0;
            computeShader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
            dispatchCount = Mathf.CeilToInt(mesh.vertexCount / threadX + 1);

            vertData = new NativeArray<VertexData>(mesh.vertexCount, Allocator.Temp);
            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                VertexData v = new VertexData();
                v.pos = mesh.vertices[i];
                v.nor = mesh.normals[i];
                v.uv = mesh.uv[i];
                vertData[i] = v;
            }

            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            mesh.SetVertexBufferParams(mesh.vertexCount, layout);

            cBuffer = new ComputeBuffer(mesh.vertexCount, 8 * 4); // 3*4bytes = sizeof(Vector3)
            if (vertData.IsCreated)
            {
                cBuffer.SetData(vertData);
            }

            computeShader.SetBuffer(_kernel, "vertexBuffer", cBuffer);
            computeShader.SetFloat("_Force", _powerOfDeformation);
            computeShader.SetFloat("_Radius", _radiusOfDeformation);
        }

        private void Update()
        {
            //run the compute shader, the position of particles will be updated in GPU
            if (_isDispatched && request.done && !request.hasError)
            {
                _isDispatched = false;
                //Readback and show result on texture
                vertData = request.GetData<VertexData>();

                //Update mesh
                mesh.MarkDynamic();
                mesh.SetVertexBufferData(vertData, 0, 0, vertData.Length);
                // mesh.RecalculateNormals();

                //Update to collider
                mc.sharedMesh = mesh;

                //Request AsyncReadback again
                request = AsyncGPUReadback.Request(cBuffer);
            }
        }

        private void CleanUp()
        {
            cBuffer?.Release();
        }

        void OnDisable()
        {
            CleanUp();
        }

        void OnDestroy()
        {
            CleanUp();
        }
    }
}