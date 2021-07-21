using System.Runtime.InteropServices;
using UnityEngine;

namespace Core.JobDeformer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;
    }
}