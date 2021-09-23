using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.WritableBitmap
{
    /// <summary>
    /// Drawing extensions for writeable bitmap
    /// </summary>
    public class WritableBitmapWriter
    {
        private static byte[] _blankBuffer;
        private const int lineColor = 0;
        private readonly int rastColor = Colors.Gray.ToInt();
        private WriteableBitmap _bitmap;

        private float[] _depthBuffer;
        private IntPtr backBuffer;
        private int stride;
        private int pixelWidth;
        private int pixelHeight;
        public WriteableBitmap Bitmap
        {
            get => _bitmap;
            set
            {
                int bitmapLength = value.PixelWidth * value.PixelHeight * 4;

                pixelWidth = value.PixelWidth;
                pixelHeight = value.PixelHeight;
                backBuffer = value.BackBuffer;
                stride = value.BackBufferStride;
                _blankBuffer = new byte[bitmapLength];
                _depthBuffer = new float[value.PixelWidth * value.PixelHeight];
                _bitmap = value;
            }
        }

        private void DrawLine(Point x1, Point x2, Color color)
        {
            double x2x1 = x2.X - x1.X;
            double y2y1 = x2.Y - x1.Y;
            double l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
            double xDelta = x2x1 / l;
            double yDelta = y2y1 / l;

            int color_data = color.ToInt();

            unsafe
            {
                for (int i = 0; i < l; i++)
                {
                    double x = x1.X + i * xDelta;
                    double y = x1.Y + i * yDelta;
                    if ((x >= 0 && y >= 0) && (x < pixelWidth && y < pixelHeight))
                    {
                        // Find the address of the pixel to draw.
                        IntPtr pBackBuffer = backBuffer + (int)y * stride + (int)x * 4;

                        // Assign the color data to the pixel.
                        *((int*)pBackBuffer) = color_data;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        private static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        private static float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            Vector3 lightDirection = lightPosition - vertex;
            return Math.Max(0, -Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(lightDirection)));
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void ProcessScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, int color)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            float gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            float gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            for (int x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                float z = Interpolate(z1, z2, gradient);

                if (x >= 0 && y >= 0 && x < pixelWidth && y < pixelHeight)
                {
                    IntPtr pBackBuffer = backBuffer + y * stride + x * 4;
                    int index = (x + y * pixelWidth);

                    if (_depthBuffer[index] < z)
                    {
                        continue; // Discard
                    }
                    _depthBuffer[index] = z;

                    unsafe
                    {
                        *((int*)pBackBuffer) = color;
                    }
                }
            }
        }

        private void DrawTriangle(Triangle t, Vector3 lookVector, Vector3 lightPos, Color color)
        {
            if (t.v1.Coordinates.Y == t.v2.Coordinates.Y && t.v1.Coordinates.Y == t.v3.Coordinates.Y ||
                Vector3.Dot(t.v1.Coordinates - lookVector, Vector3.Cross(t.v2.Coordinates - t.v1.Coordinates, t.v3.Coordinates - t.v1.Coordinates)) >= 0)
            {
                return; // i dont care about degenerate triangles
            }

            //sort by Y
            if (t.v1.Coordinates.Y > t.v2.Coordinates.Y)
            {
                (t.v1, t.v2) = (t.v2, t.v1);
            }

            if (t.v1.Coordinates.Y > t.v3.Coordinates.Y)
            {
                (t.v1, t.v3) = (t.v3, t.v1);
            }

            if (t.v2.Coordinates.Y > t.v3.Coordinates.Y)
            {
                (t.v2, t.v3) = (t.v3, t.v2);
            }

            // normal face's vector is the average normal between each vertex's normal
            // computing also the center point of the face
            Vector3 vnFace = (t.v1.Normal + t.v2.Normal + t.v3.Normal) / 3;
            Vector3 centerPoint = (t.v1.Coordinates + t.v2.Coordinates + t.v3.Coordinates) / 3;

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            float ndotl = ComputeNDotL(centerPoint, vnFace, lightPos);
            int shadowColor = Color.FromRgb((byte)(color.R * ndotl), (byte)(color.G * ndotl), (byte)(color.B * ndotl)).ToInt();
            //var intColor = color.ToInt();
            //int colorShaded = color.to

            //calculate inverse slopes
            double dP1P2, dP1P3;
            if (t.v2.Coordinates.Y - t.v1.Coordinates.Y > 0)
            {
                dP1P2 = (t.v2.Coordinates.X - t.v1.Coordinates.X) / (t.v2.Coordinates.Y - t.v1.Coordinates.Y);
            }
            else
            {
                dP1P2 = 0;
            }

            if (t.v3.Coordinates.Y - t.v1.Coordinates.Y > 0)
            {
                dP1P3 = (t.v3.Coordinates.X - t.v1.Coordinates.X) / (t.v3.Coordinates.Y - t.v1.Coordinates.Y);
            }
            else
            {
                dP1P3 = 0;
            }

            int min = (int)t.v1.Coordinates.Y > 0 ? (int)t.v1.Coordinates.Y : 0;
            int max = (int)t.v3.Coordinates.Y < pixelHeight ? (int)t.v3.Coordinates.Y : pixelHeight;

            for (int y = min; y <= max; y++)
            {
                if (y < t.v2.Coordinates.Y)
                {
                    ProcessScanLine(y, t.v1.Coordinates, dP1P2 > dP1P3 ? t.v3.Coordinates : t.v2.Coordinates,
                                        t.v1.Coordinates, dP1P2 > dP1P3 ? t.v2.Coordinates : t.v3.Coordinates, shadowColor);
                }
                else
                {
                    ProcessScanLine(y, dP1P2 > dP1P3 ? t.v1.Coordinates : t.v2.Coordinates, t.v3.Coordinates,
                                        dP1P2 > dP1P3 ? t.v2.Coordinates : t.v1.Coordinates, t.v3.Coordinates, shadowColor);
                }
            }
        }

        /// <summary>
        /// Draws polygon without triangulation
        /// </summary>
        private void DrawPolygon(Polygon p, Vector3[] vertices, Vector3[] normals, Color color, bool drawTriangles, Vector3 lookVector, Vector3 lightPos)
        {
            if (drawTriangles)
            {
                for (int i = 0; i < p.TriangleIndexes.Length; i++)
                {
                    Triangle triangle = new Triangle
                    {
                        v1 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index1.Vertex], Normal = normals[p.TriangleIndexes[i].Index1.Normal] },
                        v2 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index2.Vertex], Normal = normals[p.TriangleIndexes[i].Index2.Normal] },
                        v3 = new Vertex { Coordinates = vertices[p.TriangleIndexes[i].Index3.Vertex], Normal = normals[p.TriangleIndexes[i].Index3.Normal] }
                    };
                    DrawTriangle(triangle, lookVector, lightPos, color);
                }
            }
            else
            {
                for (int i = 0; i < p.PolygonVertices.Length; i++)
                {
                    if (i < p.PolygonVertices.Length - 1)
                    {
                        DrawLine(vertices[p.PolygonVertices[i].Vertex].ToPoint(), vertices[p.PolygonVertices[i + 1].Vertex].ToPoint(), color);
                    }
                    else
                    {
                        DrawLine(vertices[p.PolygonVertices[^1].Vertex].ToPoint(), vertices[p.PolygonVertices[0].Vertex].ToPoint(), color);
                    }
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        public void Clear()
        {
            for (int i = 0; i < _depthBuffer.Length; i++)
            {
                _depthBuffer[i] = float.MaxValue;
            }

            try
            {
                // Reserve the back buffer for updates.
                unsafe
                {
                    fixed (byte* b = _blankBuffer)
                    {
                        System.Runtime.CompilerServices.Unsafe.InitBlock(b, 255, (uint)_blankBuffer.Length);
                        CopyMemory(Bitmap.BackBuffer, (IntPtr)b, (uint)_blankBuffer.Length);
                    }
                }

                Bitmap.Lock();
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
            }
            finally
            {
                Bitmap.Unlock();
            }
        }

        public void DrawPolygons(Polygon[] polygons, Vector3[] vertices, Vector3[] normals, Color color, bool drawTriangles, Vector3 lookVector, Vector3 lightPos)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            try
            {
                Parallel.ForEach(Partitioner.Create(0, polygons.Length), options, Range =>
                 {
                     for (int i = Range.Item1; i < Range.Item2; i++)
                     {
                         DrawPolygon(polygons[i], vertices, normals, color, drawTriangles, lookVector, lightPos);
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

    }
}
