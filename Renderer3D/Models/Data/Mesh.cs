using System;

namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents mesh parsed from .obj file
    /// </summary>
    public class Mesh
    {
        public Model OriginalModel { get; set; }
        public Model TransformedModel { get; set; }

        public Mesh(Model model)
        {
            OriginalModel = model;
            TransformedModel = new Model(OriginalModel.Vertices.Length, OriginalModel.Textures.Length, OriginalModel.Normals.Length, OriginalModel.Polygons.Length);
            Array.Copy(OriginalModel.Vertices, TransformedModel.Vertices, OriginalModel.Vertices.Length);
            Array.Copy(OriginalModel.Polygons, TransformedModel.Polygons, OriginalModel.Polygons.Length);
            Array.Copy(OriginalModel.Textures, TransformedModel.Textures, OriginalModel.Textures.Length);
            Array.Copy(OriginalModel.Normals, TransformedModel.Normals, OriginalModel.Normals.Length);
        }
    }
}
