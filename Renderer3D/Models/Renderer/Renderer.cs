using Renderer3D.Models.Data;
using Renderer3D.Models.Translation;
using Renderer3D.Models.WritableBitmap;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Renderer
{
    /// <summary>
    /// Renders complex models into bitmap source
    /// </summary>
    public class Renderer
    {
        #region Private Fields 

        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly WritableBitmapWriter writer = new WritableBitmapWriter();
        private readonly PixelFormat PixelFormat = PixelFormats.Bgr32;

        #endregion

        #region Bitmap Properties

        /// <summary>
        /// Width of the bitmap in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the bitmap in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Width of the row of pixels of the bitmap
        /// </summary>
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;

        #endregion

        #region Model Properties

        /// <summary>
        /// Scale of the model
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Offset of the model in world coordinates
        /// </summary>
        public Vector3 Offset { get; set; } = Vector3.Zero;

        /// <summary>
        /// Rotation of the model around X axis
        /// </summary>
        public float ModelRotationX { get; set; } = 0;

        /// <summary>
        /// Rotation of the model around Y axis
        /// </summary>
        public float ModelRotationY { get; set; } = 0;

        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightPosition { get; set; } = Vector3.Zero;

        #endregion

        #region Camera Properties

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

        #endregion

        #region Screen Properties

        /// <summary>
        /// Aspect ration of the screen aka Width / Height
        /// </summary>
        public float AspectRatio => (float)Width / Height;

        #endregion

        #region Render Properties

        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public Mesh Mesh { get; set; }

        /// <summary>
        /// Turns on/off triangle render mode
        /// </summary>
        public bool TriangleMode { get; set; } = false;

        #endregion

        #region Private Methods

        #region Camera Methods

        private void UpdateCameraUpVector()
        {
            Vector3 lookVector = Vector3.Normalize(CameraPosition - CameraTarget);
            Vector3 rightVector = Vector3.Cross(lookVector, Vector3.UnitY);
            CameraUpVector = Vector3.Cross(rightVector, lookVector);
        }

        private void RotateCamera(Vector3 axis, float angle)
        {
            CameraPosition = Vector3.Transform(CameraPosition - CameraTarget, Matrix4x4.CreateFromAxisAngle(axis, angle)) + CameraTarget;
            UpdateCameraUpVector();
        }

        private Vector3 FindGeometricAverage(Vector4[] vertices)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                x += vertices[i].X;
                y += vertices[i].Y;
                z += vertices[i].Z;
            }
            return new Vector3 { X = (float)x / vertices.Length, Y = (float)y / vertices.Length, Z = (float)z / vertices.Length };
        }

        #endregion

        #region Projection Methods

        private Vector4 PerspectiveDivide(Vector4 vector)
        {
            return new Vector4 { X = vector.X / vector.W, Y = vector.Y / vector.W, Z = vector.Z / vector.W, W = 1 };
        }

        private TransformMatrixes GetTransformMatrixes()
        {
            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(Scale) *
                              Matrix4x4.CreateRotationX(ModelRotationX) *
                              Matrix4x4.CreateRotationY(ModelRotationY) *
                              Matrix4x4.CreateTranslation(Offset);
            Matrix4x4 viewMatrix = worldMatrix * Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUpVector);
            Matrix4x4 perspectiveMatrix = viewMatrix * Matrix4x4.CreatePerspectiveFieldOfView(Fov, AspectRatio, 1, 100);
            Matrix4x4 viewportMatrix = Translator.CreateViewportMatrix(Width, Height);

            return new TransformMatrixes(worldMatrix, viewMatrix, perspectiveMatrix, viewportMatrix);
        }

        private void ProjectVertices(Matrix4x4 perspectiveMatrix, Matrix4x4 viewportMatrix)
        {
            Parallel.ForEach(Partitioner.Create(0, Mesh.OriginalVertices.Length), Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    Vector4 coordinates = Vector4.Transform
                    (
                        PerspectiveDivide(Vector4.Transform(Mesh.OriginalVertices[i], perspectiveMatrix)),
                        viewportMatrix
                    );
                    Mesh.TransformedVertices[i] = new Vector3(coordinates.X, coordinates.Y, coordinates.Z);
                }
            });
        }

        private void ProjectNormals(Matrix4x4 viewMatrix, Matrix4x4 viewportMatrix)
        {
            Parallel.ForEach(Partitioner.Create(0, Mesh.OriginalNormalVectors.Length), Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    Vector4 normal = Vector4.Transform
                    (
                        Vector4.Transform
                        (
                            Mesh.OriginalNormalVectors[i],
                            viewMatrix
                        ),
                        viewportMatrix
                    );
                    Mesh.TransformedNormalVectors[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }
            });
        }

        #endregion

        #endregion

        public Renderer(PixelFormat pixelFormat, int width, int height, Mesh model)
        {
            (PixelFormat, Width, Height, Mesh) = (pixelFormat, width, height, model);

            CameraTarget = FindGeometricAverage(model.OriginalVertices);
            LightPosition = CameraTarget + new Vector3(0, 100, 100);
            UpdateCameraUpVector();

            UpdateWritableBitmap();
        }

        #region Public Methods

        /// <summary>
        /// Recreates bitmap with new width and height
        /// </summary>
        public void UpdateWritableBitmap()
        {
            writer.Bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride)); ;
            writer.Clear();
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

            CameraPosition += (look / max) * distance;
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {

            Debug.WriteLine($"Render started. Rendering {Mesh.Polygons.Length} polygons");
            writer.Clear();

            TransformMatrixes matrixes = GetTransformMatrixes();

            Stopwatch.Restart();
            long prevMs = Stopwatch.ElapsedMilliseconds;

            ProjectVertices(matrixes.PerspectiveMatrix, matrixes.ViewportMatrix);
            ProjectNormals(matrixes.ViewMatrix, matrixes.ViewportMatrix);


            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            writer.DrawPolygons(Mesh.Polygons, Mesh.TransformedVertices, Mesh.TransformedNormalVectors, Colors.Gray, TriangleMode, LightPosition);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return writer.Bitmap;
        }

        #endregion

    }
}
