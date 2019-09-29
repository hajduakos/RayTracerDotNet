using System;
using RayTracer.Common;

namespace RayTracer.Objects
{
    public sealed class Sphere : ObjectBase
    {
        private readonly Vec3 center;
        private readonly float radius;

        public Sphere(Vec3 center, float radius, Material material) : base(material)
        {
            this.center = center;
            this.radius = radius;
        }

        public override Intersection Intersect(Ray ray)
        {
            float a = ray.Dir * ray.Dir;
            float b = 2 * (ray.Dir * (ray.Start - center));
            float c = (ray.Start - center) * (ray.Start - center) - radius * radius;
            float discr = b * b - 4 * a * c;

            if (discr < 0) return Intersection.Miss();
            float t;
            if (MathF.Abs(discr) < Global.EPS)
            {
                t = -b / 2 / a;
            }
            else
            {
                float t1 = (-b + MathF.Sqrt(discr)) / 2 / a;
                float t2 = (-b - MathF.Sqrt(discr)) / 2 / a;
                if (t1 > Global.EPS && t2 > Global.EPS) t = Math.Min(t1, t2);
                else if (t1 > Global.EPS) t = t1;
                else if (t2 > Global.EPS) t = t2;
                else t = -1;
            }

            if (t < Global.EPS) return Intersection.Miss();
            Vec3 n = (ray.Start + ray.Dir * t) - center;
            return Intersection.Hit(t, n);
        }
    }
}
