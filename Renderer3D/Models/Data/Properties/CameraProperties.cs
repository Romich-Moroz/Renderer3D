using System;
using System.Collections.Generic;
using System.Numerics;

namespace Renderer3D.Models.Data.Properties
{
    public class CameraProperties
    {
        /// <summary>
        /// Position of the camera itself
        /// </summary>
        public Vector3 CameraPosition { get; set; } = Vector3.One;

        /// <summary>
        /// Position where the camera actually looks
        /// </summary>
        public Vector3 CameraTarget { get; set; } = Vector3.Zero;

        /// <summary>
        /// Vertical vector from camera stand point
        /// </summary>
        public Vector3 CameraUpVector { get; set; } = Vector3.UnitY;

        /// <summary>
        /// Camera field of view
        /// </summary>
        public float Fov { get; set; } = (float)Math.PI / 4;

        private void UpdateCameraUpVector()
        {
            Vector3 lookVector = Vector3.Normalize(CameraPosition - CameraTarget);
            Vector3 rightVector = Vector3.Cross(lookVector, Vector3.UnitY);
            CameraUpVector = Vector3.Cross(rightVector, lookVector);
        }

        public void RotateCamera(Vector3 axis, float angle)
        {
            CameraPosition = Vector3.Transform(CameraPosition - CameraTarget, Matrix4x4.CreateFromAxisAngle(axis, angle)) + CameraTarget;
            UpdateCameraUpVector();
        }

        public void CenterCamera(List<Vector4> vertices)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                x += vertices[i].X;
                y += vertices[i].Y;
                z += vertices[i].Z;
            }
            CameraTarget = new Vector3 { X = (float)x / vertices.Count, Y = (float)y / vertices.Count, Z = (float)z / vertices.Count };
            CameraPosition = new Vector3(CameraTarget.X + 50, CameraTarget.Y, 0);
            UpdateCameraUpVector();
        }

        /// <summary>
        /// Move camera from or to model
        /// </summary>
        /// <param name="distance">Distance to move camera from object</param>
        public void OffsetCamera(Vector3 distance)
        {
            Vector3 look = CameraPosition - CameraTarget;
            float max = Math.Abs(look.X);
            if (max < Math.Abs(look.Y))
            {
                max = Math.Abs(look.Y);
            }

            if (max < Math.Abs(look.Z))
            {
                max = Math.Abs(look.Z);
            }

            CameraPosition += look / max * distance;
        }

        /// <summary>
        /// Rotate camera around X axis
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        public void RotateCameraX(float angle)
        {
            RotateCamera(Vector3.UnitX, angle);
        }

        /// <summary>
        /// Rotate camera around Y axis
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        public void RotateCameraY(float angle)
        {
            RotateCamera(Vector3.UnitY, angle);
        }

        /// <summary>
        /// Rotate camera around Y axis
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        public void RotateCameraZ(float angle)
        {
            RotateCamera(Vector3.UnitZ, angle);
        }

        public void Reset()
        {
            CameraPosition = Vector3.One;
            CameraTarget = Vector3.Zero;
            CameraUpVector = Vector3.UnitY;
        }
    }
}
