using RayTracer.Common;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace RayTracer.Composition.Light
{
    public class AreaLight
    {
        /// <summary> Position </summary>
        public Vec3 Pos { get; }

        /// <summary> Luminance </summary>
        public Color Lum { get; }

        /// <summary> Radius </summary>
        public float Radius { get; }

        /// <summary> Number of samples </summary>
        public int Samples { get; }

        /// <summary>
        /// Create a new area light
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="lum">Luminance</param>
        /// <param name="radius">Radius</param>
        /// <param name="samples">Samples</param>
        public AreaLight(Vec3 pos, Color lum, float radius, int samples)
        {
            Contract.Requires(samples > 0, "Number of samples must be greater than 0");
            this.Pos = pos;
            this.Lum = lum;
            this.Radius = radius;
            this.Samples = samples;
        }

        /// <summary>
        /// Convert light to a list of point lights by random sampling
        /// </summary>
        /// <returns>List of point lights</returns>
        public List<PointLight> ToPointLights()
        {
            // For only one sample, return the center
            if (Samples == 1) return new List<PointLight>() { new PointLight(Pos, Lum) };

            List<PointLight> pl = new List<PointLight>(Samples);
            float x, y, z;
            ThreadSafeRandom trnd = new ThreadSafeRandom();
            while (pl.Count < Samples)
            {
                do
                {
                    x = trnd.NextFloat() * 2 * Radius - Radius;
                    y = trnd.NextFloat() * 2 * Radius - Radius;
                    z = trnd.NextFloat() * 2 * Radius - Radius;
                } while (x * x + y * y + z * z > Radius * Radius);
                pl.Add(new PointLight(Pos + new Vec3(x, y, z), Lum / Samples));
            }
            return pl;
        }
    }
}
