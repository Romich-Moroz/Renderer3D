using System;
using System.Windows;

namespace Renderer3D.Models.Data
{
    public struct DdaStruct
    {
        /// <summary>
        /// Total length of the line
        /// </summary>
        public readonly float LineLength;

        /// <summary>
        /// Step offset for X axis
        /// </summary>
        public readonly float DeltaX;

        /// <summary>
        /// Step offset for Y axis
        /// </summary>
        public readonly float DeltaY;

        public DdaStruct(float lineLength, float deltaX, float deltaY)
        {
            (LineLength, DeltaX, DeltaY) = (lineLength, deltaX, deltaY);
        }

        /// <summary>
        /// Creates Dda struct from two points
        /// </summary>
        /// <param name="v1">Start of the line</param>
        /// <param name="v2">End of the line</param>
        /// <returns></returns>
        public static DdaStruct FromPoints(Point v1, Point v2)
        {
            double x2x1 = v2.X - v1.X;
            double y2y1 = v2.Y - v1.Y;
            double l = Math.Abs(x2x1) > Math.Abs(y2y1) ? Math.Abs(x2x1) : Math.Abs(y2y1);
            double xDelta = x2x1 / l;
            double yDelta = y2y1 / l;

            return new DdaStruct((float)l, (float)xDelta, (float)yDelta);
        }
    }
}
