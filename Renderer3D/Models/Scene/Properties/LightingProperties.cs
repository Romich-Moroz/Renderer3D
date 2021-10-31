using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public class LightingProperties
    {
        private float _AmbientIntensityCoefficient;
        private Vector3 _AmbientColor;
        private Vector3 _AmbientIntensity;

        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightSourcePosition { get; set; } = Vector3.Zero;
        public float LightSourceIntensity { get; set; } = 1;

        public float Ka
        {
            get => _AmbientIntensityCoefficient;
            set
            {
                _AmbientIntensityCoefficient = value;
                _AmbientColor = _AmbientIntensity * value;
            }
        }
        public Vector3 Ia
        {
            get => _AmbientColor;
            set
            {
                _AmbientColor = value;
                _AmbientColor = _AmbientIntensity * value;
            }
        }

        public Vector3 AmbientIntensity => _AmbientIntensity;

        public float Kd { get; set; } = 1f;
        public Vector3 Id { get; set; } = new Vector3(0xD4, 0XAF, 0x37);

        public float Ks { get; set; } = 2f;
        public Vector3 Is { get; set; } = new Vector3(0xFF, 0xCF, 0x42);
        public int ShininessCoefficient { get; set; } = 512;

        public LightingProperties()
        {
            Ia = new Vector3(0xD4, 0XAF, 0x37);
            Ka = 0.1f;
        }
    }
}
