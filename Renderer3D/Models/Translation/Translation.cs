using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Renderer3D.Models.Translation
{
    /// <summary>
    /// Matrix translation library
    /// </summary>
    public static class Translation
    {
        private static Matrix4x4 CreateMovingMatrix(Vector3 offset)
        {
            return new Matrix4x4 
            {
                M11 = 1, M12 = 0, M13 = 0, M14 = offset.X,
                M21 = 0, M22 = 1, M23 = 0, M24 = offset.Y,
                M31 = 0, M32 = 0, M33 = 1, M34 = offset.Z,
                M41 = 0, M42 = 0, M43 = 0, M44 = 1
            };
        }

        private static Matrix4x4 CreateScaleMatrix(Vector3 offset)
        {
            return new Matrix4x4 
            {
                M11 = offset.X, M12 = 0,        M13 = 0,        M14 = 0,
                M21 = 0,        M22 = offset.Y, M23 = 0,        M24 = 0,
                M31 = 0,        M32 = 0,        M33 = offset.Z, M34 = 0,
                M41 = 0,        M42 = 0,        M43 = 0,        M44 = 1
            };
        }
        private static Matrix4x4 CreateXRotationMatrix(float angle)
        {
            return new Matrix4x4 
            {
                M11 = 1, M12 = 0,                       M13 = 0,                        M14 = 0,
                M21 = 0, M22 = (float)Math.Cos(angle),  M23 = -(float)Math.Sin(angle),  M24 = 0,
                M31 = 0, M32 = (float)Math.Sin(angle),  M33 = (float)Math.Cos(angle),   M34 = 0,
                M41 = 0, M42 = 0,                       M43 = 0,                        M44 = 1
            };
        }

        private static Matrix4x4 CreateYRotationMatrix(float angle)
        {
            return new Matrix4x4 
            {
                M11 = (float)Math.Cos(angle),   M12 = 0, M13 = (float)Math.Sin(angle),  M14 = 0,
                M21 = 0,                        M22 = 1, M23 = 0,                       M24 = 0,
                M31 = -(float)Math.Sin(angle),  M32 = 0, M33 = (float)Math.Cos(angle),  M34 = 0,
                M41 = 0,                        M42 = 0, M43 = 0,                       M44 = 1
            };
        }

        private static Matrix4x4 CreateZRotationMatrix(float angle)
        {
            return new Matrix4x4 
            {
                M11 = (float)Math.Cos(angle),   M12 = -(float)Math.Sin(angle), M13 = 0, M14 = 0,
                M21 = (float)Math.Sin(angle),   M22 = (float)Math.Cos(angle),  M23 = 0, M24 = 0,
                M31 = 0,                        M32 = 0,                       M33 = 1, M34 = 0,
                M41 = 0,                        M42 = 0,                       M43 = 0, M44 = 1
            };
        }

        /// <summary>
        /// Moves vertex in world coordinates
        /// </summary>
        /// <param name="objectVertex">Vertex to move</param>
        /// <param name="locationOffset">Offset from original object position</param>
        /// <returns>Offseted vertex</returns>
        public static Vector4 WorldMove(Vector4 objectVertex, Vector3 locationOffset) => Vector4.Transform(objectVertex, CreateMovingMatrix(locationOffset));

        /// <summary>
        /// Scales vertex to new coordinates
        /// </summary>
        /// <param name="objectVertex">Vertex to scale</param>
        /// <param name="scale">Scalling vector for each axis</param>
        /// <returns>Scalled vertex</returns>
        public static Vector4 WorldScale(Vector4 objectVertex, Vector3 scale) => Vector4.Transform(objectVertex, CreateScaleMatrix(scale));

        /// <summary>
        /// Rotates vertex around X axis
        /// </summary>
        /// <param name="objectVertex">Vertex to rotate</param>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>Rotated vertex</returns>
        public static Vector4 WorldRotateX(Vector4 objectVertex, float angle) => Vector4.Transform(objectVertex, CreateXRotationMatrix(angle));

        /// <summary>
        /// Rotates vertex around Y axis
        /// </summary>
        /// <param name="objectVertex">Vertex to rotate</param>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>Rotated vertex</returns>
        public static Vector4 WorldRotateY(Vector4 objectVertex, float angle) => Vector4.Transform(objectVertex, CreateYRotationMatrix(angle));

        /// <summary>
        /// Rotates vertex around Z axis
        /// </summary>
        /// <param name="objectVertex">Vertex to rotate</param>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>Rotated vertex</returns>
        public static Vector4 WorldRotateZ(Vector4 objectVertex, float angle) => Vector4.Transform(objectVertex, CreateZRotationMatrix(angle));

        /// <summary>
        /// Creates view matrix for further transformation
        /// </summary>
        /// <param name="cameraPosition">Original camera position</param>
        /// <param name="targetPosition">Location where the camera actually looks</param>
        /// <param name="upVector">Vector pointing straight up from camera point of view</param>
        /// <returns>View matrix for translation</returns>
        public static Matrix4x4 CreateViewMatrix(Vector3 cameraPosition, Vector3 targetPosition, Vector3 upVector)
        {
            var zAxis = Vector3.Normalize(Vector3.Subtract(cameraPosition, targetPosition));
            var xAxis = Vector3.Normalize(Vector3.Cross(upVector, zAxis));
            var yAxis = upVector;

            var dotX = Vector3.Dot(xAxis, cameraPosition);
            var dotY = Vector3.Dot(yAxis, cameraPosition);
            var dotZ = Vector3.Dot(zAxis, cameraPosition);

            return new Matrix4x4
            {
                M11 = xAxis.X, M12 = xAxis.Y, M13 = xAxis.Z, M14 = -dotX,
                M21 = yAxis.X, M22 = yAxis.Y, M23 = yAxis.Z, M24 = -dotY,
                M31 = zAxis.X, M32 = zAxis.Y, M33 = zAxis.Z, M34 = -dotZ,
                M41 = 0,       M42 = 0,       M43 = 0,       M44 = 1
            };
        }
        /// <summary>
        /// Creates projection matrix for transformation
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio of the screen, 16/9 for 1920:1080 etc.</param>
        /// <param name="fov">Camera field of view (PI/4 = 90 degrees)</param>
        /// <param name="zFar">Distance from furthest plane to the camera</param>
        /// <param name="zNear">Distance from the closest plane to the camera</param>
        /// <returns>Projection matrix for translation</returns>
        public static Matrix4x4 CreateProjectionMatrix(float aspectRatio, float fov, float zFar, float zNear)
        {
            return new Matrix4x4
            {
                M11 = (float)(1/(aspectRatio*Math.Tan(fov/2))), M12 = 0,                            M13 = 0,                 M14 = 0,
                M21 = 0,                                        M22 = (float)(1/(Math.Tan(fov/2))), M23 = 0,                 M24 = 0,
                M31 = 0,                                        M32 = 0,                            M33 = zFar/(zNear-zFar), M34 = (zNear*zFar)/(zNear-zFar),
                M41 = 0,                                        M42 = 0,                            M43 = -1,                M44 = 0
            };
        }

        /// <summary>
        /// Creates viewport matrix for transformation
        /// </summary>
        /// <param name="width">Width of the screen</param>
        /// <param name="height">Height of the screen</param>
        /// <param name="xMin">Min screen coordinate of x axis</param>
        /// <param name="yMin">Min screen coordinate of y axis</param>
        /// <returns></returns>
        public static Matrix4x4 CreateViewportMatrix(float width, float height, int xMin = 0, int yMin = 0)
        {
            return new Matrix4x4
            {
                M11 = width/2, M12 = 0,         M13 = 0, M14 = xMin + width/2,
                M21 = 0,       M22 = -height/2, M23 = 0, M24 = yMin + height/2,
                M31 = 0,       M32 = 0,         M33 = 1, M34 = 0,
                M41 = 0,       M42 = 0,         M43 = 0, M44 = 1
            };
        }

        /// <summary>
        /// Translate vector using specified matrix
        /// </summary>
        /// <param name="vector">Vector to translate</param>
        /// <param name="matrix">Matrix used for translation</param>
        /// <returns></returns>
        public static Vector4 Translate(Vector4 vector, Matrix4x4 matrix) => Vector4.Transform(vector, matrix);
    }
}
