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
                indices[i] = new TriangleValue
                {
                    v0 = new VertexValue
                    {
                        Coordinates = meshProperties.Vertices[p.TriangleIndices[i].VertexOneIndex.Coordinates].ToV3(),
                        Normal = meshProperties.Normals[p.TriangleIndices[i].VertexOneIndex.Normal],
                        //Texture = Textures[p.TriangleIndices[i].VertexOneIndex.Texture]
                    },
                    v1 = new VertexValue
                    {
                        Coordinates = meshProperties.Vertices[p.TriangleIndices[i].VertexTwoIndex.Coordinates].ToV3(),
                        Normal = meshProperties.Normals[p.TriangleIndices[i].VertexTwoIndex.Normal],
                        //Texture = Textures[p.TriangleIndices[i].VertexTwoIndex.Texture]
                    },
                    v2 = new VertexValue
                    {
                        Coordinates = meshProperties.Vertices[p.TriangleIndices[i].VertexThreeIndex.Coordinates].ToV3(),
                        Normal = meshProperties.Normals[p.TriangleIndices[i].VertexThreeIndex.Normal],
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
