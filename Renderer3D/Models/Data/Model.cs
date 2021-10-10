using System.Numerics;

namespace Renderer3D.Models.Data
{
    public class Model
    {
        /// <summary>
        /// All vertices of the model (x, y, z, w)
        /// </summary>
        public Vector4[] Vertices { get; }

        /// <summary>
        /// All texture pieces of the model (x(u), y(v), z(w))
        /// </summary>
        public Vector3[] Textures { get; }

        /// <summary>
        /// Normal vectors of the model (x(i), y(j), z(k))
        /// </summary>
        public Vector3[] Normals { get; }

        public PolygonIndex[] Polygons { get; }

        public Model(Vector4[] vertices, Vector3[] textures, Vector3[] normals, PolygonIndex[] polygons)
        {
            (Vertices, Textures, Normals, Polygons) = (vertices, textures, normals, polygons);
        }

        public Model(int verticesLength, int texturesLength, int normalsLength, int polygonsLength)
        {
            Vertices = new Vector4[verticesLength];
            Textures = new Vector3[texturesLength];
            Normals = new Vector3[normalsLength];
            Polygons = new PolygonIndex[polygonsLength];
        }

        public PolygonValue GetPolygonValue(PolygonIndex p)
        {
            TriangleValue[] indices = new TriangleValue[p.TriangleIndices.Length];
            for (int i = 0; i < p.TriangleIndices.Length; i++)
            {
                indices[i] = new TriangleValue
                {
                    v0 = new VertexValue
                    {
                        Coordinates = Vertices[p.TriangleIndices[i].VertexOneIndex.Coordinates],
                        Normal = Normals[p.TriangleIndices[i].VertexOneIndex.Normal],
                        //Texture = Textures[p.TriangleIndices[i].VertexOneIndex.Texture]
                    },
                    v1 = new VertexValue
                    {
                        Coordinates = Vertices[p.TriangleIndices[i].VertexTwoIndex.Coordinates],
                        Normal = Normals[p.TriangleIndices[i].VertexTwoIndex.Normal],
                        //Texture = Textures[p.TriangleIndices[i].VertexTwoIndex.Texture]
                    },
                    v2 = new VertexValue
                    {
                        Coordinates = Vertices[p.TriangleIndices[i].VertexThreeIndex.Coordinates],
                        Normal = Normals[p.TriangleIndices[i].VertexThreeIndex.Normal],
                        //Texture = Textures[p.TriangleIndices[i].VertexThreeIndex.Texture]
                    },
                };
            }

            return new PolygonValue
            {
                TriangleValues = indices
            };
        }
    }
}
