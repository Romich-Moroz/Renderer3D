using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public class RenderProperties
    {
        private Vector3 _RenderFallbackColor;
        private int _RenderFallbackColorInt;

        public RenderMode RenderMode { get; set; } = RenderMode.MeshOnly;
        public float Sensitivity { get; set; } = (float)System.Math.PI / 360;
        public float MoveStep { get; set; } = 0.25f;
        public float ScaleStep { get; set; } = 1f;
        public Vector3 RenderFallbackColor
        {
            get => _RenderFallbackColor;
            set
            {
                _RenderFallbackColor = value;
                _RenderFallbackColorInt = value.ToColorInt();
            }
        }
        public int RenderFallbackColorInt => _RenderFallbackColorInt;

        public RenderProperties()
        {
            RenderFallbackColor = new Vector3(0x80, 0x80, 0x80);
        }

        public void Reset()
        {
            RenderMode = RenderMode.MeshOnly;
        }
    }
}
