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
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly WritableBitmapWriter writer = new WritableBitmapWriter();
        private Vector3[] Vertices { get; set; }
        private Vector3[] Normals { get; set; }
        private int _width = 800;
        private float _ModelRotationX = 0;
        private float _ModelRotationY = 0;

        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    UpdateWritableBitmap();
                }
            }
        }

        private int _height = 600;

        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    UpdateWritableBitmap();
                }
            }
        }

        /// <summary>
        /// Format of pixels for rendered bitmap
        /// </summary>
        public readonly PixelFormat PixelFormat = PixelFormats.Bgr32;
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Position of the camera itself
        /// </summary>
        public Vector3 CameraPosition { get; set; } = Vector3.One;

        /// <summary>
        /// Width of the row of pixels of the bitmap
        /// </summary>
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Aspect ration of the screen aka Width / Height
        /// </summary>
        public float AspectRatio => (float)Width / Height;

        /// <summary>
        /// Camera field of view
        /// </summary>
        public float Fov { get; set; } = (float)Math.PI / 4;

        /// <summary>
        /// Position where the camera actually looks
        /// </summary>
        public Vector3 CameraTarget { get; set; } = Vector3.Zero;

        /// <summary>
        /// Vertical vector from camera stand point
        /// </summary>
        public Vector3 CameraUpVector { get; set; } = Vector3.UnitY;

        public Vector3 Offset { get; set; } = Vector3.Zero;
        public Vector3 LightPosition { get; set; } = Vector3.Zero;
        public bool TriangleMode { get; set; } = false;


        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public Mesh Mesh { get; set; }

        private void UpdateWritableBitmap()
        {
            writer.Bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride)); ;
            writer.Clear();
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

        private void UpdateCameraUpVector()
        {
            Vector3 lookVector = Vector3.Normalize(CameraPosition - CameraTarget);
            Vector3 rightVector = Vector3.Cross(lookVector, Vector3.UnitY);
            CameraUpVector = Vector3.Cross(rightVector, lookVector);
        }

        public Renderer(PixelFormat pixelFormat, int width, int height, Mesh model)
        {
            (PixelFormat, Width, Height, Mesh) = (pixelFormat, width, height, model);

            CameraTarget = FindGeometricAverage(model.Vertices);
            LightPosition = CameraTarget + new Vector3(0, 100, 100);
            UpdateCameraUpVector();

            UpdateWritableBitmap();
            Vertices = new Vector3[Mesh.Vertices.Length];
            Normals = new Vector3[Mesh.NormalVectors.Length];
        }



        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {

            Debug.WriteLine($"Render started. Rendering {Mesh.Polygons.Length} polygons");
            writer.Clear();
            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(Scale) *
                                    Matrix4x4.CreateRotationX(_ModelRotationX) *
                                    Matrix4x4.CreateRotationY(_ModelRotationY) *
                                    Matrix4x4.CreateTranslation(Offset) *
                                    Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUpVector);
            Matrix4x4 perspectiveMatrix = worldMatrix *
                                    Matrix4x4.CreatePerspectiveFieldOfView(Fov, AspectRatio, 1, 100);
            Matrix4x4 viewportMatrix = Translator.CreateViewportMatrix(Width, Height);

            Stopwatch.Restart();
            long prevMs = Stopwatch.ElapsedMilliseconds;

            Parallel.ForEach(Partitioner.Create(0, Mesh.Vertices.Length), Range =>
             {
                 for (int i = Range.Item1; i < Range.Item2; i++)
                 {
                     Vector4 coordinates = Vector4.Transform
                     (
                         Vector4.Transform
                         (
                             Mesh.Vertices[i],
                             perspectiveMatrix
                         ).PerspectiveDivide(),
                         viewportMatrix
                     );
                     Vertices[i] = new Vector3(coordinates.X, coordinates.Y, coordinates.Z);
                 }
             });
            Parallel.ForEach(Partitioner.Create(0, Mesh.NormalVectors.Length), Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    Vector4 normal = Vector4.Transform
                    (
                        Vector4.Transform
                        (
                            Mesh.NormalVectors[i],
                            worldMatrix
                        ),
                        viewportMatrix
                    );
                    Normals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }
            });
            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            writer.DrawPolygons(Mesh.Polygons, Vertices, Normals, Colors.Gray, TriangleMode, CameraPosition, LightPosition);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return writer.Bitmap;
        }

        public void RotateCameraX(float angle)
        {
            RotateCamera(Vector3.UnitX, angle);
        }

        public void RotateCameraY(float angle)
        {
            RotateCamera(Vector3.UnitY, angle);
        }

        public void RotateModelX(float angle)
        {
            _ModelRotationX += angle;
        }

        public void RotateModelY(float angle)
        {
            _ModelRotationY += angle;
        }

        public void OffsetCamera(Vector3 offset)
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

            CameraPosition += (look / max) * offset;
        }
    }
}
