using Renderer3D.Models.Data;
using Renderer3D.Models.Processing;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

            SceneProperties.ScreenProperties.Width = newWidth;
            SceneProperties.ScreenProperties.Height = newHeight;
            Renderer.Bitmap = SceneProperties.ScreenProperties.CreateFromProperties();
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
            SceneProperties.ScreenProperties.Width = width;
            SceneProperties.ScreenProperties.Height = height;
            ChangeMesh(mesh);
        }

        public void ResetState()
        {
            SceneProperties.ModelProperties.Reset();
            SceneProperties.RenderProperties.Reset();
            SceneProperties.CameraProperties.Reset();
            SceneProperties.CameraProperties.CenterCamera(_mesh.OriginalMeshProperties.Vertices);
            SceneProperties.LightingProperties.LightSourcePosition = SceneProperties.CameraProperties.CameraTarget + new Vector3(-5, 100, 100);

        }

        public void ChangeMesh(Mesh mesh)
        {
            _mesh = mesh;
            ResetState();
            Renderer.Bitmap = SceneProperties.ScreenProperties.CreateFromProperties();
        }

        /// <summary>
        /// Renders the loaded model into bitmap
        /// </summary>
        /// <returns>Rendered bitmap</returns>
        public BitmapSource GetRenderedScene(out long renderTime)
        {

            int polyCount = _mesh.Models.Sum(m => m.Polygons.Count);
            Debug.WriteLine($"Render started. Rendering {polyCount} polygons");
            Renderer.Clear();
            TransformMatrixes matrixes = Projection.GetTransformMatrixes(SceneProperties.ModelProperties, SceneProperties.CameraProperties, SceneProperties.ScreenProperties);

            Stopwatch.Restart();
            renderTime = Stopwatch.ElapsedMilliseconds;
            long prevMs = Stopwatch.ElapsedMilliseconds;

            Projection.ProjectMesh(matrixes, _mesh, SceneProperties.RenderProperties.RenderMode);

            Debug.WriteLine($"Vertex calculation time: {Stopwatch.ElapsedMilliseconds - prevMs}");
            prevMs = Stopwatch.ElapsedMilliseconds;

            for (int i = 0; i < _mesh.Models.Count; i++)
            {
                Renderer.RenderModel(_mesh.TransformedMeshProperties, _mesh.Models[i], SceneProperties);
            }

            Debug.WriteLine($"Render time: {Stopwatch.ElapsedMilliseconds - prevMs}");

            Debug.WriteLine("Render ended\n");
            renderTime = Stopwatch.ElapsedMilliseconds - renderTime;
            Stopwatch.Stop();

            return Renderer.Bitmap;
        }
    }
}
