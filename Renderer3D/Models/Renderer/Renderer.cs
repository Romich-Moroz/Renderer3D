using Renderer3D.Models.Parser;
using Renderer3D.Models.Translation;
using Renderer3D.Models.WritableBitmap;
using System;
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
        /// <summary>
        /// Format of pixels for rendered bitmap
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height { get; set; }

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
        public float Fov { get; set;} = (float)System.Math.PI / 4;

        /// <summary>
        /// Current position of the camera itself
        /// </summary>
        public Vector3 EyeLocation { get; set;} = new Vector3 { X = 1, Y = 1, Z = 1 };

        /// <summary>
        /// Position where the camera actually looks
        /// </summary>
        public Vector3 TargetLocation { get; set;} = new Vector3 { X = 0, Y = 0, Z = 0 };

        /// <summary>
        /// Vertical vector from camera stand point
        /// </summary>
        public Vector3 CameraUpVector { get; set;} = new Vector3 { X = 0, Y = 1, Z = 0 };

        /// <summary>
        /// Parsed model to render on bitmap
        /// </summary>
        public ObjectModel ObjectModel { get; set; }

        private WriteableBitmap _bitmap { get; set; }

        public Renderer(PixelFormat pixelFormat, int width, int height, ObjectModel model)
        {
            (PixelFormat, Width, Height, ObjectModel) = (pixelFormat, width, height, model);
            UpdateWritableBitmap();
        }

        private void UpdateWritableBitmap()
        {
            _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));
            _bitmap.Clear(Colors.White);
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {
            //Init bitmap
            if (_bitmap.PixelWidth != Width || _bitmap.PixelHeight != Height)
            {
                UpdateWritableBitmap();
            }

            var vertices = new Point[ObjectModel.Vertices.Length];

            //Translate each vertex from model to view port
            for (int i = 0; i < ObjectModel.Vertices.Length; i++)
            {
                //Apply any moving, rotation, etc. to this vertex using model specific matrix
                var modelVert = ObjectModel.Vertices[i].RotateX(0).RotateY(0);
                var viewVert = modelVert.Translate(Translator.CreateViewMatrix(EyeLocation, TargetLocation, CameraUpVector));
                var projVert = viewVert.Translate(Translator.CreateProjectionMatrix(AspectRatio, Fov)).Normalize();
                var portVert = projVert.Translate(Translator.CreateViewportMatrix(Width, Height));
                //var portVert = projVert;

                vertices[i] = new Point { X = portVert.X, Y = portVert.Y };
            }

            //Connect vertices of polygons
            for (int i = 0; i < ObjectModel.Polygons.Length; i++)
            {
                for (int j = 0; j < ObjectModel.Polygons[i].Vertices.Length - 1; j++)
                {
                    _bitmap.DrawLineDda
                    (
                        vertices[ObjectModel.Polygons[i].Vertices[j].VertexIndex],
                        vertices[ObjectModel.Polygons[i].Vertices[j + 1].VertexIndex],
                        Colors.Black
                    );
                }
                _bitmap.DrawLineDda
                (
                    vertices[ObjectModel.Polygons[i].Vertices[ObjectModel.Polygons[i].Vertices.Length - 1].VertexIndex],
                    vertices[ObjectModel.Polygons[i].Vertices[0].VertexIndex],
                    Colors.Black
                );
            }

            //_bitmap.Freeze();
            return _bitmap;
        }
    }
}
