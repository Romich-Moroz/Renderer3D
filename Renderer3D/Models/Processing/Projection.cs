using Renderer3D.Models.Data;
using Renderer3D.Models.Scene;
using System.Numerics;

namespace Renderer3D.Models.Processing
{
    public static class Projection
    {
        private static Vector4 PerspectiveDivide(Vector4 vector)
        {
            return new Vector4 { X = vector.X / vector.W, Y = vector.Y / vector.W, Z = vector.Z / vector.W, W = 1 };
        }

        /// <summary>
        /// Creates viewport matrix for transformation
        /// </summary>
        /// <param name="width">Width of the screen</param>
        /// <param name="height">Height of the screen</param>
        /// <param name="xMin">Min screen coordinate of x axis</param>
        /// <param name="yMin">Min screen coordinate of y axis</param>
        /// <returns>Viewport patrix for translation</returns>
        public static Matrix4x4 CreateViewportMatrix(float width, float height, int xMin = 0, int yMin = 0)
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

        public static TransformMatrixes GetTransformMatrixes(ModelProperties modelProperties, CameraProperties cameraProperties, BitmapProperties bitmapProperties)
        {
            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(modelProperties.Scale) *
                                    Matrix4x4.CreateRotationX(modelProperties.Rotation.X) *
                                    Matrix4x4.CreateRotationY(modelProperties.Rotation.Y) *
                                    Matrix4x4.CreateRotationZ(modelProperties.Rotation.Z) *
                                    Matrix4x4.CreateTranslation(modelProperties.Offset);
            Matrix4x4 viewMatrix = worldMatrix * Matrix4x4.CreateLookAt(cameraProperties.CameraPosition, cameraProperties.CameraTarget, cameraProperties.CameraUpVector);
            Matrix4x4 perspectiveMatrix = viewMatrix * Matrix4x4.CreatePerspectiveFieldOfView(cameraProperties.Fov, bitmapProperties.AspectRatio, 1, 100);
            Matrix4x4 viewportMatrix = CreateViewportMatrix(bitmapProperties.Width, bitmapProperties.Height);

            return new TransformMatrixes(worldMatrix, viewMatrix, perspectiveMatrix, viewportMatrix);
        }

        public static Vector3 ProjectVertex(Matrix4x4 perspectiveMatrix, Matrix4x4 viewportMatrix, Vector4 vertex)
        {
            Vector4 result = Vector4.Transform(PerspectiveDivide(Vector4.Transform(vertex, perspectiveMatrix)), viewportMatrix);
            return new Vector3(result.X, result.Y, result.Z);
        }

        public static Vector3 ProjectNormal(Matrix4x4 viewMatrix, Matrix4x4 viewportMatrix, Vector3 normal)
        {
            Vector3 result = Vector3.Transform(Vector3.Transform(normal, viewMatrix), viewportMatrix);
            return new Vector3(result.X, result.Y, result.Z);
        }
    }
}
