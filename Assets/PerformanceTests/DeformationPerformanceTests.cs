using System.Collections;
using Core;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class DeformationPerformanceTests
    {
        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_MeshData_PerformanceTest()
        {
            yield return DeformPlane("JobifiedMeshDataApiSample");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_ComputeShader_PerformanceTest()
        {
            yield return DeformPlane("ComputeShaderWithAsyncReadbackSample");
        }
    
        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_NaiveJob_PerformanceTest()
        {
            yield return DeformPlane("JobifiedNaiveSample_Lerp");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_Naive_PerformanceTest()
        {
            yield return DeformPlane("NaiveSample");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_ProBuilder_PerformanceTest()
        {
            yield return DeformPlane("ProBuilderSample");
        }

        private static IEnumerator DeformPlane(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return null;

            var plane = Object.FindObjectOfType<DeformablePlane>();
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