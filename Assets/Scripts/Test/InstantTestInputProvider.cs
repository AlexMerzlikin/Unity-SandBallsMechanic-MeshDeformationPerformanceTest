using System;
using InputProvider;
using UnityEngine;

namespace Test
{
    public class InstantTestInputProvider : IInputProvider
    {
        public event Action<Vector3> InputReceived;

        private readonly Vector3 _startPosition = new Vector3(950, 1000, 0);
        private readonly Vector3 _endPosition = new Vector3(950, -20, 0);
        private readonly int _stepsAmount = 30;

        private Vector3 _step;
        private int _stepsCount;
        private float _lastStepTime; 

        public InstantTestInputProvider()
        {
            Init();
        }

        public InstantTestInputProvider(Vector3 startPosition,
            Vector3 endPosition,
            int stepsAmount)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _stepsAmount = stepsAmount;
            Init();
        }

        private void Init()
        {
            var diff = _startPosition - _endPosition;
            _step = diff / _stepsAmount;
        }

        public void Tick()
        {
            if (_stepsCount >= _stepsAmount)
            {
                return;
            }

            for (int i = 0; i < _stepsAmount; i++)
            {
                InputReceived?.Invoke(_startPosition - _step * i);
            }

            _stepsCount = _stepsAmount;
        }
    }
}