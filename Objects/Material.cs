using RayTracer.Common;
using System;
using System.Diagnostics.Contracts;

namespace RayTracer.Objects
{
    public class Material
    {
        public bool IsRough { get { return Rough > Global.EPS; } }
        public float Rough { get; }
        public Color Ambient { get; }
        public Color Diffuse { get; }
        public Color Specular { get; }

        public float Shine { get; }

        public bool IsSmooth { get { return Smooth > Global.EPS; } }
        public float Smooth { get; }
        public bool IsReflective { get; }
        public bool IsRefractive { get; }

        public Color N { get; } // Index of reflection
        public Color Kap { get; } // Absorbtion coefficient
        public float Blur { get; }
        public int BlurSamples { get; }
        private readonly Color f0;

        public Material(float rough, Color ambient, Color diffuse, Color specular, float shine,
            float smooth, bool isReflective, bool isRefractive, Color n, Color kap, float blur, int blurSamples)
        {
            Contract.Requires(blurSamples > 0, "Blur samples must be greater than 0");
            this.Rough = rough;
            this.Ambient = ambient;
            this.Diffuse = diffuse;
            this.Specular = specular;
            this.Shine = shine;
            this.Smooth = smooth;
            this.IsReflective = isReflective;
            this.IsRefractive = isRefractive;
            this.N = n;
            this.Kap = kap;
            this.Blur = blur;
            this.BlurSamples = blurSamples;
            // Cache F0 for Fresnel
            float r = (MathF.Pow(N.R - 1, 2) + MathF.Pow(Kap.R, 2)) / (MathF.Pow(N.R + 1, 2) + MathF.Pow(Kap.R, 2));
            float g = (MathF.Pow(N.G - 1, 2) + MathF.Pow(Kap.G, 2)) / (MathF.Pow(N.G + 1, 2) + MathF.Pow(Kap.G, 2));
            float b = (MathF.Pow(N.B - 1, 2) + MathF.Pow(Kap.B, 2)) / (MathF.Pow(N.B + 1, 2) + MathF.Pow(Kap.B, 2));
            this.f0 = new Color(r, g, b);
        }

        public Color GetFresnel(float costh)
        {
            return f0 + (f0 * (-1) + new Color(1, 1, 1)) * MathF.Pow(1 - costh, 5);
        }
    }
}
