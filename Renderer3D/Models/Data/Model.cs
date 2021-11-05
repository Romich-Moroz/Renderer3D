using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System.Collections.Generic;

namespace Renderer3D.Models.Data
{
    public class Model
    {
        public string ModelName { get; }
        public string MaterialKey { get; set; }
        public MaterialProperties MaterialProperties { get; set; }
        public List<PolygonIndex> Polygons { get; } = new List<PolygonIndex>();

        public PolygonValue GetPolygonValue(PolygonIndex p, MeshProperties meshProperties)
        {
            TriangleValue[] indices = new TriangleValue[p.TriangleIndices.Length];
            for (int i = 0; i < p.TriangleIndices.Length; i++)
            {
                indices[i] = GetTriangleValue(p.TriangleIndices[i], meshProperties);
            }

            return new PolygonValue
            {
                TriangleValues = indices
            };
        }

        public TriangleValue GetTriangleValue(TriangleIndex t, MeshProperties meshProperties)
        {
            return new TriangleValue
            {
                v0 = new VertexValue
                {
                    Coordinates = meshProperties.Vertices[t.VertexOneIndex.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.VertexOneIndex.Normal],
                    Texture = meshProperties.Textures[t.VertexOneIndex.Texture]
                },
                v1 = new VertexValue
                {
                    Coordinates = meshProperties.Vertices[t.VertexTwoIndex.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.VertexTwoIndex.Normal],
                    Texture = meshProperties.Textures[t.VertexTwoIndex.Texture]
                },
                v2 = new VertexValue
                {
                    Coordinates = meshProperties.Vertices[t.VertexThreeIndex.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.VertexThreeIndex.Normal],
                    Texture = meshProperties.Textures[t.VertexThreeIndex.Texture]
                }
            };
        }
    }
}
