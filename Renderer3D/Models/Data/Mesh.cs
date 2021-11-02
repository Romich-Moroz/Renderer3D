using Renderer3D.Models.Data.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents mesh parsed from .obj file
    /// </summary>
    public class Mesh
    {
        public MeshProperties OriginalMeshProperties { get; set; }
        public MeshProperties TransformedMeshProperties { get; set; }
        public List<Model> Models { get; set; }

        public Mesh(List<Model> models, MeshProperties meshProperties)
        {
            Models = models;
            OriginalMeshProperties = meshProperties;
            TransformedMeshProperties = new MeshProperties
            {
                Vertices = new List<Vector4>(OriginalMeshProperties.Vertices),
                Textures = new List<Vector3>(OriginalMeshProperties.Textures),
                Normals = new List<Vector3>(OriginalMeshProperties.Normals)
            };
        }
    }
}
