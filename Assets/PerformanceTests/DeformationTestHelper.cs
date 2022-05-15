using System;
using System.Collections;
using Core;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PerformanceTests
{
    public static class DeformationTestHelper
    {
        public static IEnumerator DeformPlane(string sceneName, Action<DeformablePlane> planeSetup = null)
        {
            yield return LoadScene(sceneName);
            var plane = Object.FindObjectOfType<DeformablePlane>();
            planeSetup?.Invoke(plane);
            yield return DeformGradually(plane);
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null;
        }

        private static IEnumerator DeformGradually(DeformablePlane plane)
        {
            var bounds = plane.GetComponent<Collider>().bounds;
            var min = bounds.min;
            var max = bounds.max;
            var pos = min;
            using (Measure.Frames().Scope())
            {
                while (pos.x < max.x)
                {
                    while (pos.y < max.y)
                    {
                        plane.Deform(pos);
                        yield return null;
                        pos = new Vector3(pos.x, pos.y + 1f, pos.z);
                    }

                    pos = new Vector3(pos.x + 1f, min.y, pos.z);
                }
            }
        }
    }
}