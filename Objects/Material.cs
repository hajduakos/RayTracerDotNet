using RayTracer.Common;
using System;

namespace RayTracer.Objects
{
    public class Material
    {
        public Color Ambient { get; }
        public Color Diffuse { get; }
        public Color Specular { get; }

        public float Shine { get; }

        public bool IsReflective { get; }
        public bool IsRefractive { get; }

        public Color N { get; } // Index of reflection
        public Color Kap { get; } // Absorbtion coefficient

        public Material(Color ambient, Color diffuse, Color specular, float shine,
            bool isReflective, bool isRefractive, Color n, Color kap)
        {
            this.Ambient = ambient;
            this.Diffuse = diffuse;
            this.Specular = specular;
            this.Shine = shine;
            this.IsReflective = isReflective;
            this.IsRefractive = isRefractive;
            this.N = n;
            this.Kap = kap;
        }

        public Material(Color ambient, Color diffuse, Color specular, float shine) : this(ambient, diffuse, specular, shine, false, false, new Color(), new Color()) { }

        public Color GetFresnel(float costh)
        {
            float r = (MathF.Pow(N.R - 1, 2) + MathF.Pow(Kap.R, 2)) / (MathF.Pow(N.R + 1, 2) + MathF.Pow(Kap.R, 2));
            float g = (MathF.Pow(N.G - 1, 2) + MathF.Pow(Kap.G, 2)) / (MathF.Pow(N.G + 1, 2) + MathF.Pow(Kap.G, 2));
            float b = (MathF.Pow(N.B - 1, 2) + MathF.Pow(Kap.B, 2)) / (MathF.Pow(N.B + 1, 2) + MathF.Pow(Kap.B, 2));
            Color f0 = new Color(r, g, b);
            return f0 + (f0 * (-1) + new Color(1, 1, 1)) * MathF.Pow(1 - costh, 5);
        }
    }
}
