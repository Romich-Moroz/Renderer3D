using Renderer3D.Models.Parser;
using Renderer3D.Models.Translation;
using Renderer3D.Models.WritableBitmap;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
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
        private Point[] _Vertices { get; set; }
        private WriteableBitmap _bitmap { get; set; }
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


        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public ObjectModel ObjectModel { get; set; }

        private void UpdateWritableBitmap()
        {
            _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));
            _bitmap.Clear();
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

        public Renderer(PixelFormat pixelFormat, int width, int height, ObjectModel model)
        {
            (PixelFormat, Width, Height, ObjectModel) = (pixelFormat, width, height, model);

            CameraTarget = FindGeometricAverage(model.Vertices);
            UpdateCameraUpVector();

            UpdateWritableBitmap();
            _Vertices = new Point[ObjectModel.Vertices.Length];
        }



        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {
            
            Debug.WriteLine($"Render started. Rendering {ObjectModel.Polygons.Length} polygons");
            _bitmap.Clear();
            Matrix4x4 translationMatrix = Matrix4x4.CreateScale(Scale) *
                                    Matrix4x4.CreateRotationX(_ModelRotationX) *
                                    Matrix4x4.CreateRotationY(_ModelRotationY) *
                                    Matrix4x4.CreateTranslation(Offset) *
                                    Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUpVector) *
                                    Matrix4x4.CreatePerspectiveFieldOfView(Fov, AspectRatio, 1, 100);
            Matrix4x4 viewportMatrix = Translator.CreateViewportMatrix(Width, Height);

            Stopwatch.Restart();
            var prevMs = Stopwatch.ElapsedMilliseconds;

            Parallel.ForEach(Partitioner.Create(0, ObjectModel.Vertices.Length), Range =>
             {
                 for (int i = Range.Item1; i < Range.Item2; i++)
                 {
                     Vector4 portVert = Vector4.Transform
                     (
                         Vector4.Transform
                         (
                             ObjectModel.Vertices[i],
                             translationMatrix
                         ).PerspectiveDivide(),
                         viewportMatrix
                     );

                     _Vertices[i] = new Point { X = portVert.X, Y = portVert.Y };
                 }
             });
            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            _bitmap.DrawPolygons(ObjectModel.Polygons, _Vertices, Colors.Black);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return _bitmap;
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
