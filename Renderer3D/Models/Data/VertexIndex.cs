namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents single vertex of the polygon
    /// </summary>
    public struct VertexIndex
    {
        /// <summary>
        /// Index in the table of vertices. Also knows an v1
        /// </summary>
        public int Vertex;
        /// <summary>
        /// Index in the table of texture pieces. Also knows as vt1
        /// </summary>
        public int Texture;
        /// <summary>
        /// Index in the table of normal vectors. Also knows as vn1
        /// </summary>
        public int Normal;
    }
}
