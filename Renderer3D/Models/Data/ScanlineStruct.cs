using Renderer3D.Models.Processing;

namespace Renderer3D.Models.Data
{
    public struct ScanlineStruct
    {
        public int Y { get; set; }
        public float Gradient1 { get; set; }
        public float Gradient2 { get; set; }

        public int StartX { get; set; }
        public int EndX { get; set; }
        public float Z1 { get; set; }
        public float Z2 { get; set; }

        public TriangleValue Triangle { get; set; }

        public ScanlineStruct(int y, TriangleValue t)
        {
            double dP1P2, dP1P3;
            (dP1P2, dP1P3) = Calculation.GetInverseSlopes(t.v0.Coordinates, t.v1.Coordinates, t.v2.Coordinates);

            VertexValue pa, pb, pc, pd;
            if (y < t.v1.Coordinates.Y)
            {
                pa = t.v0;
                pb = dP1P2 > dP1P3 ? t.v2 : t.v1;
                pc = t.v0;
                pd = dP1P2 > dP1P3 ? t.v1 : t.v2;
            }
            else
            {
                pa = dP1P2 > dP1P3 ? t.v0 : t.v1;
                pb = t.v2;
                pc = dP1P2 > dP1P3 ? t.v1 : t.v0;
                pd = t.v2;
            }

            Gradient1 = pa.Coordinates.Y != pb.Coordinates.Y ? (y - pa.Coordinates.Y) / (pb.Coordinates.Y - pa.Coordinates.Y) : 1;
            Gradient2 = pc.Coordinates.Y != pd.Coordinates.Y ? (y - pc.Coordinates.Y) / (pd.Coordinates.Y - pc.Coordinates.Y) : 1;

            StartX = (int)Calculation.Interpolate(pa.Coordinates.X, pb.Coordinates.X, Gradient1);
            EndX = (int)Calculation.Interpolate(pc.Coordinates.X, pd.Coordinates.X, Gradient2);

            Z1 = Calculation.Interpolate(pa.Coordinates.Z, pb.Coordinates.Z, Gradient1);
            Z2 = Calculation.Interpolate(pc.Coordinates.Z, pd.Coordinates.Z, Gradient2);

            (Y, Triangle) = (y, t);
        }
    }
}
