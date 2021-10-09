using Renderer3D.Models.Data;
using Renderer3D.Models.Processing;
using Renderer3D.Models.WriteableBitmapWriter;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Scene
{
    /// <summary>
    /// Represents the whole scene to render
    /// </summary>
    public class Scene
    {
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly WritableBitmapWriter BitmapWriter = new WritableBitmapWriter();

        private BitmapProperties _bitmapProperties = new BitmapProperties(PixelFormats.Bgr32, 800, 600);
        private ModelProperties _modelProperties = new ModelProperties(Vector3.One, Vector3.Zero, Vector3.Zero);
        private LightingProperties _lightingProperties = new LightingProperties(Vector3.Zero);
        private CameraProperties _cameraProperties = new CameraProperties(Vector3.One, Vector3.Zero, Vector3.UnitY, (float)Math.PI / 4);
        private RenderProperties _renderProperties = new RenderProperties(false);

        private Mesh _mesh;

        private void ProjectVertices(Matrix4x4 perspectiveMatrix, Matrix4x4 viewportMatrix)
        {
            _ = Parallel.ForEach(Partitioner.Create(0, _mesh.OriginalVertices.Length), Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    _mesh.TransformedVertices[i] = Projection.ProjectVertex(perspectiveMatrix, viewportMatrix, _mesh.OriginalVertices[i]);
                }
            });
        }

        private void ProjectNormals(Matrix4x4 viewMatrix, Matrix4x4 viewportMatrix)
        {
            _ = Parallel.ForEach(Partitioner.Create(0, _mesh.OriginalNormalVectors.Length), Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    _mesh.TransformedNormalVectors[i] = Projection.ProjectNormal(viewMatrix, viewportMatrix, _mesh.OriginalNormalVectors[i]);
                }
            });
        }

        private void UpdateWritableBitmap()
        {
            BitmapWriter.Bitmap = _bitmapProperties.CreateFromProperties();
            BitmapWriter.Clear();
        }

        public void OffsetModel(Vector3 offset)
        {
            _modelProperties.Offset += offset;
        }

        public void ScaleModel(Vector3 scale)
        {
            _modelProperties.Scale += scale;
        }

        public void OffsetCamera(Vector3 offset)
        {
            _cameraProperties.OffsetCamera(offset);
        }

        public void ResizeScene(int newWidth, int newHeight)
        {
            _bitmapProperties.Width = newWidth;
            _bitmapProperties.Height = newHeight;
            UpdateWritableBitmap();
        }

        public void RotateModel(Vector3 rotation)
        {
            _modelProperties.Rotation += rotation;
        }

        public Scene(PixelFormat pixelFormat, int width, int height, Mesh mesh)
        {
            _bitmapProperties = new BitmapProperties(pixelFormat, width, height);
            ChangeMesh(mesh);
        }

        public void ResetState()
        {
            _modelProperties.Scale = Vector3.One;
            _modelProperties.Offset = Vector3.Zero;
            _modelProperties.Rotation = Vector3.Zero;
            _cameraProperties.CameraPosition = Vector3.One;
            _cameraProperties.SetTargetToCenter(_mesh.OriginalVertices);
            _lightingProperties.LightSourcePosition = _cameraProperties.CameraTarget + new Vector3(-5, 100, 100);
            _renderProperties.TriangleMode = false;
        }

        public void ChangeMesh(Mesh mesh)
        {
            _mesh = mesh;
            ResetState();
            UpdateWritableBitmap();
        }

        public void ToggleTriangleMode()
        {
            _renderProperties.TriangleMode ^= true;
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource Render()
        {

            Debug.WriteLine($"Render started. Rendering {_mesh.Polygons.Length} polygons");
            BitmapWriter.Clear();

            TransformMatrixes matrixes = Projection.GetTransformMatrixes(_modelProperties, _cameraProperties, _bitmapProperties);

            Stopwatch.Restart();
            long prevMs = Stopwatch.ElapsedMilliseconds;

            ProjectVertices(matrixes.PerspectiveMatrix, matrixes.ViewportMatrix);
            ProjectNormals(matrixes.ViewMatrix, matrixes.ViewportMatrix);


            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            BitmapWriter.DrawPolygons(_mesh.Polygons, _mesh.TransformedVertices, _mesh.TransformedNormalVectors, Colors.Gray, _renderProperties.TriangleMode, _lightingProperties.LightSourcePosition);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return BitmapWriter.Bitmap;
        }
    }
}
