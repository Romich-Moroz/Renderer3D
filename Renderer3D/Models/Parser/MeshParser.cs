using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            if (values[0] == "v" && values.Length >= 4)
            {
                return new Vector4
                {
                    X = float.Parse(values[1]),
                    Y = float.Parse(values[2]),
                    Z = float.Parse(values[3]),
                    W = values.Length == 5 ? float.Parse(values[4]) : 1
                };
            }
            throw new InvalidOperationException("Supplied line values are invalid");
        }

        private static Vector3 ParseVertexTexture(string[] values)
        {
            if (values[0] == "vt" && values.Length >= 2)
            {
                return new Vector3
                {
                    X = float.Parse(values[1]),
                    Y = values.Length >= 3 ? float.Parse(values[2]) : 0,
                    Z = values.Length == 4 ? float.Parse(values[3]) : 0
                };
            }
            throw new InvalidOperationException("Supplied line values are invalid");
        }

        private static Vector3 ParseNormalVector(string[] values)
        {
            if (values[0] == "vn" && values.Length >= 4)
            {
                return new Vector3
                {
                    X = float.Parse(values[1]),
                    Y = float.Parse(values[2]),
                    Z = float.Parse(values[3])
                };
            }
            throw new InvalidOperationException("Supplied line values are invalid");
        }

        private static Vector3 ParseVector3(string[] values)
        {
            if (values.Length >= 4)
            {
                return new Vector3
                {
                    X = float.Parse(values[1]),
                    Y = float.Parse(values[2]),
                    Z = float.Parse(values[3])
                };
            }
            throw new InvalidOperationException("Supplied line values are invalid");
        }


        private static TriangleIndex[] SplitPolygon(List<VertexIndex> polygonVertices)
        {
            TriangleIndex[] result = new TriangleIndex[polygonVertices.Count - 2];
            if (polygonVertices.Count == 3)
            {
                result[0] = new TriangleIndex
                {
                    VertexOneIndex = polygonVertices[0],
                    VertexTwoIndex = polygonVertices[1],
                    VertexThreeIndex = polygonVertices[2],
                };
            }
            else
            {
                for (int i = 2; i < polygonVertices.Count; i++)
                {
                    result[i - 2] = new TriangleIndex
                    {
                        VertexOneIndex = polygonVertices[0],
                        VertexTwoIndex = polygonVertices[i - 1],
                        VertexThreeIndex = polygonVertices[i]
                    };
                }
            }
            return result;
        }

        private static PolygonIndex ParsePolygon(string[] values, int vertexCount)
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
                    Coordinates = verticeIndex,
                    Texture = textureIndex,
                    Normal = normalVectorIndex
                });
            }

            return new PolygonIndex
            {
                TriangleIndices = SplitPolygon(polygonVertices)
            };
        }

        private static string[] GetStringEntries(string line)
        {
            string l = line.Trim();
            if (l.Length != 0 && l[0] != '#')
            {
                return l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        private static (List<Model>, MeshProperties) ParseModels(string filepath)
        {
            using StreamReader file = new StreamReader(filepath);

            MeshProperties meshProperties = new MeshProperties();
            List<Model> models = new List<Model>();

            string line;
            int lineCounter = 0;
            while ((line = file.ReadLine()) != null)
            {
                lineCounter++;
                string[] stringValues = GetStringEntries(line);
                if (stringValues.Length == 0)
                {
                    continue;
                }
                switch (stringValues[0])
                {
                    case "v":
                        meshProperties.Vertices.Add(ParseVertex(stringValues));
                        break;
                    case "vt":
                        meshProperties.Textures.Add(ParseVertexTexture(stringValues));
                        break;
                    case "vn":
                        meshProperties.Normals.Add(ParseNormalVector(stringValues));
                        break;
                    case "f":
                        models[^1].Polygons.Add(ParsePolygon(stringValues, meshProperties.Vertices.Count));
                        break;
                    case "usemtl":
                        models.Add(new Model());
                        models[^1].MaterialKey = string.Join(' ', stringValues.Skip(1));
                        break;
                    default:
                        Debug.WriteLine($"Ignored unsupported format on line №{lineCounter}, Line content: {line}");
                        continue;
                }
            }

            return (models, meshProperties);
        }

        private static Dictionary<string, MaterialProperties> ParseMaterialsFile(string filepath)
        {
            using StreamReader file = new StreamReader(filepath);

            Dictionary<string, MaterialProperties> result = new Dictionary<string, MaterialProperties>();

            string line;
            int lineCounter = 0;
            MaterialProperties matProperties = new MaterialProperties();
            while ((line = file.ReadLine()) != null)
            {
                lineCounter++;
                string[] stringValues = GetStringEntries(line);

                if (stringValues.Length == 0)
                {
                    continue;
                }

                switch (stringValues[0])
                {
                    case "newmtl":
                        matProperties = new MaterialProperties();
                        result.Add(stringValues[1], matProperties);
                        break;
                    case "Ns":
                        matProperties.SpecularHighlight = float.Parse(stringValues[1]);
                        break;
                    case "Ka":
                        matProperties.AmbientColorIntensity = ParseVector3(stringValues);
                        break;
                    case "Kd":
                        matProperties.DiffuseColorIntensity = ParseVector3(stringValues);
                        break;
                    case "Ks":
                        matProperties.SpecularColorIntensity = ParseVector3(stringValues);
                        break;
                    case "Ni":
                        matProperties.OpticalDensity = float.Parse(stringValues[1]);
                        break;
                    case "d":
                        matProperties.Dissolve = float.Parse(stringValues[1]);
                        break;
                    case "illum":
                        matProperties.IlluminationModel = (IlluminationModel)int.Parse(stringValues[1]);
                        break;
                    case "map_Kd":
                        matProperties.ColorTextureFileName = stringValues[1];
                        break;
                    case "map_Ks":
                        matProperties.ColorSpecularFileName = stringValues[1];
                        break;
                    default:
                        Debug.WriteLine($"Ignored unsupported format on line №{lineCounter}, Line content: {line}");
                        continue;
                }
            }

            return result;
        }

        private static string GetMaterialsPath(string filepath)
        {
            using StreamReader file = new StreamReader(filepath);
            string line;
            string[] entries = new string[0];
            while ((line = file.ReadLine()) != null)
            {
                entries = GetStringEntries(line);
                if (entries.Length != 0 && entries[0] == "mtllib")
                {
                    break;
                }
            }

            if (entries.Length < 2)
            {
                throw new InvalidOperationException($"File {filepath} does not contain materials path");
            };

            return string.Join(' ', entries.Skip(1));
        }

        /// <summary>
        /// Parse any .obj model file
        /// </summary>
        /// <param name="filepath">File path to .obj file</param>
        /// <returns>Parsed model object</returns>
        public static Mesh Parse(string filepath)
        {
            string materialsPath = GetMaterialsPath(filepath);

            List<Model> models;
            MeshProperties meshProperties;
            (models, meshProperties) = ParseModels(filepath);

            string dir = Path.GetDirectoryName(filepath);
            string path = Path.Combine(dir, materialsPath);
            meshProperties.MaterialProperties = ParseMaterialsFile(path);

            foreach (Model model in models)
            {
                model.MaterialProperties = meshProperties.MaterialProperties[model.MaterialKey];
            }

            return new Mesh(models, meshProperties);
        }
    }
}
