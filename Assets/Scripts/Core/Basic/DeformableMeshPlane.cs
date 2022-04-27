using UnityEngine;

namespace Core.Basic
{
    /// <summary>
    /// Basic deformable mesh implementation with direct modification of vertices array and setting it back into the mesh in the main thread
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class DeformableMeshPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private Vector3[] _vertices;

        private void Awake()
        {
            var meshFilter = GetComponent<MeshFilter>();
            _collider = GetComponent<MeshCollider>();
            _mesh = meshFilter.mesh;
            _vertices = _mesh.vertices;
        }

        public override void Deform(Vector3 positionToDeform)
        {
            positionToDeform = transform.InverseTransformPoint(positionToDeform);
            var somethingDeformed = false;

            for (var i = 0; i < _vertices.Length; i++)
            {
                var dist = (_vertices[i] - positionToDeform).sqrMagnitude;
                if (dist < _radiusOfDeformation)
                {
                    _vertices[i] -= Vector3.up * _powerOfDeformation;
                    somethingDeformed = true;
                }
            }

            if (!somethingDeformed)
            {
                return;
            }

            _mesh.vertices = _vertices;
            _collider.sharedMesh = _mesh;
        }
    }
}