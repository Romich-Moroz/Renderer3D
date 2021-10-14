using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
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
                        Vector3 point = new Vector3(x, scanlineStruct.Y, z);
                        Vector3 viewVector = sceneProperties.CameraProperties.CameraPosition - point;
                        Vector3 lightVector = sceneProperties.LightingProperties.LightSourcePosition - point;
                        Vector3 hVector = Vector3.Normalize(viewVector + lightVector);


                        Vector3 bary = Calculation.GetFastBarycentricCoordinates
                        (
                            scanlineStruct.Triangle.v0.Coordinates,
                            scanlineStruct.Triangle.v1.Coordinates,
                            scanlineStruct.Triangle.v2.Coordinates,
                            point
                        );
                        Vector3 n = Vector3.Normalize(scanlineStruct.Triangle.v0.Normal * bary.X + scanlineStruct.Triangle.v1.Normal * bary.Y + scanlineStruct.Triangle.v2.Normal * bary.Z);

                        Vector3 ambient = sceneProperties.LightingProperties.AmbientIntensity;
                        Vector3 diffuse = Calculation.GetDiffuseLightingColor(sceneProperties.LightingProperties, lightVector, n);
                        Vector3 reflection = Calculation.GetReflectionLightingColor(sceneProperties.LightingProperties, hVector, n);

                        Vector3 intensity = ambient + diffuse + reflection;
                        intensity.X = Math.Min(intensity.X, 255);
                        intensity.Y = Math.Min(intensity.Y, 255);
                        intensity.Z = Math.Min(intensity.Z, 255);
                        _bitmapWriter.DrawPixel(x, scanlineStruct.Y, z, intensity.ToColorInt());

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

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (t.v0.Coordinates + t.v1.Coordinates + t.v2.Coordinates) / 3;

            float ndotl = Calculation.ComputeNDotL(sceneProperties.LightingProperties.LightSourcePosition - centerPoint, vnFace) * sceneProperties.LightingProperties.LightSourceIntensity;

            RasterizeTriangle(t, sceneProperties, (sceneProperties.RenderProperties.RenderFallbackColor * ndotl).ToColorInt());
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
            try
            {
                _ = Parallel.ForEach(Partitioner.Create(0, model.Polygons.Length), _options, Range =>
                {
                    for (int i = Range.Item1; i < Range.Item2; i++)
                    {
                        RenderPolygon(model.GetPolygonValue(model.Polygons[i]), sceneProperties);
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
