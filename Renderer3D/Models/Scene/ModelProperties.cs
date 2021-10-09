using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public struct ModelProperties
    {
        /// <summary>
        /// Scale of the model
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Offset of the model in world coordinates
        /// </summary>
        public Vector3 Offset { get; set; }

        /// <summary>
        /// Rotation of the model around X,Y,Z axises
        /// </summary>
        public Vector3 Rotation { get; set; }

        public ModelProperties(Vector3 scale, Vector3 offset, Vector3 rotation)
        {
            (Scale, Offset, Rotation) = (scale, offset, rotation);
        }
    }
}
