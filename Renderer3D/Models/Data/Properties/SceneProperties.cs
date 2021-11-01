using Renderer3D.Models.Data.Properties;

namespace Renderer3D.Models.Scene
{
    public class SceneProperties
    {
        public ScreenProperties ScreenProperties { get; set; } = new ScreenProperties();
        public ModelProperties ModelProperties { get; set; } = new ModelProperties();
        public CameraProperties CameraProperties { get; set; } = new CameraProperties();
        public LightingProperties LightingProperties { get; set; } = new LightingProperties();
        public RenderProperties RenderProperties { get; set; } = new RenderProperties();
    }
}
