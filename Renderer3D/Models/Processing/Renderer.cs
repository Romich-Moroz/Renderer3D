using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using Renderer3D.Models.Scene;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Processing
{
    public class Renderer
    {
        private readonly BitmapWriter _bitmapWriter = new BitmapWriter();
        private readonly ParallelOptions _options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        public WriteableBitmap Bitmap
        {
            get => _bitmapWriter.Bitmap;
            set => _bitmapWriter.Bitmap = value;
        }

        private void ProcessScanLine(ScanlineStruct scanlineStruct, CameraProperties cameraProperties, LightingProperties lightProperties, RenderProperties renderProperties, Color color)
        {
            int colorInt = color.ToInt();
            for (int x = scanlineStruct.StartX; x < scanlineStruct.EndX; x++)
            {
                float gradient = (x - scanlineStruct.StartX) / (float)(scanlineStruct.EndX - scanlineStruct.StartX);
                float z = Calculation.Interpolate(scanlineStruct.Z1, scanlineStruct.Z2, gradient);
                switch (renderProperties.RenderMode)
                {
                    case RenderMode.FlatShading:
                        _bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, colorInt);
                        break;
                    case RenderMode.PhongShading:
                        Vector3 point = new Vector3(x, scanlineStruct.Y, z);
                        Vector3 bary = Calculation.GetBarycentricCoordinates
                        (
                            scanlineStruct.Triangle.v0.Coordinates,
                            scanlineStruct.Triangle.v1.Coordinates,
                            scanlineStruct.Triangle.v2.Coordinates,
                            point
                        );
                        Vector3 n = Vector3.Normalize(scanlineStruct.Triangle.v0.Normal * bary.X + scanlineStruct.Triangle.v1.Normal * bary.Y + scanlineStruct.Triangle.v2.Normal * bary.Z);
                        Vector3 ambient = Calculation.GetAmbientLightingColor(lightProperties);
                        Vector3 diffuse = Calculation.GetDiffuseLightingColor(lightProperties, point, n);
                        Vector3 reflection = Calculation.GetReflectionLightingColor(cameraProperties, lightProperties, point, n);

                        Vector3 intensity = ambient + diffuse + reflection;
                        intensity.X = intensity.X > 255 ? 255 : intensity.X;
                        intensity.Y = intensity.Y > 255 ? 255 : intensity.Y;
                        intensity.Z = intensity.Z > 255 ? 255 : intensity.Z;
                        _bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, Color.FromRgb((byte)intensity.X, (byte)intensity.Y, (byte)intensity.Z).ToInt());

                        //float ndotl = Calculation.ComputeNDotL(lightProperties.LightSourcePosition - new Vector3(x, scanlineStruct.Y, z), n) * lightProperties.LightSourceIntensity;
                        //_bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, color.Multiply(ndotl).ToInt());

                        break;
                    default:
                        throw new NotImplementedException("Specified render mode is not implemented");
                }
            }
        }

        private void RasterizeTriangle(TriangleValue t, CameraProperties cameraProperties, LightingProperties lightProperties, RenderProperties renderProperties, Color color = default)
        {
            Calculation.SortTriangleVerticesByY(ref t);

            int min = (int)t.v0.Coordinates.Y > 0 ? (int)t.v0.Coordinates.Y : 0;
            int max = (int)t.v2.Coordinates.Y < _bitmapWriter.Height ? (int)t.v2.Coordinates.Y : _bitmapWriter.Width;

            for (int y = min; y <= max; y++)
            {
                ProcessScanLine(new ScanlineStruct(y, t), cameraProperties, lightProperties, renderProperties, color);
            }
        }

        private void RenderFlatTriangle(TriangleValue t, LightingProperties lightProperties, RenderProperties renderProperties, Color color)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (t.v0.Coordinates + t.v1.Coordinates + t.v2.Coordinates) / 3;

            float ndotl = Calculation.ComputeNDotL(lightProperties.LightSourcePosition - centerPoint, vnFace) * lightProperties.LightSourceIntensity;

            RasterizeTriangle(t, default, lightProperties, renderProperties, color.Multiply(ndotl));
        }

        private void RenderPhongTriangle(TriangleValue t, CameraProperties cameraProperties, LightingProperties lightProperties, RenderProperties renderProperties, Color color)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            RasterizeTriangle(t, cameraProperties, lightProperties, renderProperties, color);
        }

        private void RenderPolygon(PolygonValue polygon, Color color, RenderProperties renderProperties, LightingProperties lightProperties, CameraProperties cameraProperties)
        {
            switch (renderProperties.RenderMode)
            {
                case RenderMode.LinesOnly:
                    int colorInt = color.ToInt();
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v0.Coordinates.ToPoint(), polygon.TriangleValues[i].v1.Coordinates.ToPoint(), colorInt);
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v1.Coordinates.ToPoint(), polygon.TriangleValues[i].v2.Coordinates.ToPoint(), colorInt);
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v2.Coordinates.ToPoint(), polygon.TriangleValues[i].v0.Coordinates.ToPoint(), colorInt);
                    }
                    break;
                case RenderMode.FlatShading:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        RenderFlatTriangle(polygon.TriangleValues[i], lightProperties, renderProperties, color);
                    }
                    break;
                case RenderMode.PhongShading:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        RenderPhongTriangle(polygon.TriangleValues[i], cameraProperties, lightProperties, renderProperties, color);
                    }
                    break;
                default:
                    throw new NotImplementedException("Specified render mode is not supported");
            }
        }

        public void RenderModel(Model model, Color color, RenderProperties renderProperties, LightingProperties lightProperties, CameraProperties cameraProperties)
        {

            try
            {
                _ = Parallel.ForEach(Partitioner.Create(0, model.Polygons.Length), _options, Range =>
                {
                    for (int i = Range.Item1; i < Range.Item2; i++)
                    {
                        RenderPolygon(model.GetPolygonValue(model.Polygons[i]), color, renderProperties, lightProperties, cameraProperties);
                    }
                });
                Bitmap.Lock();
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                Bitmap.Unlock();
            }
        }

        public void Clear()
        {
            _bitmapWriter.Clear();
        }

    }
}
