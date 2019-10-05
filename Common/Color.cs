namespace RayTracer.Common
{
    /// <summary>
    /// RGB color
    /// </summary>
    public readonly struct Color
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }

        public Color(float r = 0, float g = 0, float b = 0)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public float Lum { get { return .21f * R + .72f * G + .07f * B; } }

        public static Color operator +(Color a, Color b) => new Color(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Color operator -(Color a, Color b) => new Color(a.R - b.R, a.G - b.G, a.B - b.B);
        public static Color operator *(Color a, Color b) => new Color(a.R * b.R, a.G * b.G, a.B * b.B);
        public static Color operator *(Color a, float f) => new Color(a.R * f, a.G * f, a.B * f);
        public static Color operator /(Color a, Color b) => new Color(a.R / b.R, a.G / b.G, a.B / b.B);
    }
}
