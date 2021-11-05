using System.Collections.Generic;
using System.Numerics;

namespace Renderer3D.Models.Data.Properties
{
    public class MeshProperties
    {
        /// <summary>
        /// All vertices of the mesh (x, y, z, w)
        /// </summary>
        public List<Vector4> Vertices { get; set; } = new List<Vector4>();

        /// <summary>
        /// All texture pieces of the mesh (x(u), y(v), z(w))
        /// </summary>
        public List<Vector3> Textures { get; set; } = new List<Vector3>();

        /// <summary>
        /// Normal vectors of the mesh (x(i), y(j), z(k))
        /// </summary>
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
    }
}
