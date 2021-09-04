using Renderer3D.Models.Data;
using System.Numerics;

namespace Renderer3D.Models.Parser
{
    /// <summary>
    /// Represents model parsed from .obj file
    /// </summary>
    public class ObjectModel
    {
        /// <summary>
        /// All vertices of the model (x, y, z, w)
        /// </summary>
        public Vector4[] Vertices { get; }
        /// <summary>
        /// All texture pieces of the model (x(u), y(v), z(w))
        /// </summary>
        public Vector3[] TexturePieces { get; }
        /// <summary>
        /// Normal vectors of the model (x(i), y(j), z(k))
        /// </summary>
        public Vector3[] NormalVectors { get; }
        /// <summary>
        /// Polygons of the model (v1, v2, v3, v4...)
        /// </summary>
        public Polygon[] Polygons { get; }

        public ObjectModel(Vector4[] vertices, Vector3[] texturePieces, Vector3[] normalVectors, Polygon[] polygons) =>
            (Vertices, TexturePieces, NormalVectors, Polygons) = (vertices, texturePieces, normalVectors, polygons);
    }
}
