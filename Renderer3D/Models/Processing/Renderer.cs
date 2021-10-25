using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using Renderer3D.Models.Processing.Shaders;
using Renderer3D.Models.Scene;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
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

        private void ProcessScanLine(ScanlineStruct scanlineStruct, SceneProperties sceneProperties, int color)
        {
            int cls = Math.Clamp(scanlineStruct.StartX, 0, _bitmapWriter.Width);
            int cle = Math.Clamp(scanlineStruct.EndX, 0, _bitmapWriter.Width);

            for (int x = cls; x < cle; x++)
            {
                float gradient = (x - scanlineStruct.StartX) / (float)(scanlineStruct.EndX - scanlineStruct.StartX);
                float z = Calculation.Interpolate(scanlineStruct.Z1, scanlineStruct.Z2, gradient);
                switch (sceneProperties.RenderProperties.RenderMode)
                {
                    case RenderMode.FlatShading:
                        _bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, color);
                        break;
                    case RenderMode.PhongShading:
                        ;
                        _bitmapWriter.DrawPixel
                        (
                            x,
                            scanlineStruct.Y,
                            z,
                            PhongShader.GetPixelColor
                            (
                                scanlineStruct.Triangle,
                                sceneProperties.LightingProperties,
                                sceneProperties.CameraProperties,
                                new Vector3(x, scanlineStruct.Y, z)
                            )
                        );

                        break;
                    default:
                        throw new NotImplementedException("Specified render mode is not implemented");
                }
            }
        }

        private void RasterizeTriangle(TriangleValue t, SceneProperties sceneProperties, int color)
        {
            Calculation.SortTriangleVerticesByY(ref t);

            int min = (int)Math.Clamp(t.v0.Coordinates.Y, 0, _bitmapWriter.Height);
            int max = (int)Math.Clamp(t.v2.Coordinates.Y, 0, _bitmapWriter.Height);

            for (int y = min; y <= max; y++)
            {
                ProcessScanLine(new ScanlineStruct(y, t), sceneProperties, color);
            }
        }

        private void RenderFlatTriangle(TriangleValue t, SceneProperties sceneProperties)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }
            RasterizeTriangle(t, sceneProperties, FlatShader.GetFaceColor(t, sceneProperties.LightingProperties, sceneProperties.RenderProperties.RenderFallbackColor));
        }

        private void RenderPhongTriangle(TriangleValue t, SceneProperties sceneProperties)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            RasterizeTriangle(t, sceneProperties, default);
        }

        private void RenderPolygon(PolygonValue polygon, SceneProperties sceneProperties)
        {
            switch (sceneProperties.RenderProperties.RenderMode)
            {
                case RenderMode.LinesOnly:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v0.Coordinates.ToPoint(), polygon.TriangleValues[i].v1.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v1.Coordinates.ToPoint(), polygon.TriangleValues[i].v2.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                        _bitmapWriter.DrawLine(polygon.TriangleValues[i].v2.Coordinates.ToPoint(), polygon.TriangleValues[i].v0.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                    }
                    break;
                case RenderMode.FlatShading:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        RenderFlatTriangle(polygon.TriangleValues[i], sceneProperties);
                    }
                    break;
                case RenderMode.PhongShading:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        RenderPhongTriangle(polygon.TriangleValues[i], sceneProperties);
                    }
                    break;
                default:
                    throw new NotImplementedException("Specified render mode is not supported");
            }
        }

        public void RenderModel(Model model, SceneProperties sceneProperties)
        {
            _ = Parallel.ForEach(Partitioner.Create(0, model.Polygons.Length), _options, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    RenderPolygon(model.GetPolygonValue(model.Polygons[i]), sceneProperties);
                }
            });

            try
            {
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
