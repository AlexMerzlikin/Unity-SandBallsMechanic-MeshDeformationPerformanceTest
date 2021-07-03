using InputProvider;
using Test;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Camera))]
    public class Deformator : MonoBehaviour
    {
        [SerializeField] private float _planeDistance;
        [SerializeField] private DeformablePlane _deformablePlane;

        private Camera _camera;
        private readonly IInputProvider _inputProvider = new InstantTestInputProvider();

        private void Awake()
        {
            _camera = transform.GetComponent<Camera>();
            _inputProvider.InputReceived += OnInputReceived;
        }

        private void OnInputReceived(Vector3 position)
        {
            var ray = _camera.ScreenPointToRay(position);
            DeformMesh(ray);
        }

        private void FixedUpdate()
        {
            _inputProvider.Tick();
        }

        private void DeformMesh(Ray ray)
        {
            if (Physics.Raycast(ray, out var hit))
            {
                if ((_deformablePlane.transform.position - hit.point).sqrMagnitude < _planeDistance)
                {
                    _deformablePlane.Deform(hit.point);
                }
            }
        }
    }
}