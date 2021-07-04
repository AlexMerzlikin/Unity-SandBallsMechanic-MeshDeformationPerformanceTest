using System;
using UnityEngine;

namespace InputProvider
{
    public class MouseInputProvider : IInputProvider
    {
        public event Action<Vector3> InputReceived;

        public void Tick()
        {
            if (Input.GetMouseButton(0))
            {
                OnInputReceived(Input.mousePosition);
            }
        }
    
        private void OnInputReceived(Vector3 mousePosition)
        {
            InputReceived?.Invoke(mousePosition);
        }
    }
}