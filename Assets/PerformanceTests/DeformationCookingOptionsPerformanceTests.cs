using System.Collections;
using Core.JobDeformer;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

namespace PerformanceTests
{
    public class DeformationCookingOptionsPerformanceTests
    {
        private static MeshColliderCookingOptions[] cookingOptions =
        {
            0,
            (MeshColliderCookingOptions) 2,
            (MeshColliderCookingOptions) 4,
            (MeshColliderCookingOptions) 6,
            (MeshColliderCookingOptions) 8,
            (MeshColliderCookingOptions) 10,
            (MeshColliderCookingOptions) 12,
            (MeshColliderCookingOptions) 14,
            (MeshColliderCookingOptions) 16,
            (MeshColliderCookingOptions) 18,
            (MeshColliderCookingOptions) 20,
            (MeshColliderCookingOptions) 24,
            (MeshColliderCookingOptions) 26,
            (MeshColliderCookingOptions) 28,
            (MeshColliderCookingOptions) 30
        };

        [UnityTest, Performance]
        public IEnumerator DeformableMeshPlane_NaiveJob_PerformanceTest(
            [ValueSource(nameof(cookingOptions))] MeshColliderCookingOptions cookingOptionsSettings)
        {
            yield return DeformationTestHelper.DeformPlane("JobifiedNaiveSample_Lerp",
                plane =>
                {
                    ((LerpJobDeformableMeshPlane) plane).InnerloopBatchCount = 32;
                    var collider = plane.GetComponent<MeshCollider>();
                    collider.cookingOptions = cookingOptionsSettings;
                });
        }
    }
}