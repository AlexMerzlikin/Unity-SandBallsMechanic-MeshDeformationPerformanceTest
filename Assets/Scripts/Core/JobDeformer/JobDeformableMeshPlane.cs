using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Core.JobDeformer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class JobDeformableMeshPlane : DeformablePlane
    {
        private Mesh _mesh;
        private MeshCollider _collider;
        private NativeArray<float4> _vertices;
        private bool _scheduled;
        private ArrayMeshDeformerJob _job;
        private JobHandle _handle;
        private NativeList<float4> _deformationPointsNativeArray;

        public override void Deform(Vector3 point)
        {
            var inversedPoint = transform.InverseTransformPoint(point);
            _deformationPointsNativeArray.Add(new float4(inversedPoint.x, inversedPoint.y, inversedPoint.z, 0));
        }

        private void Awake()
        {
            _mesh = gameObject.GetComponent<MeshFilter>().mesh;
            _mesh.MarkDynamic();
            _collider = gameObject.GetComponent<MeshCollider>();
            var vertices = _mesh.vertices.Select(x => new float4(x.x, x.y, x.z, 0)).ToArray();
            _vertices = new NativeArray<float4>(vertices, Allocator.Persistent);
            _deformationPointsNativeArray = new NativeList<float4>(Allocator.Persistent);
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
            _mesh.vertices = _vertices.Select(v=> new Vector3(v.x, v.y, v.z)).ToArray();
            _collider.sharedMesh = _mesh;
            _deformationPointsNativeArray.Clear();
            _scheduled = false;
        }
    }
}