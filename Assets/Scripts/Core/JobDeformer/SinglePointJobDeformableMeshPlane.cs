using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    /// <summary>
    /// Jobified vertices array update. Modifies a mesh by using mesh.vertices setter
    /// Schedules a job only with 1 point to deform
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class SinglePointJobDeformableMeshPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<Vector3> _vertices;
        private bool _scheduled;
        private MeshDeformerJob _job;
        private JobHandle _handle;
        private NativeList<Vector3> _deformationPoints;

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

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = GetComponent<MeshCollider>();
            _vertices = new NativeArray<Vector3>(_mesh.vertices, Allocator.Persistent);
            _deformationPoints = new NativeList<Vector3>(Allocator.Persistent);
        }

        private void LateUpdate()
        {
            CompleteJob();
        }

        private void OnDestroy()
        {
            _vertices.Dispose();
            _deformationPoints.Dispose();
        }

        private void CompleteJob()
        {
            if (!_scheduled)
            {
                return;
            }

            _handle.Complete();
            _job.Vertices.CopyTo(_vertices);
            _mesh.SetVertices(_vertices);
            _collider.sharedMesh = _mesh;
            _deformationPoints.Clear();
            _scheduled = false;
        }
    }
}