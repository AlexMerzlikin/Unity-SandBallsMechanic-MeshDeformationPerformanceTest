using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Core.Basic
{
    /// <summary>
    /// Basic deformable mesh implementation with direct modification of vertices array and setting it back to the mesh
    /// Uses ProBuilderMesh instead of the default Mesh type
    /// </summary>
    [RequireComponent(typeof(ProBuilderMesh))]
    public class DeformableProBuilderMeshPlane : DeformablePlane
    {
        private ProBuilderMesh _mesh;
        private Vector3[] _vertices;

        private void Awake()
        {
            _mesh = GetComponent<ProBuilderMesh>();
            _vertices = _mesh.positions.ToArray();
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

            _mesh.positions = _vertices;
            _mesh.ToMesh();
            _mesh.Refresh();
        }
    }
}