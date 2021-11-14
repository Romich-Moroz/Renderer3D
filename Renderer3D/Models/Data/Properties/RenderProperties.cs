using Renderer3D.Models.Extensions;
using System.Numerics;

namespace Renderer3D.Models.Data.Properties
{
    public class RenderProperties
    {
        public ShadingMode ShadingMode { get; set; } = ShadingMode.None;
        public float Sensitivity { get; set; } = (float)System.Math.PI / 360;
        public float ScaleStep { get; set; } = 1f;
        public int MeshLineColor => new Vector3(0x80, 0x80, 0x80).ToInt();

        public void Reset()
        {
            ShadingMode = ShadingMode.None;
        }
    }
}
