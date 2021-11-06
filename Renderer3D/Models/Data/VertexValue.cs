using System.Numerics;

namespace Renderer3D.Models.Data
{
    public struct VertexValue
    {
        /// <summary>
        /// Original coordinates translated to screen space using translation matrices
        /// </summary>
        public Vector4 Coordinates;

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

        public static VertexValue operator -(VertexValue v1, VertexValue v2)
        {
            return new VertexValue
            {
                Coordinates = v1.Coordinates - v2.Coordinates,
                Normal = v1.Normal - v2.Normal,
                Texture = v1.Texture - v2.Texture
            };
        }

        public static VertexValue operator +(VertexValue v1, VertexValue v2)
        {
            return new VertexValue
            {
                Coordinates = v1.Coordinates + v2.Coordinates,
                Normal = v1.Normal + v2.Normal,
                Texture = v1.Texture + v2.Texture
            };
        }

        public static VertexValue operator *(VertexValue v1, float v)
        {
            return new VertexValue
            {
                Coordinates = v1.Coordinates * v,
                Normal = v1.Normal * v,
                Texture = v1.Texture * v
            };
        }

        public static VertexValue operator /(VertexValue v1, float v)
        {
            return new VertexValue
            {
                Coordinates = v1.Coordinates / v,
                Normal = v1.Normal / v,
                Texture = v1.Texture / v
            };
        }

    }
}
