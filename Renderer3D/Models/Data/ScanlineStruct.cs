using Renderer3D.Models.Processing;
using System.Numerics;

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

        public ScanlineStruct(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd)
        {
            Gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            Gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            StartX = (int)Calculation.Interpolate(pa.X, pb.X, Gradient1);
            EndX = (int)Calculation.Interpolate(pc.X, pd.X, Gradient2);

            Z1 = Calculation.Interpolate(pa.Z, pb.Z, Gradient1);
            Z2 = Calculation.Interpolate(pc.Z, pd.Z, Gradient2);

            Y = y;
        }
    }
}
