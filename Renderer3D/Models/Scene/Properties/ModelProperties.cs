using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public class ModelProperties
    {
        /// <summary>
        /// Scale of the model
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Offset of the model in world coordinates
        /// </summary>
        public Vector3 Offset { get; set; } = Vector3.Zero;

        /// <summary>
        /// Rotation of the model around X,Y,Z axises
        /// </summary>
        public Vector3 Rotation { get; set; } = Vector3.Zero;

        public void Reset()
        {
            Scale = Vector3.One;
            Offset = Vector3.Zero;
            Rotation = Vector3.Zero;
        }
    }
}
