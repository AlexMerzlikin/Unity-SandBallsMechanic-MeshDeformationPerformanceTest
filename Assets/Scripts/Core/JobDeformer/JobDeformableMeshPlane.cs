using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    /// <summary>
    /// Jobified vertices array update. Modifies a mesh by using mesh.vertices setter
    /// Schedules a job with a list of deformation points accumulated during execution of a previous job
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class JobDeformableMeshPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<Vector3> _vertices;
        private bool _scheduled;
        private MultipleDeformationPointsMeshDeformerJob _job;
        private JobHandle _handle;
        private NativeList<Vector3> _deformationPoints;
        private NativeList<Vector3> _deformationPointsCopy;

        public override void Deform(Vector3 point)
        {
            _deformationPoints.Add(transform.InverseTransformPoint(point));
        }

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = GetComponent<MeshCollider>();
            _vertices = new NativeArray<Vector3>(_mesh.vertices, Allocator.Persistent);
            _deformationPoints = new NativeList<Vector3>(Allocator.Persistent);
            _deformationPointsCopy = new NativeList<Vector3>(Allocator.Persistent);
        }

        private void Update()
        {
            ScheduleJob();
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

        private void ScheduleJob()
        {
            if (_scheduled || _deformationPoints.Length == 0)
            {
                return;
            }

            _scheduled = true;
            _deformationPointsCopy.CopyFrom(_deformationPoints);
            _deformationPoints.Clear();
            _job = new MultipleDeformationPointsMeshDeformerJob(
                _radiusOfDeformation,
                _powerOfDeformation,
                _vertices,
                _deformationPointsCopy);
            _handle = _job.Schedule(_vertices.Length, 64);
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
            _deformationPointsCopy.Clear();
            _scheduled = false;
        }
    }
}