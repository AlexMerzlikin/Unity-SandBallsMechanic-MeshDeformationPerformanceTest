using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class JobDeformableMeshPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<Vector3> _vertices;
        private bool _scheduled;
        private MeshDeformerJob _job;
        private JobHandle _handle;

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = gameObject.GetComponent<MeshCollider>();
            _vertices = new NativeArray<Vector3>(_mesh.vertices, Allocator.Persistent);
        }

        private void LateUpdate()
        {
            if (!_scheduled)
            {
                return;
            }

            _handle.Complete();
            _job.Vertices.CopyTo(_vertices);
            _mesh.vertices = _vertices.ToArray();
            _collider.sharedMesh = _mesh;
            _scheduled = false;
        }

        private void OnDestroy()
        {
            _vertices.Dispose();
        }

        public override void Deform(Vector3 point)
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _job = new MeshDeformerJob(
                transform.InverseTransformPoint(point),
                _radiusOfDeformation,
                _powerOfDeformation,
                _vertices);
            _handle = _job.Schedule(_vertices.Length, 64);
        }
    }
}