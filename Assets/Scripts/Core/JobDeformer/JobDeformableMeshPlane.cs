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
        private ArrayMeshDeformerJob _job;
        private JobHandle _handle;
        private NativeList<Vector3> _deformationPointsNativeArray;

        public override void Deform(Vector3 point)
        {
            _deformationPointsNativeArray.Add(transform.InverseTransformPoint(point));
        }

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = gameObject.GetComponent<MeshCollider>();
            _vertices = new NativeArray<Vector3>(_mesh.vertices, Allocator.Persistent);
            _deformationPointsNativeArray = new NativeList<Vector3>(Allocator.Persistent);
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
            _deformationPointsNativeArray.Dispose();
        }

        private void ScheduleJob()
        {
            if (_scheduled || _deformationPointsNativeArray.Length == 0)
            {
                return;
            }
            
            StartEstimation();
            _scheduled = true;
            _job = new ArrayMeshDeformerJob(
                _radiusOfDeformation,
                _powerOfDeformation,
                _vertices,
                _deformationPointsNativeArray);
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
            _mesh.vertices = _vertices.ToArray();
            _collider.sharedMesh = _mesh;
            _deformationPointsNativeArray.Clear();
            _scheduled = false;
            FinishEstimation();
        }
    }
}