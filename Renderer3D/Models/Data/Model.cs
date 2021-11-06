using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System.Collections.Generic;

namespace Renderer3D.Models.Data
{
    public class Model
    {
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
                    Coordinates = meshProperties.Vertices[t.Vi1.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.Vi1.Normal],
                    Texture = meshProperties.Textures[t.Vi1.Texture]
                },
                v1 = new VertexValue
                {
                    Coordinates = meshProperties.Vertices[t.Vi2.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.Vi2.Normal],
                    Texture = meshProperties.Textures[t.Vi2.Texture]
                },
                v2 = new VertexValue
                {
                    Coordinates = meshProperties.Vertices[t.Vi3.Coordinates].ToV3(),
                    Normal = meshProperties.Normals[t.Vi3.Normal],
                    Texture = meshProperties.Textures[t.Vi3.Texture]
                }
            };
        }
    }
}
