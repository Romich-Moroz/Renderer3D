namespace Renderer3D.Models.Scene
{
    public struct RenderProperties
    {
        /// <summary>
        /// Turns on/off triangle render mode
        /// </summary>
        public bool TriangleMode { get; set; }

        public RenderProperties(bool triangleMode)
        {
            TriangleMode = triangleMode;
        }
    }
}
