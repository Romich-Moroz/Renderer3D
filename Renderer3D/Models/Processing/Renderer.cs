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

        public WriteableBitmap Bitmap
        {
            get => _bitmapWriter.Bitmap;
            set => _bitmapWriter.Bitmap = value;
        }

        private void ProcessScanLine(ScanlineStruct scanlineStruct, int color, RenderProperties renderProperties)
        {
            for (int x = scanlineStruct.StartX; x < scanlineStruct.EndX; x++)
            {
                float gradient = (x - scanlineStruct.StartX) / (float)(scanlineStruct.EndX - scanlineStruct.StartX);
                float z = Calculation.Interpolate(scanlineStruct.Z1, scanlineStruct.Z2, gradient);
                switch (renderProperties.RenderMode)
                {
                    case RenderMode.FlatShading:
                        _bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, color);
                        break;
                    case RenderMode.PhongShading:

                        break;
                    default:
                        throw new NotImplementedException("Specified render mode is not implemented");
                }
            }
        }

        private void RasterizeTriangle(TriangleValue t, RenderProperties renderProperties, int rasterizationColor)
        {
            Calculation.SortTriangleVerticesByY(ref t);

            Vector3 v0 = t.v0.Coordinates.ToV3();
            Vector3 v1 = t.v1.Coordinates.ToV3();
            Vector3 v2 = t.v2.Coordinates.ToV3();

            double dP1P2, dP1P3;
            (dP1P2, dP1P3) = Calculation.GetInverseSlopes(v0, v1, v2);

            int min = (int)v0.Y > 0 ? (int)v0.Y : 0;
            int max = (int)v2.Y < _bitmapWriter.Height ? (int)v2.Y : _bitmapWriter.Width;

            for (int y = min; y <= max; y++)
            {
                if (y < v1.Y)
                {
                    ProcessScanLine(new ScanlineStruct(y, v0, dP1P2 > dP1P3 ? v2 : v1, v0, dP1P2 > dP1P3 ? v1 : v2), rasterizationColor, renderProperties);
                }
                else
                {
                    ProcessScanLine(new ScanlineStruct(y, dP1P2 > dP1P3 ? v0 : v1, v2, dP1P2 > dP1P3 ? v1 : v0, v2), rasterizationColor, renderProperties);
                }
            }
        }

        private void RenderFlatTriangle(TriangleValue t, LightingProperties lightProperties, RenderProperties renderProperties, Color color)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            Vector3 v0 = t.v0.Coordinates.ToV3();
            Vector3 v1 = t.v1.Coordinates.ToV3();
            Vector3 v2 = t.v2.Coordinates.ToV3();

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (v0 + v1 + v2) / 3;

            float ndotl = Calculation.ComputeNDotL(lightProperties.LightSourcePosition - centerPoint, vnFace) * lightProperties.Intensity;
            int shadowColor = Calculation.MultiplyColorByFloat(color, ndotl);

            RasterizeTriangle(t, renderProperties, shadowColor);
        }

        private void RenderPhongTriangle(TriangleValue t, LightingProperties lightProperties, RenderProperties renderProperties, Color color)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            Vector3 v0 = t.v0.Coordinates.ToV3();
            Vector3 v1 = t.v1.Coordinates.ToV3();
            Vector3 v2 = t.v2.Coordinates.ToV3();

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (v0 + v1 + v2) / 3;

            float ndotl = Calculation.ComputeNDotL(lightProperties.LightSourcePosition - centerPoint, vnFace) * lightProperties.Intensity;
            int shadowColor = Calculation.MultiplyColorByFloat(color, ndotl);

            RasterizeTriangle(t, renderProperties, shadowColor);
        }

        private void RenderPolygon(PolygonValue polygon, Color color, RenderProperties renderProperties, LightingProperties lightProperties)
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
                        RenderPhongTriangle(polygon.TriangleValues[i], lightProperties, renderProperties, color);
                    }
                    break;
                default:
                    throw new NotImplementedException("Specified render mode is not supported");
            }
        }

        public void RenderModel(Model model, Color color, RenderProperties renderProperties, LightingProperties lightProperties)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            try
            {
                _ = Parallel.ForEach(Partitioner.Create(0, model.Polygons.Length), options, Range =>
                {
                    for (int i = Range.Item1; i < Range.Item2; i++)
                    {
                        RenderPolygon(model.GetPolygonValue(model.Polygons[i]), color, renderProperties, lightProperties);
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
