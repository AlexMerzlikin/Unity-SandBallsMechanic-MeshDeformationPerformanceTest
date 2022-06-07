using System.Collections;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class DeformationPlaneResolutionPerformanceTests
    {
        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_LowPolyPlane_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("NaiveSample_LowPoly");
        }

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_HighPolyPlane_PerformanceTest()
        {
            yield return DeformationTestHelper.DeformPlane("NaiveSample");
        }
    }
}