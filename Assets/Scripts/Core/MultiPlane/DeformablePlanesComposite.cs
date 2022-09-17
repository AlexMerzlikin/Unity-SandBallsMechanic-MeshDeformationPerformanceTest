using UnityEngine;

namespace Core.MultiPlane
{
    public class DeformablePlanesComposite : DeformablePlane
    {
        [SerializeField] private DeformablePlane[] _deformablePlanes;
        [SerializeField] private float _distanceThreshold = 1f;
        private Bounds[] _bounds;
        private Vector3 _positionToDeform;

        private void Awake()
        {
            _bounds = new Bounds[_deformablePlanes.Length];
            for (var i = 0; i < _deformablePlanes.Length; i++)
            {
                _bounds[i] = _deformablePlanes[i].GetComponent<Collider>().bounds;
            }
        }

        public override void Deform(Vector3 positionToDeform)
        {
            _positionToDeform = positionToDeform;
            for (var i = 0; i < _deformablePlanes.Length; i++)
            {
                var deformablePlane = _deformablePlanes[i];
                var bounds = _bounds[i];
                var sqrDistance = bounds.SqrDistance(_positionToDeform);
                if (sqrDistance < _distanceThreshold)
                {
                    deformablePlane.Deform(positionToDeform);
                }
            }
        }
    }
}