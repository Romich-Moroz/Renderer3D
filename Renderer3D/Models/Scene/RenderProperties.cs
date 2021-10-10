using Renderer3D.Models.Data;

namespace Renderer3D.Models.Scene
{
    public struct RenderProperties
    {
        /// <summary>
        /// Turns on/off triangle render mode
        /// </summary>
        public RenderMode RenderMode { get; set; }

        public RenderProperties(RenderMode renderMode)
        {
            RenderMode = renderMode;
        }
    }
}
