using UnityEngine;

namespace Core.JobDeformer
{
    public static class PointsUtility
    {
        public static int GetAllPointsBetween(Vector3[] results, Vector3 start, Vector3 end, float radius)
        {
            var distance = Vector3.Distance(start, end);
            var pointsAmount = Mathf.FloorToInt(distance / radius);
            var distanceBetweenFillPoints = radius / distance;
            if (results.Length < pointsAmount)
            {
                Debug.LogError($"{nameof(PointsUtility)}: results size is too small to fit all points. Need: {pointsAmount}");
                return 0;
            }

            var lerpParameter = 0f;
            for (var i = 0; i < pointsAmount; i++)
            {
                lerpParameter += distanceBetweenFillPoints;
                results[i] = Vector3.Lerp(start, end, lerpParameter);
            }
            return pointsAmount;
        }
    }
}