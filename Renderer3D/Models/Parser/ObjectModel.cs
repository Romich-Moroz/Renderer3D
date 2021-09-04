using Renderer3D.Models.Data;
using System.Numerics;

namespace Renderer3D.Models.Parser
{
    public class ObjectModel
    {
        public Vector4[] Vertexes { get; }
        public Vector3[] VertexTextures { get; }
        public Vector3[] NormalVectors { get; }
        public Polygon[] Polygons { get; }

        public ObjectModel(Vector4[] vertexes, Vector3[] vertexTextures, Vector3[] normalVectors, Polygon[] polygons)
        {
            (Vertexes, VertexTextures, NormalVectors, Polygons) = (vertexes, vertexTextures, normalVectors, polygons);
        }
    }
}
