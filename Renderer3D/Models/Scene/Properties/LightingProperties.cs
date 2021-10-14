using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public class LightingProperties
    {
        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightSourcePosition { get; set; } = Vector3.Zero;
        public float LightSourceIntensity { get; set; } = 1;

        public float Ka { get; set; } = 0.1f;
        public Vector3 Ia { get; set; } = new Vector3(0xD4, 0XAF, 0x37);

        public float Kd { get; set; } = 1f;
        public Vector3 Id { get; set; } = new Vector3(0xD4, 0XAF, 0x37);

        public float Ks { get; set; } = 2f;
        public Vector3 Is { get; set; } = new Vector3(0xFF, 0xCF, 0x42);
        public float ShininessCoefficient { get; set; } = 512f;
    }
}
