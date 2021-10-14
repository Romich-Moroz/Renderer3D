namespace Renderer3D.Models.Scene
{
    public class SceneProperties
    {
        public BitmapProperties BitmapProperties { get; set; } = new BitmapProperties();
        public ModelProperties ModelProperties { get; set; } = new ModelProperties();
        public CameraProperties CameraProperties { get; set; } = new CameraProperties();
        public LightingProperties LightingProperties { get; set; } = new LightingProperties();
        public RenderProperties RenderProperties { get; set; } = new RenderProperties();
    }
}
