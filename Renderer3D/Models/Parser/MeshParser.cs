﻿using Renderer3D.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Renderer3D.Models.Parser
{
    /// <summary>
    /// Parser for .obj files
    /// </summary>
    public static class MeshParser
    {
        private static Vector4 ParseVertex(string[] values)
        {
            if (values[0] != "v" || values.Length < 4)
            {
                throw new InvalidOperationException("Supplied line values are invalid");
            }

            return new Vector4
            {
                X = float.Parse(values[1]),
                Y = float.Parse(values[2]),
                Z = float.Parse(values[3]),
                W = values.Length == 5 ? float.Parse(values[4]) : 1
            };
        }

        private static Vector3 ParseVertexTexture(string[] values)
        {
            if (values[0] != "vt" || values.Length < 2)
            {
                throw new InvalidOperationException("Supplied line values are invalid");
            }

            return new Vector3
            {
                X = float.Parse(values[1]),
                Y = values.Length >= 3 ? float.Parse(values[2]) : 0,
                Z = values.Length == 4 ? float.Parse(values[3]) : 0
            };
        }

        private static Vector3 ParseNormalVector(string[] values)
        {
            if (values[0] != "vn" || values.Length < 4)
            {
                throw new InvalidOperationException("Supplied line values are invalid");
            }

            return new Vector3
            {
                X = float.Parse(values[1]),
                Y = float.Parse(values[2]),
                Z = float.Parse(values[3])
            };
        }
        private static TriangleIndex[] SplitPolygon(List<VertexIndex> vertices)
        {
            TriangleIndex[] result = new TriangleIndex[vertices.Count - 2];
            if (vertices.Count == 3)
            {
                result[0] = new TriangleIndex
                {
                    IndexP1 = vertices[0].Vertex,
                    IndexP2 = vertices[1].Vertex,
                    IndexP3 = vertices[2].Vertex
                };
            }
            else
            {
                for (int i = 2; i < vertices.Count; i++)
                {
                    result[i - 2] = new TriangleIndex
                    {
                        IndexP1 = vertices[0].Vertex,
                        IndexP2 = vertices[i - 1].Vertex,
                        IndexP3 = vertices[i].Vertex
                    };
                }
            }
            return result;
        }

        private static Polygon ParsePolygon(string[] values, int vertexCount)
        {
            if (values[0] != "f" || values.Length < 4)
            {
                throw new InvalidOperationException("Supplied line values are invalid");
            }

            List<VertexIndex> polygonVertices = new List<VertexIndex>();

            for (int i = 1; i < values.Length; i++)
            {
                string[] polygonValues = values[i].Split('/');

                int verticeIndex = int.Parse(polygonValues[0]);
                verticeIndex = verticeIndex > 0 ? verticeIndex - 1 : vertexCount + verticeIndex;

                int normalVectorIndex = polygonValues.Length == 3 ? int.Parse(polygonValues[2]) - 1 : -1;

                int textureIndex = -1;
                if (polygonValues.Length >= 2)
                {
                    textureIndex = string.IsNullOrEmpty(polygonValues[1]) ? -1 : int.Parse(polygonValues[1]) - 1;
                }

                polygonVertices.Add(new VertexIndex
                {
                    Vertex = verticeIndex,
                    Texture = textureIndex,
                    Normal = normalVectorIndex
                });
            }

            return new Polygon
            {
                Vertices = polygonVertices.ToArray(),
                TriangleIndexes = SplitPolygon(polygonVertices)
            };
        }

        /// <summary>
        /// Parse any .obj model file
        /// </summary>
        /// <param name="objPath">File path to .obj file</param>
        /// <returns>Parsed model object</returns>
        public static Mesh Parse(string objPath)
        {
            using StreamReader file = new StreamReader(objPath);
            string line;
            int lineCounter = 0;

            List<Vector4> vertexes = new List<Vector4>();
            List<Vector3> vertexTextures = new List<Vector3>();
            List<Vector3> normalVectors = new List<Vector3>();
            List<Polygon> polygons = new List<Polygon>();

            while ((line = file.ReadLine()) != null)
            {
                lineCounter++;
                line = line.Trim();

                //Comment or empty
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                string[] stringValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (stringValues[0])
                {
                    case "v":
                        vertexes.Add(ParseVertex(stringValues));
                        break;
                    case "vt":
                        vertexTextures.Add(ParseVertexTexture(stringValues));
                        break;
                    case "vn":
                        normalVectors.Add(ParseNormalVector(stringValues));
                        break;
                    case "f":
                        polygons.Add(ParsePolygon(stringValues, vertexes.Count));
                        break;
                    default:
                        Console.WriteLine($"Ignorred unsupported format on line №{lineCounter}, Line content: {line}");
                        continue;
                }
            }

            return new Mesh(vertexes.ToArray(), vertexTextures.ToArray(), normalVectors.ToArray(), polygons.ToArray());
        }
    }
}