using Renderer3D.Models.Parser;
using Renderer3D.Models.Translation;
using Renderer3D.Models.WritableBitmap;
using System;
using System.Diagnostics;
using System.Numerics;
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

        private int _width = 800;
        private int _height = 600;
        private Vector3 _scale = new Vector3 { X = 1f, Y = 1f, Z = 1f };

        /// <summary>
        /// Format of pixels for rendered bitmap
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

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

        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    UpdateWritableBitmap();
                }
            }
        }

        public Vector3 Eye { get; set; } = new Vector3 { X = 1, Y = 1, Z = 1 };


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
        public float Fov { get; set; } = (float)System.Math.PI / 4;

        /// <summary>
        /// Position where the camera actually looks
        /// </summary>
        public Vector3 TargetLocation { get; set; } = new Vector3 { X = 0, Y = 0, Z = 0 };

        /// <summary>
        /// Vertical vector from camera stand point
        /// </summary>
        public Vector3 CameraUpVector { get; set; } = new Vector3 { X = 0, Y = 1, Z = 0 };

        public float RotationX { get; set; }

        public float RotationY { get; set; }

        public Vector3 Offset { get; set; } = new Vector3 { X = 0, Y = 0, Z = 0 };

        private Stopwatch Stopwatch = new Stopwatch();
        private Point[] Vertices { get; set; }


        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public ObjectModel ObjectModel { get; set; }

        private WriteableBitmap _bitmap { get; set; }

        public Renderer(PixelFormat pixelFormat, int width, int height, ObjectModel model)
        {
            (PixelFormat, Width, Height, ObjectModel) = (pixelFormat, width, height, model);
            UpdateWritableBitmap();
            Vertices = new Point[ObjectModel.Vertices.Length];
        }

        private void UpdateWritableBitmap()
        {
            _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));
            _bitmap.Clear();
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {
            Debug.WriteLine("Render started");
            Stopwatch.Restart();

            _bitmap.Clear();
            Debug.WriteLine($"Clear time: {Stopwatch.ElapsedMilliseconds}");

            var translation = Translator.CreateViewportMatrix(Width, Height) *
                              Translator.CreateProjectionMatrix(AspectRatio, Fov) *
                              Translator.CreateViewMatrix(Eye, TargetLocation, CameraUpVector) *
                              Translator.CreateScaleMatrix(Scale) *
                              Translator.CreateXRotationMatrix(RotationX) *
                              Translator.CreateYRotationMatrix(RotationY) *
                              Translator.CreateMovingMatrix(Offset);
            Debug.WriteLine($"Translation matrix time: {Stopwatch.ElapsedMilliseconds}");

            for (int i = 0; i < ObjectModel.Vertices.Length; i++)
            {
                Vector4 portVert = ObjectModel.Vertices[i]
                    .Translate(translation)
                    .Normalize();

                Vertices[i] = new Point { X = portVert.X, Y = portVert.Y };
            }
            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds}");

            //Connect vertices of polygons
            _bitmap.DrawPolygons(ObjectModel.Polygons, Vertices, Colors.Black);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds}");
            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();
            //_bitmap.Freeze();
            return _bitmap;
        }
    }
}
