using System.Numerics;

namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents model parsed from .obj file
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// All vertices of the model (x, y, z, w)
        /// </summary>
        public Vector4[] OriginalVertices { get; }

        public Vector3[] TransformedVertices { get; }
        /// <summary>
        /// All texture pieces of the model (x(u), y(v), z(w))
        /// </summary>
        public Vector3[] OriginalTexturePieces { get; }
        /// <summary>
        /// Normal vectors of the model (x(i), y(j), z(k))
        /// </summary>
        public Vector3[] OriginalNormalVectors { get; }

        public Vector3[] TransformedNormalVectors { get; set; }

        /// <summary>
        /// Polygons of the model (v1, v2, v3, v4...)
        /// </summary>
        public Polygon[] Polygons { get; }

        public Mesh(Vector4[] vertices, Vector3[] texturePieces, Vector3[] normalVectors, Polygon[] polygons)
        {
            (OriginalVertices, OriginalTexturePieces, OriginalNormalVectors, Polygons, TransformedVertices, TransformedNormalVectors) =
                (vertices, texturePieces, normalVectors, polygons, new Vector3[vertices.Length], new Vector3[normalVectors.Length]);
        }
    }
}
