using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
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

        private void RenderTriangle(Triangle t, Vector3 lightPos, Color color)
        {
            if (Processing.IsTriangleInvisible(t))
            {
                return;
            }

            Processing.SortTriangleVerticesByY(ref t);

            Vector3 vnFace = (t.v0.Normal + t.v1.Normal + t.v2.Normal) / 3;
            Vector3 centerPoint = (t.v0.Coordinates + t.v1.Coordinates + t.v2.Coordinates) / 3;

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            float ndotl = Processing.ComputeNDotL(centerPoint, vnFace, lightPos);
            int shadowColor = Color.FromRgb((byte)(color.R * ndotl), (byte)(color.G * ndotl), (byte)(color.B * ndotl)).ToInt();

            //calculate inverse slopes
            double dP1P2, dP1P3;
            (dP1P2, dP1P3) = Processing.GetInverseSlopes(t);

            int min = (int)t.v0.Coordinates.Y > 0 ? (int)t.v0.Coordinates.Y : 0;
            int max = (int)t.v2.Coordinates.Y < _bitmapWriter.Height ? (int)t.v2.Coordinates.Y : _bitmapWriter.Width;

            for (int y = min; y <= max; y++)
            {
                if (y < t.v1.Coordinates.Y)
                {
                    ProcessScanLine(y, t.v0.Coordinates, dP1P2 > dP1P3 ? t.v2.Coordinates : t.v1.Coordinates,
                                        t.v0.Coordinates, dP1P2 > dP1P3 ? t.v1.Coordinates : t.v2.Coordinates, shadowColor);
                }
                else
                {
                    ProcessScanLine(y, dP1P2 > dP1P3 ? t.v0.Coordinates : t.v1.Coordinates, t.v2.Coordinates,
                                        dP1P2 > dP1P3 ? t.v1.Coordinates : t.v0.Coordinates, t.v2.Coordinates, shadowColor);
                }
            }
        }

        /// <summary>
        /// Draws polygon without triangulation
        /// </summary>
        private void RenderPolygon(Polygon p, Vector3[] vertices, Vector3[] normals, Color color, bool drawTriangles, Vector3 lightPos)
        {
            if (drawTriangles)
            {
                for (int i = 0; i < p.TriangleIndexes.Length; i++)
                {
                    Triangle triangle = new Triangle
                    {
                        v0 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index1.Vertex], Normal = normals[p.TriangleIndexes[i].Index1.Normal] },
                        v1 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index2.Vertex], Normal = normals[p.TriangleIndexes[i].Index2.Normal] },
                        v2 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index3.Vertex], Normal = normals[p.TriangleIndexes[i].Index3.Normal] }
                    };
                    RenderTriangle(triangle, lightPos, color);
                }
            }
            else
            {
                int colorInt = color.ToInt();
                for (int i = 0; i < p.PolygonVertices.Length; i++)
                {
                    if (i < p.PolygonVertices.Length - 1)
                    {
                        _bitmapWriter.DrawLine(vertices[p.PolygonVertices[i].Vertex].ToPoint(), vertices[p.PolygonVertices[i + 1].Vertex].ToPoint(), colorInt);
                    }
                    else
                    {
                        _bitmapWriter.DrawLine(vertices[p.PolygonVertices[^1].Vertex].ToPoint(), vertices[p.PolygonVertices[0].Vertex].ToPoint(), colorInt);
                    }
                }
            }
        }

        public void RenderPolygons(Polygon[] polygons, Vector3[] vertices, Vector3[] normals, Color color, bool drawTriangles, Vector3 lightPos)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            try
            {
                _ = Parallel.ForEach(Partitioner.Create(0, polygons.Length), options, Range =>
                {
                    for (int i = Range.Item1; i < Range.Item2; i++)
                    {
                        RenderPolygon(polygons[i], vertices, normals, color, drawTriangles, lightPos);
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
