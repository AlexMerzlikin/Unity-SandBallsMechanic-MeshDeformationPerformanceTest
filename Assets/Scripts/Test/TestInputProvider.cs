using System;
using InputProvider;
using UnityEngine;

namespace Test
{
    public class TestInputProvider : IInputProvider
    {
        public event Action<Vector3> InputReceived;

        private readonly Vector3 _startPosition = new Vector3(950, 1000, 0);
        private readonly Vector3 _endPosition = new Vector3(950, -20, 0);
        private readonly int _stepsAmount = 30;
        private readonly float _stepCooldown = 0.05f;
        private readonly float _initDelay = 1f;

        private Vector3 _step;
        private int _stepsCount;
        private float _lastStepTime; 

        public TestInputProvider()
        {
            Init();
        }

        public TestInputProvider(Vector3 startPosition,
            Vector3 endPosition,
            int stepsAmount,
            float stepCooldown,
            float initDelay)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _stepsAmount = stepsAmount;
            _stepCooldown = stepCooldown;
            _initDelay = initDelay;
            Init();
        }

        private void Init()
        {
            var diff = _startPosition - _endPosition;
            _step = diff / _stepsAmount;
            _lastStepTime = _initDelay + _stepCooldown;
        }

        public void Tick()
        {
            if (_initDelay > Time.realtimeSinceStartup)
            {
                return;
            }

            if (_stepsCount >= _stepsAmount)
            {
                return;
            }
        
            if (_lastStepTime + _stepCooldown > Time.realtimeSinceStartup)
            {
                return;
            }

            InputReceived?.Invoke(_startPosition - _step * _stepsCount);
            _stepsCount++;
            _lastStepTime = Time.realtimeSinceStartup;
        }
    }
}