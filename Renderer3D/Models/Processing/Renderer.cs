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

        private void ProcessScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, int color)
        {
            float gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            float gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Processing.Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Processing.Interpolate(pc.X, pd.X, gradient2);

            float z1 = Processing.Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Processing.Interpolate(pc.Z, pd.Z, gradient2);

            for (int x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                float z = Processing.Interpolate(z1, z2, gradient);

                _bitmapWriter.DrawPixel(x, y, z, color);
            }
        }

        private void RasterizeTriangle(TriangleValue t, int rasterizationColor)
        {
            Vector3 v0 = t.v0.Coordinates.ToV3();
            Vector3 v1 = t.v1.Coordinates.ToV3();
            Vector3 v2 = t.v2.Coordinates.ToV3();

            double dP1P2, dP1P3;
            (dP1P2, dP1P3) = Processing.GetInverseSlopes(v0, v1, v2);

            int min = (int)v0.Y > 0 ? (int)v0.Y : 0;
            int max = (int)v2.Y < _bitmapWriter.Height ? (int)v2.Y : _bitmapWriter.Width;

            for (int y = min; y <= max; y++)
            {
                if (y < v1.Y)
                {
                    ProcessScanLine(y, v0, dP1P2 > dP1P3 ? v2 : v1, v0, dP1P2 > dP1P3 ? v1 : v2, rasterizationColor);
                }
                else
                {
                    ProcessScanLine(y, dP1P2 > dP1P3 ? v0 : v1, v2, dP1P2 > dP1P3 ? v1 : v0, v2, rasterizationColor);
                }
            }
        }

        private void RenderTriangle(TriangleValue t, Vector3 lightPos, Color color)
        {
            if (Processing.IsTriangleInvisible(t))
            {
                return;
            }

            Processing.SortTriangleVerticesByY(ref t);

            Vector3 v0 = t.v0.Coordinates.ToV3();
            Vector3 v1 = t.v1.Coordinates.ToV3();
            Vector3 v2 = t.v2.Coordinates.ToV3();

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (v0 + v1 + v2) / 3;

            float ndotl = Processing.ComputeNDotL(centerPoint, vnFace, lightPos);
            int shadowColor = Processing.MultiplyColorByFloat(color, ndotl);

            RasterizeTriangle(t, shadowColor);
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
                        RenderTriangle(polygon.TriangleValues[i], lightProperties.LightSourcePosition, color);
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
