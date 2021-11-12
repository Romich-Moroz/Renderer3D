using Renderer3D.Models.Data;
using Renderer3D.Models.Data.Properties;
using Renderer3D.Models.Extensions;
using System.Numerics;

namespace Renderer3D.Models.Processing.Shaders
{
    public static class PhongShader
    {

        public static int GetPixelColor(MaterialProperties materialProperties, LightingProperties lightingProperties, CameraProperties cameraProperties, VertexValue vertex, RenderStruct renderStruct)
        {
            Vector3 viewVector = cameraProperties.CameraPosition - vertex.Coordinates.ToV3();
            Vector3 lightVector = lightingProperties.LightSourcePosition - vertex.Coordinates.ToV3();
            Vector3 hVector = Vector3.Normalize(viewVector + lightVector);

            Vector3 ambient = renderStruct.DiffuseColor * materialProperties.AmbientColorIntensity;
            Vector3 diffuse = Calculation.GetDiffuseLightingColor(materialProperties, lightVector, renderStruct);
            Vector3 reflection = Calculation.GetSpecularColor(materialProperties, hVector, renderStruct);

            Vector3 intensity = ambient + diffuse + reflection;

            return intensity.ToColor().ToInt();
        }
    }
}
