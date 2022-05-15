using System.Collections;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class DeformationPerformanceTests
    {
        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_MeshData_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("JobifiedMeshDataApiSample");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_ComputeShader_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("ComputeShaderWithAsyncReadbackSample_Lerped");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_NaiveJob_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("JobifiedNaiveSample_Lerp");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_Naive_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("NaiveSample");
        }
    }
}