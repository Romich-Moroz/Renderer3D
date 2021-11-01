using System.Numerics;

namespace Renderer3D.Models.Data.Properties
{
    public class LightingProperties
    {
        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightSourcePosition { get; set; } = Vector3.Zero;

        /// <summary>
        /// Intensity of the light source
        /// </summary>
        public float LightSourceIntensity { get; set; } = 1;
    }
}
