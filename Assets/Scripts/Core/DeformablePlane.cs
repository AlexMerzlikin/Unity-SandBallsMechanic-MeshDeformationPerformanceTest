using UnityEngine;

namespace Core
{
    public abstract class DeformablePlane : MonoBehaviour
    {
        public abstract void Deform(Vector3 positionToDeform);
    }
}