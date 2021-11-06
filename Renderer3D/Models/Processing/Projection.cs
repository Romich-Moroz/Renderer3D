using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;

namespace Renderer3D.Models.Processing
{
    public static class Projection
    {
        private static readonly ParallelOptions ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        public static TransformMatrixes LastGetTransformMatrixesResult { get; private set; }

        private static Matrix4x4 CreateViewportMatrix(float width, float height, int xMin = 0, int yMin = 0)
        {
            return new Matrix4x4
            {
                M11 = width / 2,
                M12 = 0,
                M13 = 0,
                M14 = 0,
                M21 = 0,
                M22 = -height / 2,
                M23 = 0,
                M24 = 0,
                M31 = 0,
                M32 = 0,
                M33 = 1,
                M34 = 0,
                M41 = xMin + width / 2,
                M42 = yMin + height / 2,
                M43 = 0,
                M44 = 1
            };
        }

        public static TransformMatrixes GetTransformMatrixes(ModelProperties modelProperties, CameraProperties cameraProperties, ScreenProperties screenProperties)
        {
            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(modelProperties.Scale) *
                                    Matrix4x4.CreateRotationX(modelProperties.Rotation.X) *
                                    Matrix4x4.CreateRotationY(modelProperties.Rotation.Y) *
                                    Matrix4x4.CreateRotationZ(modelProperties.Rotation.Z) *
                                    Matrix4x4.CreateTranslation(modelProperties.Offset);
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(cameraProperties.CameraPosition, cameraProperties.CameraTarget, cameraProperties.CameraUpVector);
            Matrix4x4 perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(cameraProperties.Fov, screenProperties.AspectRatio, 1, 100);
            Matrix4x4 viewportMatrix = CreateViewportMatrix(screenProperties.Width, screenProperties.Height);

            LastGetTransformMatrixesResult = new TransformMatrixes(worldMatrix, viewMatrix, perspectiveMatrix, viewportMatrix);
            return LastGetTransformMatrixesResult;
        }

        public static void ProjectMesh(TransformMatrixes transformMatrixes, Mesh mesh, ShadingMode renderMode)
        {
            //Project to screen
            _ = Parallel.ForEach(Partitioner.Create(0, mesh.OriginalMeshProperties.Vertices.Count), ParallelOptions, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    Matrix4x4 projMatrix = transformMatrixes.WorldMatrix * transformMatrixes.ViewMatrix * transformMatrixes.PerspectiveMatrix;
                    if (renderMode == ShadingMode.None)
                    {
                        projMatrix *= transformMatrixes.ViewportMatrix;
                    }
                    Vector4 result = Vector4.Transform(mesh.OriginalMeshProperties.Vertices[i], projMatrix);
                    if (renderMode == ShadingMode.None)
                    {
                        result /= result.W;
                    }
                    mesh.TransformedMeshProperties.Vertices[i] = result;
                }
            });

            //Project normals
            _ = Parallel.ForEach(Partitioner.Create(0, mesh.OriginalMeshProperties.Normals.Count), ParallelOptions, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    mesh.TransformedMeshProperties.Normals[i] = Vector3.TransformNormal(mesh.OriginalMeshProperties.Normals[i], transformMatrixes.WorldMatrix);
                }
            });
        }
    }
}
