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

        private void DrawFlatTriangle(VertexValue v0, VertexValue v1, VertexValue v2, VertexValue dv0, VertexValue dv1, VertexValue itEdge1, SceneProperties sceneProperties, MaterialProperties materialProperties, float ndotl)
        {
            VertexValue itEdge0 = v0;

            int yStart = (int)Math.Ceiling(v0.Coordinates.Y - 0.5f);
            int yEnd = (int)Math.Ceiling(v2.Coordinates.Y - 0.5f);

            itEdge0 += dv0 * (yStart + 0.5f - v0.Coordinates.Y);
            itEdge1 += dv1 * (yStart + 0.5f - v0.Coordinates.Y);

            for (int y = yStart; y < yEnd; y++, itEdge0 += dv0, itEdge1 += dv1)
            {
                int xStart = (int)Math.Ceiling(itEdge0.Coordinates.X - 0.5f);
                int xEnd = (int)Math.Ceiling(itEdge1.Coordinates.X - 0.5f);

                VertexValue iLine = itEdge0;

                float dx = itEdge1.Coordinates.X - itEdge0.Coordinates.X;
                VertexValue diLine = (itEdge1 - iLine) / dx;

                iLine += diLine * (xStart + 0.5f - itEdge0.Coordinates.X);

                for (int x = xStart; x < xEnd; x++, iLine += diLine)
                {
                    float w = 1.0f / iLine.Coordinates.W;
                    VertexValue interpPixel = iLine * w;

                    switch (sceneProperties.RenderProperties.RenderMode)
                    {
                        case ShadingMode.Flat:
                            var color = materialProperties.TexturesBitmap?.GetColor(interpPixel.Texture.X, interpPixel.Texture.Y) ?? MaterialProperties.DefaultDiffuseColor;
                            _concurrentBitmap.DrawPixel(x, y, w, FlatShader.GetFaceColor(color, ndotl));
                            break;
                        case ShadingMode.Phong:
                            _concurrentBitmap.DrawPixel(x, y, w, PhongShader.GetPixelColor(materialProperties, sceneProperties.LightingProperties, sceneProperties.CameraProperties, interpPixel));
                            break;
                        default:
                            throw new NotImplementedException("Specified render mode is not implemented");
                    }
                }
            }
        }

        private void DrawFlatTopTriangle(VertexValue v0, VertexValue v1, VertexValue v2, SceneProperties sceneProperties, MaterialProperties materialProperties, float ndotl)
        {
            float deltaY = v2.Coordinates.Y - v0.Coordinates.Y;
            VertexValue dv0 = (v2 - v0) / deltaY;
            VertexValue dv1 = (v2 - v1) / deltaY;

            DrawFlatTriangle(v0, v1, v2, dv0, dv1, v1, sceneProperties, materialProperties, ndotl);
        }

        private void DrawFlatBottomTriangle(VertexValue v0, VertexValue v1, VertexValue v2, SceneProperties sceneProperties, MaterialProperties materialProperties, float ndotl)
        {
            float deltaY = v2.Coordinates.Y - v0.Coordinates.Y;
            VertexValue dv0 = (v1 - v0) / deltaY;
            VertexValue dv1 = (v2 - v0) / deltaY;

            DrawFlatTriangle(v0, v1, v2, dv0, dv1, v0, sceneProperties, materialProperties, ndotl);
        }

        private void CorrectTransform(ref VertexValue v)
        {
            float wInv = 1.0f / v.Coordinates.W;
            v *= wInv;
            v.Coordinates = Vector4.Transform(v.Coordinates, Projection.LastGetTransformMatrixesResult.ViewportMatrix);
            v.Coordinates.W = wInv;
        }

        public void RasterizeTriangle(TriangleValue t, SceneProperties sceneProperties, MaterialProperties materialProperties)
        {
            if (Calculation.IsTriangleInvisible(t))
            {
                return;
            }

            Calculation.SortTriangleVerticesByY(ref t);

            float ndotl = default;
            if (sceneProperties.RenderProperties.RenderMode == ShadingMode.Flat)
            {
                ndotl = FlatShader.GetNdotL(t, sceneProperties.LightingProperties);
            }

            if (t.v0.Coordinates.Y == t.v1.Coordinates.Y) // Natural flat top
            {
                if (t.v1.Coordinates.X < t.v0.Coordinates.X)
                {
                    (t.v0, t.v1) = (t.v1, t.v0);
                }
                DrawFlatTopTriangle(t.v0, t.v1, t.v2, sceneProperties, materialProperties, ndotl);
            }
            else if (t.v1.Coordinates.Y == t.v2.Coordinates.Y) // Natural flat bottom
            {
                if (t.v2.Coordinates.X < t.v1.Coordinates.X)
                {
                    (t.v1, t.v2) = (t.v2, t.v1);
                }
                DrawFlatBottomTriangle(t.v0, t.v1, t.v2, sceneProperties, materialProperties, ndotl);
            }
            else //Generic triangle
            {
                float interpRatio = t.GetInterpolationRatioY();
                VertexValue splittingVertex = t.v0.InterpolateTo(t.v2, interpRatio);

                if (t.v1.Coordinates.X < splittingVertex.Coordinates.X) //Major right
                {
                    DrawFlatBottomTriangle(t.v0, t.v1, splittingVertex, sceneProperties, materialProperties, ndotl);
                    DrawFlatTopTriangle(t.v1, splittingVertex, t.v2, sceneProperties, materialProperties, ndotl);
                }
                else //Major left
                {
                    DrawFlatBottomTriangle(t.v0, splittingVertex, t.v1, sceneProperties, materialProperties, ndotl);
                    DrawFlatTopTriangle(splittingVertex, t.v1, t.v2, sceneProperties, materialProperties, ndotl);
                }
            }
        }

        public void RenderPolygon(PolygonValue polygon, SceneProperties sceneProperties, MaterialProperties materialProperties)
        {
            switch (sceneProperties.RenderProperties.RenderMode)
            {
                case ShadingMode.None:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v0.Coordinates.ToPoint(), polygon.TriangleValues[i].v1.Coordinates.ToPoint(), sceneProperties.RenderProperties.MeshLineColor);
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v1.Coordinates.ToPoint(), polygon.TriangleValues[i].v2.Coordinates.ToPoint(), sceneProperties.RenderProperties.MeshLineColor);
                        _concurrentBitmap.DrawLine(polygon.TriangleValues[i].v2.Coordinates.ToPoint(), polygon.TriangleValues[i].v0.Coordinates.ToPoint(), sceneProperties.RenderProperties.MeshLineColor);
                    }
                    break;
                case ShadingMode.Flat:
                case ShadingMode.Phong:
                case ShadingMode.NormalMap:
                    for (int i = 0; i < polygon.TriangleValues.Length; i++)
                    {
                        CorrectTransform(ref polygon.TriangleValues[i].v0);
                        CorrectTransform(ref polygon.TriangleValues[i].v1);
                        CorrectTransform(ref polygon.TriangleValues[i].v2);
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
                Bitmap.Unlock();
            }
        }

        public void Clear()
        {
            _concurrentBitmap.Clear();
        }

    }
}
