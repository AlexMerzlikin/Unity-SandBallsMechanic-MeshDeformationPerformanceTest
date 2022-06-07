using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core.JobDeformer
{
    /// <summary>
    /// Jobified vertices array update. Modifies a mesh by using mesh.vertices setter.
    /// Schedules a job with a list of deformation points accumulated during execution of a previous job
    /// Additionally lerps points between each in the list to compensate very fast mouse movement skipping large areas between frames 
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class LerpJobDeformableMeshPlane : DeformablePlane
    {
        [SerializeField] private int _lerpedDeformationPointsArraySize = 20;
        [SerializeField] private int _bufferingFrames = 2;
        [SerializeField] private int _innerloopBatchCount = 64;

        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<Vector3> _vertices;
        private bool _scheduled;
        private MultipleDeformationPointsMeshDeformerJob _job;
        private JobHandle _handle;
        private NativeList<Vector3> _deformationPointsBuffer;
        private NativeList<Vector3> _deformationPoints;
        private Vector3[] _lerpedDeformationPoints;
        private int _previousInputFrame;
        private Vector3 _previousInput;

        public int InnerloopBatchCount
        {
            get => _innerloopBatchCount;
            set => _innerloopBatchCount = value;
        }

        public override void Deform(Vector3 point)
        {
            var newPoint = transform.InverseTransformPoint(point);
            if (_previousInputFrame + _bufferingFrames <= Time.frameCount)
            {
                _deformationPointsBuffer.Add(newPoint);
                return;
            }

            if (_previousInput == newPoint)
            {
                return;
            }

            var distance = Vector3.Distance(_previousInput, newPoint);
            if (distance < _radiusOfDeformation)
            {
                return;
            }

            var pointsCount = PointsUtility.GetAllPointsBetween(
                _lerpedDeformationPoints,
                _previousInput,
                newPoint,
                _radiusOfDeformation * 0.75f);
            if (pointsCount == 0)
            {
                _deformationPointsBuffer.Add(newPoint);
                return;
            }

            for (var i = 0; i < pointsCount; i++)
            {
                _deformationPointsBuffer.Add(_lerpedDeformationPoints[i]);
            }
        }

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = GetComponent<MeshCollider>();
            _vertices = new NativeArray<Vector3>(_mesh.vertices, Allocator.Persistent);
            _deformationPointsBuffer = new NativeList<Vector3>(Allocator.Persistent);
            _deformationPoints = new NativeList<Vector3>(Allocator.Persistent);
            _lerpedDeformationPoints = new Vector3[_lerpedDeformationPointsArraySize];
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
            _deformationPointsBuffer.Dispose();
            _deformationPoints.Dispose();
        }

        private void ScheduleJob()
        {
            if (_scheduled || _deformationPointsBuffer.Length == 0)
            {
                return;
            }

            _scheduled = true;
            _deformationPoints.AddRange(_deformationPointsBuffer);
            _job = new MultipleDeformationPointsMeshDeformerJob(
                _radiusOfDeformation,
                _powerOfDeformation,
                _vertices,
                _deformationPoints);

            _previousInputFrame = Time.frameCount;
            _previousInput = _deformationPointsBuffer[^1];
            _deformationPointsBuffer.Clear();
            _handle = _job.Schedule(_vertices.Length, InnerloopBatchCount);
        }

        private void CompleteJob()
        {
            if (!_scheduled)
            {
                return;
            }

            _handle.Complete();
            _deformationPoints.Clear();
            _job.Vertices.CopyTo(_vertices);
            _mesh.SetVertices(_vertices);
            _collider.sharedMesh = _mesh;
            _scheduled = false;
        }
    }
}