using RayTracer.Common;

namespace RayTracer.Composition
{
    /// <summary>
    /// Directional light source
    /// </summary>
    public sealed class DirLight
    {
        /// <summary> Direction from which the light comes </summary>
        public Vec3 Dir { get; }

        /// <summary> Luminance </summary>
        public Color Lum { get; }

        /// <summary>
        /// Create a new directional light
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="lum">Luminance</param>
        public DirLight(Vec3 dir, Color lum)
        {
            this.Dir = dir;
            this.Lum = lum;
        }
    }
}
