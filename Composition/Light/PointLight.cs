using RayTracer.Common;

namespace RayTracer.Composition.Light
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

        /// <summary>
        /// Create a new point light
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="lum">Luminance</param>
        public PointLight(Vec3 pos, Color lum)
        {
            this.Pos = pos;
            this.Lum = lum;
        }
    }
}
