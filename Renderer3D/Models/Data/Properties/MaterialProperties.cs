using System.Drawing;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Data.Properties
{
    public enum IlluminationModel
    {
        /// <summary>
        /// A constant color illumination model, using the Kd for the material
        /// </summary>
        Simple,
        /// <summary>
        /// A diffuse illumination model using Lambertian shading, taking into account Ka, Kd, the intensity and position of each light source and the angle at which it strikes the surface.
        /// </summary>
        Diffuse,
        /// <summary>
        /// A diffuse and specular illumination model using Lambertian shading and Blinn's interpretation of Phong's specular illumination model,
        /// taking into account Ka, Kd, Ks, and the intensity and position of each light source and the angle at which it strikes the surface.
        /// </summary>
        Phong
    }

    public class MaterialProperties
    {
        /// <summary>
        /// Specifies ambient color, to account for light that is scattered about the entire scene 
        /// [see Wikipedia entry for Phong Reflection Model] using values between 0 and 1 for the RGB components.
        /// Also known as Ka.
        /// </summary>
        public Vector3 AmbientColorIntensity;

        /// <summary>
        /// Specifies diffuse color, which typically contributes most of the color to an object [see Wikipedia entry for Diffuse Reflection].
        /// In this example, Kd represents a grey color, which will get modified by a colored texture map specified in the map_Kd statement.
        /// Also known as Kd
        /// </summary>
        public Vector3 DiffuseColorIntensity;

        /// <summary>
        /// Specifies specular color, the color seen where the surface is shiny and mirror-like [see Wikipedia entry for Specular Reflection].
        /// Also knows as Ks.
        /// </summary>
        public Vector3 SpecularColorIntensity;

        /// <summary>
        /// Defines the focus of specular highlights in the material. Ns values normally range from 0 to 1000, with a high value resulting in a tight, concentrated highlight.
        /// Also known as Ns
        /// </summary>
        public float SpecularHighlight;

        /// <summary>
        /// Defines the optical density (aka index of refraction) in the current material. The values can range from 0.001 to 10. 
        /// A value of 1.0 means that light does not bend as it passes through an object.
        /// Also known as Ni
        /// </summary>
        public float OpticalDensity;

        /// <summary>
        /// Specifies a factor for dissolve, how much this material dissolves into the background. A factor of 1.0 is fully opaque. A factor of 0.0 is completely transparent.
        /// Also known as d.
        /// </summary>
        public float Dissolve;

        /// <summary>
        /// Specifies an illumination model, using a numeric value.
        /// Also known as illum.
        /// </summary>
        public IlluminationModel IlluminationModel;

        /// <summary>
        /// Specifies a color texture file to be applied to the diffuse reflectivity of the material. During rendering, values are multiplied by the Kd values to derive the RGB components.
        /// Also known as map_Kd.
        /// </summary>
        public string ColorTextureFileName;

        /// <summary>
        /// map_Ks.
        /// </summary>
        public string ColorSpecularFileName;

        /// <summary>
        /// map_Ke.
        /// </summary>
        public string ColorEmissiveFileName;


        public WriteableBitmap TextureColors;
        public WriteableBitmap SpecularColors;
    }
}
