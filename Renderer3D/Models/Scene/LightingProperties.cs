using System.Numerics;

namespace Renderer3D.Models.Scene
{
    public struct LightingProperties
    {
        /// <summary>
        /// Position of the light source
        /// </summary>
        public Vector3 LightSourcePosition { get; set; }
        public float LightSourceIntensity { get; set; }

        public float Ka { get; set; }
        public Vector3 Ia { get; set; }

        public float Kd { get; set; }
        public Vector3 Id { get; set; }

        public float Ks { get; set; }
        public Vector3 Is { get; set; }
        public float ShininessCoefficient { get; set; }


        public LightingProperties(Vector3 lightSourcePosition, float lightSourceIntensity, float ka,
                                  Vector3 ia, Vector3 id, float kd,
                                  Vector3 @is, float ks, float shininessCoefficient)
        {
            LightSourcePosition = lightSourcePosition;
            LightSourceIntensity = lightSourceIntensity;
            Ka = ka;
            Ia = ia;
            Id = id;
            Kd = kd;
            Is = @is;
            Ks = ks;
            ShininessCoefficient = shininessCoefficient;
        }
    }
}
