using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System.Numerics;

namespace Renderer3D.Models.Processing.Shaders
{
    public static class FlatShader
    {
        public static int GetFaceColor(Vector3 fallbackColor, float ndotl)
        {
            return (fallbackColor * ndotl).ToInt();
        }

        public static float GetNdotL(TriangleValue triangle, LightingProperties lightingProperties)
        {
            Vector3 vnFace = (triangle.v0.Normal + triangle.v1.Normal + triangle.v2.Normal) / 3;
            Vector3 centerPoint = (triangle.v0.Coordinates + triangle.v1.Coordinates + triangle.v2.Coordinates) / 3;
            return Calculation.ComputeNDotL(lightingProperties.LightSourcePosition - centerPoint, vnFace) * lightingProperties.LightSourceIntensity;
        }
    }
}
