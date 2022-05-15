using System.Collections;
using Core.JobDeformer;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class DeformationBatchCountPerformanceTests
    {
        private static int[] values = { 1, 16, 32, 64, 128, 256, 1024 };

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_NaiveJob_PerformanceTest([ValueSource(nameof(values))] int batchCount)
        {
            yield return DeformationTestHelper.DeformPlane("JobifiedNaiveSample_Lerp",
                plane => { ((LerpJobDeformableMeshPlane) plane).InnerloopBatchCount = batchCount; });
        }
    }
}