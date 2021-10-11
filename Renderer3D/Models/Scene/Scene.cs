using Renderer3D.Models.Data;
using Renderer3D.Models.Processing;
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
        private readonly Renderer Renderer = new Renderer();

        private BitmapProperties _bitmapProperties = new BitmapProperties(PixelFormats.Bgr32, 800, 600);
        private ModelProperties _modelProperties = new ModelProperties(Vector3.One, Vector3.Zero, Vector3.Zero);
        private LightingProperties _lightingProperties = new LightingProperties(Vector3.Zero, 1);
        private CameraProperties _cameraProperties = new CameraProperties(Vector3.One, Vector3.Zero, Vector3.UnitY, (float)Math.PI / 4);
        private Mesh _mesh;

        public RenderProperties RenderProperties = new RenderProperties(RenderMode.LinesOnly);

        private void ProjectVertices(Matrix4x4 transformMatrix)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            _ = Parallel.ForEach(Partitioner.Create(0, _mesh.OriginalModel.Vertices.Length), options, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    _mesh.TransformedModel.Vertices[i] = Projection.ProjectVertex(transformMatrix, _mesh.OriginalModel.Vertices[i]);
                }
            });
        }

        private void ProjectNormals(Matrix4x4 worldMatrix)
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            _ = Parallel.ForEach(Partitioner.Create(0, _mesh.OriginalModel.Normals.Length), options, Range =>
            {
                for (int i = Range.Item1; i < Range.Item2; i++)
                {
                    _mesh.TransformedModel.Normals[i] = Projection.ProjectNormal(worldMatrix, _mesh.OriginalModel.Normals[i]);
                }
            });
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
            Renderer.Bitmap = _bitmapProperties.CreateFromProperties();
        }

        public void RotateModel(Vector3 rotation)
        {
            _modelProperties.Rotation += rotation;
        }

        public void RotateCamera(Vector3 rotationAngles)
        {
            _cameraProperties.RotateCameraX(rotationAngles.X);
            _cameraProperties.RotateCameraY(rotationAngles.Y);
            _cameraProperties.RotateCameraZ(rotationAngles.Z);
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
            _cameraProperties.CenterCamera(_mesh.OriginalModel.Vertices);
            _lightingProperties.LightSourcePosition = _cameraProperties.CameraTarget + new Vector3(-5, 100, 100);
            RenderProperties.RenderMode = RenderMode.LinesOnly;
        }

        public void ChangeMesh(Mesh mesh)
        {
            _mesh = mesh;
            ResetState();
            Renderer.Bitmap = _bitmapProperties.CreateFromProperties();
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource GetRenderedScene(Color renderColor)
        {

            Debug.WriteLine($"Render started. Rendering {_mesh.TransformedModel.Polygons.Length} polygons");
            Renderer.Clear();
            TransformMatrixes matrixes = Projection.GetTransformMatrixes(_modelProperties, _cameraProperties, _bitmapProperties);

            Stopwatch.Restart();
            long prevMs = Stopwatch.ElapsedMilliseconds;

            ProjectVertices(matrixes.TransformMatrix);
            ProjectNormals(matrixes.WorldMatrix);

            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            Renderer.RenderModel(_mesh.TransformedModel, renderColor, RenderProperties, _lightingProperties);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return Renderer.Bitmap;
        }
    }
}
