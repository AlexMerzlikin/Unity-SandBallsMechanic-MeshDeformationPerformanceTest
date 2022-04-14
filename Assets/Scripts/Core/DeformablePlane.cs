using UnityEngine;

namespace Core
{
    public abstract class DeformablePlane : MonoBehaviour
    {
        [SerializeField] protected float _radiusOfDeformation = 0.2f;
        [SerializeField] protected float _powerOfDeformation = 1f;

        public abstract void Deform(Vector3 positionToDeform);
    }
}