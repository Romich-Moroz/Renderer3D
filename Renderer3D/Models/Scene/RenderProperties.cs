using Renderer3D.Models.Data;

namespace Renderer3D.Models.Scene
{
    public struct RenderProperties
    {
        /// <summary>
        /// Turns on/off triangle render mode
        /// </summary>
        public RenderMode RenderMode { get; set; }

        /// <summary>
        /// Parameter for normal interpolation (0;1)
        /// </summary>
        public float InterpolationParameter { get; set; }

        public RenderProperties(RenderMode renderMode)
        {
            RenderMode = renderMode;
            InterpolationParameter = 0.01f;
        }
    }
}
