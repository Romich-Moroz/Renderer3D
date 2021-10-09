using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public struct LightingProperties
    {
        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightSourcePosition { get; set; }

        public LightingProperties(Vector3 lightSourcePosition)
        {
            LightSourcePosition = lightSourcePosition;
        }
    }
}
