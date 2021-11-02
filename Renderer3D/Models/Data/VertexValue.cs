using System.Numerics;

namespace Renderer3D.Models.Data
{
    public struct VertexValue
    {
        /// <summary>
        /// Original coordinates translated to screen space using translation matrices
        /// </summary>
        public Vector3 Coordinates;

        /// <summary>
        /// Normal vector of the vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Texture of the vertex
        /// </summary>
        public Vector3 Texture;

        public VertexValue InterpolateTo(VertexValue v, float alphaSplit)
        {
            return new VertexValue
            {
                Coordinates = Coordinates + (v.Coordinates - Coordinates) * alphaSplit,
                Normal = Normal + (v.Normal - Normal) * alphaSplit,
                Texture = Texture + (v.Texture - Texture) * alphaSplit
            };
        }
    }
}
