using Renderer3D.Models.Data;
using Renderer3D.Models.Processing;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
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
        public readonly SceneProperties SceneProperties = new SceneProperties();

        private Mesh _mesh;

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
            SceneProperties.ModelProperties.Offset += offset;
        }

        public void ScaleModel(Vector3 scale)
        {
            SceneProperties.ModelProperties.Scale += scale;
        }

        public void OffsetCamera(Vector3 offset)
        {
            SceneProperties.CameraProperties.OffsetCamera(offset);
        }

        public void ResizeScene(int newWidth, int newHeight)
        {

            SceneProperties.BitmapProperties.Width = newWidth;
            SceneProperties.BitmapProperties.Height = newHeight;
            Renderer.Bitmap = SceneProperties.BitmapProperties.CreateFromProperties();
        }

        public void RotateModel(Vector3 rotation)
        {
            SceneProperties.ModelProperties.Rotation += rotation;
        }

        public void RotateCamera(Vector3 rotationAngles)
        {
            SceneProperties.CameraProperties.RotateCameraX(rotationAngles.X);
            SceneProperties.CameraProperties.RotateCameraY(rotationAngles.Y);
            SceneProperties.CameraProperties.RotateCameraZ(rotationAngles.Z);
        }

        public Scene(int width, int height, Mesh mesh)
        {
            SceneProperties.BitmapProperties.Width = width;
            SceneProperties.BitmapProperties.Height = height;
            ChangeMesh(mesh);
        }

        public void ResetState()
        {
            SceneProperties.ModelProperties.Reset();
            SceneProperties.RenderProperties.Reset();
            SceneProperties.CameraProperties.Reset();
            SceneProperties.CameraProperties.CenterCamera(_mesh.OriginalModel.Vertices);
            SceneProperties.LightingProperties.LightSourcePosition = SceneProperties.CameraProperties.CameraTarget + new Vector3(-5, 100, 100);

        }

        public void ChangeMesh(Mesh mesh)
        {
            _mesh = mesh;
            ResetState();
            Renderer.Bitmap = SceneProperties.BitmapProperties.CreateFromProperties();
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource GetRenderedScene()
        {

            Debug.WriteLine($"Render started. Rendering {_mesh.TransformedModel.Polygons.Length} polygons");
            Renderer.Clear();
            TransformMatrixes matrixes = Projection.GetTransformMatrixes(SceneProperties.ModelProperties, SceneProperties.CameraProperties, SceneProperties.BitmapProperties);

            Stopwatch.Restart();
            long prevMs = Stopwatch.ElapsedMilliseconds;

            ProjectVertices(matrixes.TransformMatrix);
            ProjectNormals(matrixes.WorldMatrix);

            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            Renderer.RenderModel(_mesh.TransformedModel, SceneProperties);

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");

            Debug.WriteLine("Render ended\n");
            Stopwatch.Stop();

            return Renderer.Bitmap;
        }
    }
}
