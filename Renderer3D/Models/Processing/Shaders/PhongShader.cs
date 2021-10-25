using Renderer3D.Models.Data;
using Renderer3D.Models.Extensions;
using Renderer3D.Models.Scene;
using System;
using System.Numerics;

namespace Renderer3D.Models.Processing.Shaders
{
    public static class PhongShader
    {
        public static int GetPixelColor(TriangleValue t, LightingProperties lightingProperties, CameraProperties cameraProperties, Vector3 point)
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

            Vector3 ambient = lightingProperties.AmbientIntensity;
            Vector3 diffuse = Calculation.GetDiffuseLightingColor(lightingProperties, lightVector, n);
            Vector3 reflection = Calculation.GetReflectionLightingColor(lightingProperties, hVector, n);

            Vector3 intensity = ambient + diffuse + reflection;
            intensity.X = Math.Min(intensity.X, 255);
            intensity.Y = Math.Min(intensity.Y, 255);
            intensity.Z = Math.Min(intensity.Z, 255);

            return intensity.ToColorInt();
        }
    }
}
