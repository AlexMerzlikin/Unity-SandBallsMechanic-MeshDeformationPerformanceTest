using System.Diagnostics;
using UnityEngine;

namespace Core
{
    public abstract class DeformablePlane : MonoBehaviour
    {
        [SerializeField] protected float _radiusOfDeformation = 0.2f;
        [SerializeField] protected float _powerOfDeformation = 1f;

        private float _startTime;
        private float _endTime;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        public float TotalTimeTaken { get; private set; }

        public abstract void Deform(Vector3 positionToDeform);

        protected void StartEstimation()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        protected void FinishEstimation()
        {
            _stopwatch.Stop();
            _endTime = _stopwatch.ElapsedMilliseconds;
            TotalTimeTaken +=_endTime;
        }
    }
}