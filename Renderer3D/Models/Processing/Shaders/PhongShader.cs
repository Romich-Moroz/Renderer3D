using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System;
using System.Numerics;

namespace Renderer3D.Models.Processing.Shaders
{
    public static class PhongShader
    {
        public static int GetPixelColor(TriangleValue t, MaterialProperties materialProperties, LightingProperties lightingProperties, CameraProperties cameraProperties, Vector3 point)
        {
            Vector3 viewVector = cameraProperties.CameraPosition - point;
            Vector3 lightVector = lightingProperties.LightSourcePosition - point;
            Vector3 hVector = Vector3.Normalize(viewVector + lightVector);

            Vector3 bary = Calculation.GetFastBarycentricCoordinates
            (
                t.v0.Coordinates,
                t.v1.Coordinates,
                t.v2.Coordinates,
                point
            );

            Vector3 n = Vector3.Normalize(t.v0.Normal * bary.X + t.v1.Normal * bary.Y + t.v2.Normal * bary.Z);

            Vector3 ambient = materialProperties.AmbientColorIntensity;
            Vector3 diffuse = Calculation.GetDiffuseLightingColor(materialProperties, lightVector, n);
            Vector3 reflection = Calculation.GetSpecularColor(materialProperties, hVector, n);

            Vector3 intensity = ambient + diffuse + reflection;
            intensity.X = Math.Min(intensity.X, 255);
            intensity.Y = Math.Min(intensity.Y, 255);
            intensity.Z = Math.Min(intensity.Z, 255);

            return intensity.ToInt();
        }
    }
}
