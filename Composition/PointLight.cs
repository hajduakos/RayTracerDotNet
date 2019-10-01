using RayTracer.Common;

namespace RayTracer.Composition
{
    /// <summary>
    /// Point light source
    /// </summary>
    public sealed class PointLight
    {
        /// <summary> Position </summary>
        public Vec3 Pos { get; }

        /// <summary> Luminance </summary>
        public Color Lum { get; }

        public PointLight(Vec3 pos, Color lum)
        {
            this.Pos = pos;
            this.Lum = lum;
        }
    }
}
