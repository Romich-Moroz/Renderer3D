using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Concurrency;
using Renderer3D.Models.Data.Properties;
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
        private ConcurrentBitmap _concurrentBitmap;
        private readonly ParallelOptions _options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        public WriteableBitmap Bitmap
        {
            get => _concurrentBitmap.WriteableBitmap;
            set => _concurrentBitmap = new ConcurrentBitmap(value);
        }

        private void ProcessScanLine(ScanlineStruct scanlineStruct, SceneProperties sceneProperties, MaterialProperties materialProperties, int color)
        {
            int cls = Math.Clamp(scanlineStruct.StartX, 0, _concurrentBitmap.Width);
            int cle = Math.Clamp(scanlineStruct.EndX, 0, _concurrentBitmap.Width);

            for (int x = cls; x < cle; x++)
            {
                float gradient = (x - scanlineStruct.StartX) / (float)(scanlineStruct.EndX - scanlineStruct.StartX);
                float z = Calculation.Interpolate(scanlineStruct.Z1, scanlineStruct.Z2, gradient);
                switch (sceneProperties.RenderProperties.RenderMode)
                {
                    case RenderMode.Flat:
                        _concurrentBitmap.DrawPixel(x, scanlineStruct.Y, z, color);
                        break;
                    case RenderMode.Phong:
                        _concurrentBitmap.DrawPixel
                        (
                            x,
                            scanlineStruct.Y,
                            z,
                            PhongShader.GetPixelColor
                            (
                                scanlineStruct.Triangle,
                                materialProperties,
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

        public void RasterizeTriangle(TriangleValue t, SceneProperties sceneProperties, MaterialProperties materialProperties)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }
            Calculation.SortTriangleVerticesByY(ref t);

            int min = (int)Math.Clamp(t.v0.Coordinates.Y, 0, _concurrentBitmap.Height);
            int max = (int)Math.Clamp(t.v2.Coordinates.Y, 0, _concurrentBitmap.Height);

            int color = default;
            if (sceneProperties.RenderProperties.RenderMode == RenderMode.Flat)
            {
                color = FlatShader.GetFaceColor(t, sceneProperties.LightingProperties, sceneProperties.RenderProperties.RenderFallbackColor);
            }

            for (int y = min; y <= max; y++)
            {
                ProcessScanLine(new ScanlineStruct(y, t), sceneProperties, materialProperties, color);
            }
        }

        #region FastTriangleRasterization (Not working)

        //private void DrawFlatTopTriangle(VertexValue v0, VertexValue v1, VertexValue v2, SceneProperties sceneProperties)
        //{
        //    float m0 = (v2.Coordinates.X - v0.Coordinates.X) / (v2.Coordinates.Y - v0.Coordinates.Y);
        //    float m1 = (v2.Coordinates.X - v1.Coordinates.X) / (v2.Coordinates.Y - v1.Coordinates.Y);

        //    int yStart = (int)Math.Ceiling(v0.Coordinates.Y - 0.5f);
        //    int yEnd = (int)Math.Ceiling(v2.Coordinates.Y - 0.5f);

        //    for (int y = yStart; y < yEnd; y++)
        //    {
        //        float px0 = m0 * (y + 0.5f - v0.Coordinates.Y) + v0.Coordinates.X;
        //        float px1 = m1 * (y + 0.5f - v1.Coordinates.Y) + v1.Coordinates.X;

        //        int xStart = (int)Math.Ceiling(px0 - 0.5f);
        //        int xEnd = (int)Math.Ceiling(px1 - 0.5f);

        //        int color = default;
        //        if (sceneProperties.RenderProperties.RenderMode == RenderMode.Flat)
        //        {
        //            color = FlatShader.GetFaceColor(new TriangleValue { v0 = v0, v1 = v1, v2 = v2 }, sceneProperties.LightingProperties, sceneProperties.RenderProperties.RenderFallbackColor);
        //        }

        //        for (int x = xStart; x < xEnd; x++)
        //        {
        //            _bitmapWriter.DrawPixel(x, y, color);
        //        }
        //    }
        //}

        //private void DrawFlatBottomTriangle(VertexValue v0, VertexValue v1, VertexValue v2, SceneProperties sceneProperties)
        //{
        //    float m0 = (v1.Coordinates.X - v0.Coordinates.X) / (v1.Coordinates.Y - v0.Coordinates.Y);
        //    float m1 = (v2.Coordinates.X - v0.Coordinates.X) / (v2.Coordinates.Y - v0.Coordinates.Y);

        //    int yStart = (int)Math.Ceiling(v0.Coordinates.Y - 0.5f);
        //    int yEnd = (int)Math.Ceiling(v2.Coordinates.Y - 0.5f);

        //    for (int y = yStart; y < yEnd; y++)
        //    {
        //        float px0 = m0 * (y + 0.5f - v0.Coordinates.Y) + v0.Coordinates.X;
        //        float px1 = m1 * (y + 0.5f - v0.Coordinates.Y) + v0.Coordinates.X;

        //        int xStart = (int)Math.Ceiling(px0 - 0.5f);
        //        int xEnd = (int)Math.Ceiling(px1 - 0.5f);

        //        int color = default;
        //        if (sceneProperties.RenderProperties.RenderMode == RenderMode.Flat)
        //        {
        //            color = FlatShader.GetFaceColor(new TriangleValue { v0 = v0, v1 = v1, v2 = v2 }, sceneProperties.LightingProperties, sceneProperties.RenderProperties.RenderFallbackColor);
        //        }

        //        for (int x = xStart; x < xEnd; x++)
        //        {
        //            _bitmapWriter.DrawPixel(x, y, color);
        //        }
        //    }
        //}

        //public void FastTriangleRasterization(TriangleValue t, SceneProperties sceneProperties)
        //{
        //    if (Calculation.IsTriangleInvisible(t))
        //    {
        //        return;
        //    }
        //    Calculation.SortTriangleVerticesByY(ref t);

        //    if (t.v0.Coordinates.Y == t.v1.Coordinates.Y)
        //    {
        //        if (t.v1.Coordinates.X < t.v0.Coordinates.X)
        //        {
        //            (t.v0, t.v1) = (t.v1, t.v0);
        //        }
        //        DrawFlatTopTriangle(t.v0, t.v1, t.v2, sceneProperties);
        //    }
        //    else if (t.v1.Coordinates.Y == t.v2.Coordinates.Y)
        //    {
        //        if (t.v2.Coordinates.X < t.v1.Coordinates.X)
        //        {
        //            (t.v1, t.v2) = (t.v2, t.v1);
        //        }
        //        DrawFlatBottomTriangle(t.v0, t.v1, t.v2, sceneProperties);
        //    }
        //    else
        //    {
        //        float alphaSplit = (t.v1.Coordinates.Y - t.v0.Coordinates.Y) / (t.v2.Coordinates.Y - t.v0.Coordinates.Y);
        //        Vector3 vCoords = t.v0.Coordinates + (t.v2.Coordinates - t.v0.Coordinates) * alphaSplit;
        //        Vector3 vNormal = t.v0.Normal + (t.v2.Normal - t.v0.Normal) * alphaSplit;
        //        Vector3 vTexture = t.v0.Texture + (t.v2.Texture - t.v0.Texture) * alphaSplit;

        //        var vValue = new VertexValue
        //        {
        //            Coordinates = vCoords,
        //            Normal = vNormal,
        //            Texture = vTexture
        //        };

        //        if (t.v1.Coordinates.X < vCoords.X)
        //        {
        //            DrawFlatBottomTriangle(t.v0, t.v1, vValue, sceneProperties);
        //            DrawFlatTopTriangle(t.v1, vValue, t.v2, sceneProperties);
        //        }
        //        else
        //        {
        //            DrawFlatBottomTriangle(t.v0, vValue, t.v1, sceneProperties);
        //            DrawFlatTopTriangle(vValue, t.v1, t.v2, sceneProperties);
        //        }
        //    }
        //}

        #endregion

        public void RenderPolygon(PolygonValue polygon, SceneProperties sceneProperties, MaterialProperties materialProperties)
        {
            switch (sceneProperties.RenderProperties.RenderMode)
            {
                case RenderMode.MeshOnly:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v0.Coordinates.ToPoint(), polygon.TriangleValues[i].v1.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v1.Coordinates.ToPoint(), polygon.TriangleValues[i].v2.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v2.Coordinates.ToPoint(), polygon.TriangleValues[i].v0.Coordinates.ToPoint(), sceneProperties.RenderProperties.RenderFallbackColorInt);
                    }
                    break;
                case RenderMode.Flat:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        //FastTriangleRasterization(polygon.TriangleValues[i], sceneProperties);
                        RasterizeTriangle(polygon.TriangleValues[i], sceneProperties, materialProperties);
                    }
                    break;
                case RenderMode.Phong:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        RasterizeTriangle(polygon.TriangleValues[i], sceneProperties, materialProperties);
                    }
                    break;
                default:
                    throw new NotImplementedException("Specified render mode is not supported");
            }
        }

        public void RenderModel(MeshProperties meshProperties, Model model, SceneProperties sceneProperties)
        {
            _ = Parallel.ForEach(Partitioner.Create(0, model.Polygons.Count), _options, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    RenderPolygon(model.GetPolygonValue(model.Polygons[i], meshProperties), sceneProperties, model.MaterialProperties);
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
            _concurrentBitmap.Clear();
        }

    }
}
