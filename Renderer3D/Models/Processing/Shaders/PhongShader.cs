﻿using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System;
using System.Numerics;

namespace Renderer3D.Models.Processing.Shaders
{
    public static class PhongShader
    {
        public static int GetPixelColor(MaterialProperties materialProperties, LightingProperties lightingProperties, CameraProperties cameraProperties, VertexValue vertex)
        {
            Vector3 viewVector = cameraProperties.CameraPosition - vertex.Coordinates;
            Vector3 lightVector = lightingProperties.LightSourcePosition - vertex.Coordinates;
            Vector3 hVector = Vector3.Normalize(viewVector + lightVector);
            Vector3 n = Vector3.Normalize(vertex.Normal);

            Vector3 ambient = materialProperties.AmbientColorIntensity;
            Vector3 diffuse = Calculation.GetDiffuseLightingColor(materialProperties, lightVector, n, vertex.Texture);
            Vector3 reflection = Calculation.GetSpecularColor(materialProperties, hVector, n, vertex.Texture);

            Vector3 intensity = ambient + diffuse + reflection;
            intensity.X = Math.Min(intensity.X, 255);
            intensity.Y = Math.Min(intensity.Y, 255);
            intensity.Z = Math.Min(intensity.Z, 255);

            return intensity.ToInt();
        }
    }
}
