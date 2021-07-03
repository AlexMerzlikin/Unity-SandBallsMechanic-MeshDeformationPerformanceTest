using System;
using UnityEngine;

namespace InputProvider
{
    public interface IInputProvider
    {
        event Action<Vector3> InputReceived;
        void Tick();
    }
}