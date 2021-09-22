namespace Renderer3D.Models.Data
{
    /// <summary>
    /// Represents one polygon of the model
    /// </summary>
    public struct Polygon
    {
        /// <summary>
        /// Vertices of the polygon
        /// </summary>
        public VertexIndex[] Vertices;

        /// <summary>
        /// Contains indexes of vertices for each triangle
        /// </summary>
        public TriangleIndex[] TriangleIndexes;
    }
}
