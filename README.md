# Sand Balls Mechanic: Mesh Deformation Performance Test

![DeformationSample](DeformationSample.gif)

The repository contains implementation of Sand Balls mechanic using different approaches to deform a mesh.
The goal is to play around with [MeshData API](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/Mesh.MeshData.html) and compute shader with [AsyncGPUReadback](https://docs.unity3d.com/ScriptReference/Rendering.AsyncGPUReadback.html).
And then compare its performance to other approaches.

The following samples were implemented:
- [Naive implementation](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/Basic/DeformableMeshPlane.cs)
- [Naive implementation using ProBuilder Mesh](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/Basic/DeformableProBuilderMeshPlane.cs)
- [Jobified naive implementation](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/JobDeformer/JobDeformableMeshPlane.cs)
- [Compute shader with AsyncGPUReadback](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/ComputeShaderDeformer/ComputeShaderAsyncGpuReadbackDeformablePlane.cs)
- [MeshData modification on the main thread for easier debugging](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/MeshData/MeshDataOnCPU/DeformableMeshDataSingleThread.cs)
- [MeshData modification using jobs](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/blob/master/Assets/Scripts/Core/MeshData/JobDeformableMeshDataPlane.cs)

Each sample can be found inside the [Scenes](https://github.com/AlexMerzlikin/SandBallsMechanic-MeshDeformationPerformanceTest/tree/master/Assets/Scenes) folder

## Perforormance Test Results
![PerformanceTestExample](PerformanceTest_example.gif)
