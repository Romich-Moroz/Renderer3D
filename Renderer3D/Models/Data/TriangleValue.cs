namespace Renderer3D.Models.Data
{
    public struct TriangleValue
    {
        public VertexValue v0, v1, v2;

        public float GetAlphaSplit()
        {
            return (v1.Coordinates.Y - v0.Coordinates.Y) / (v2.Coordinates.Y - v0.Coordinates.Y);
        }
    }
}
