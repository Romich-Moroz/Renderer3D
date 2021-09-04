namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents single vertex of the polygon
    /// </summary>
    public struct PolygonVertex
    {
        /// <summary>
        /// Index in the table of vertices. Also knows an v1
        /// </summary>
        public int VertexIndex;
        /// <summary>
        /// Index in the table of texture pieces. Also knows as vt1
        /// </summary>
        public int TextureIndex;
        /// <summary>
        /// Index in the table of normal vectors. Also knows as vn1
        /// </summary>
        public int NormalVectorIndex;
    }
}
