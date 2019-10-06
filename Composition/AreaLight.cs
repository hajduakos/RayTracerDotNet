using RayTracer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer.Composition
{
    public class AreaLight
    {
        /// <summary> Position </summary>
        public Vec3 Pos { get; }

        /// <summary> Luminance </summary>
        public Color Lum { get; }

        public float Radius { get; }

        public int Samples { get; }

        public AreaLight(Vec3 pos, Color lum, float radius, int samples)
        {
            this.Pos = pos;
            this.Lum = lum;
            this.Radius = radius;
            this.Samples = samples;
        }

        public List<PointLight> ToPointLights()
        {
            List<PointLight> pl = new List<PointLight>(Samples);
            float x, y, z;
            Random rnd = new Random();
            while (pl.Count < Samples)
            {
                do
                {
                    x = (float)(rnd.NextDouble() * 2 * Radius - Radius);
                    y = (float)(rnd.NextDouble() * 2 * Radius - Radius);
                    z = (float)(rnd.NextDouble() * 2 * Radius - Radius);
                } while (x * x + y * y + z * z > Radius * Radius);
                pl.Add(new PointLight(Pos + new Vec3(x, y, z), Lum / Samples));
            }
            return pl;
        }
    }
}
