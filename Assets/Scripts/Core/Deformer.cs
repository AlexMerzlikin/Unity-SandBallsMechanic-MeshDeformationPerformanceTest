using InputProvider;
using Test;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Camera))]
    public class Deformer : MonoBehaviour
    {
        [SerializeField] private float _planeDistance;
        [SerializeField] private DeformablePlane _deformablePlane;
        [SerializeField] private InputProviderType _inputProviderType;

        private Camera _camera;
        private IInputProvider _inputProvider;

        private void Awake()
        {
            _camera = transform.GetComponent<Camera>();
            _inputProvider = new InputProviderFactory().Create(_inputProviderType);
            _inputProvider.InputReceived += OnInputReceived;
        }

        private void OnInputReceived(Vector3 position)
        {
            var ray = _camera.ScreenPointToRay(position);
            DeformMesh(ray);
        }

        private void Update()
        {
            _inputProvider.Tick();
        }

        private void DeformMesh(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit))
            {
                return;
            }

            if ((_deformablePlane.transform.position - hit.point).sqrMagnitude < _planeDistance)
            {
                _deformablePlane.Deform(hit.point);
            }
        }
    }
}