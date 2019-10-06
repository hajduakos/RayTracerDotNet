using System;
using System.Collections.Generic;
using System.Text;
using RayTracer.Common;

namespace RayTracer.Objects
{
    public class Cone : IObject
    {
        private readonly Vec3 c1;
        private readonly Vec3 c2;
        private readonly float r1;
        private readonly float r2;
        private readonly Material mat;

        public Cone(Vec3 cap1center, Vec3 cap2center, float cap1radius, float cap2radius, Material material)
        {
            this.c1 = cap1center;
            this.c2 = cap2center;
            this.r1 = cap1radius;
            this.r2 = cap2radius;
            this.mat = material;
        }

        private Intersection IntersectSide(Ray ray)
        {
            Vec3 va = (c2 - c1).Normalize();
            Vec3 pa = c1 + (c2 - c1) * (r1 / (r1 - r2));
            float alpha = MathF.Atan2(r1 - r2, (c2 - c1).Length);
            Vec3 dp = ray.Start - pa;

            float a = MathF.Pow(MathF.Cos(alpha), 2) * ((ray.Dir - va * (ray.Dir * va)) * (ray.Dir - va * (ray.Dir * va))) -
                MathF.Pow(MathF.Sin(alpha), 2) * ((ray.Dir * va) * (ray.Dir * va));
            float b = 2 * MathF.Pow(MathF.Cos(alpha), 2) * ((ray.Dir - va * (ray.Dir * va)) * (dp - va * (dp * va))) -
                2 * MathF.Pow(MathF.Sin(alpha), 2) * (ray.Dir * va) * (dp * va);
            float c = MathF.Pow(MathF.Cos(alpha), 2) * ((dp - va * (dp * va)) * (dp - va * (dp * va))) -
                MathF.Pow(MathF.Sin(alpha), 2) * ((dp * va) * (dp * va));
            float discr = b * b - 4 * a * c;

            if (discr < 0) return null; // No intersection
            float t;
            if (MathF.Abs(discr) < Global.EPS) // One possible intersection
            {
                t = -b / 2 / a;
            }
            else // Two possible intersections: get the minimal positive
            {
                float t1 = (-b + MathF.Sqrt(discr)) / 2 / a;
                float t2 = (-b - MathF.Sqrt(discr)) / 2 / a;
                bool t1valid = t1 > Global.EPS && va * (ray.Start + ray.Dir * t1 - c1) > 0 && va * (ray.Start + ray.Dir * t1 - c2) < 0;
                bool t2valid = t2 > Global.EPS && va * (ray.Start + ray.Dir * t2 - c1) > 0 && va * (ray.Start + ray.Dir * t2 - c2) < 0;
                if (t1valid && t2valid) t = Math.Min(t1, t2);
                else if (t1valid) t = t1;
                else if (t2valid) t = t2;
                else t = -1;
            }

            if (t < Global.EPS) return null; // No intersection
            Vec3 pt = ray.Start + ray.Dir * t;
            float x = (pt - c1).Length;
            float r = ((c1 - pt) - va * ((c1 - pt) * va)).Length;
            Vec3 p0 = c1 + va * (MathF.Sqrt(x * x - r * r) - r * MathF.Tan(alpha));
            Vec3 n = (pt - p0).Normalize();
            return new Intersection(this, ray, t, n, mat);
        }

        private Intersection IntersectBottomCap(Ray ray)
        {
            Vec3 normal = (c1 - c2).Normalize();
            float div = ray.Dir * normal;
            if (MathF.Abs(div) < Global.EPS) return null; // Parallel ray
            float t = (c1 - ray.Start) * normal / div;
            if (t < Global.EPS) return null;
            Vec3 pt = ray.Start + ray.Dir * t;
            if ((pt - c1).Length > r1) return null;
            return new Intersection(this, ray, t, normal, mat);
        }

        private Intersection IntersectTopCap(Ray ray)
        {
            Vec3 normal = (c2 - c1).Normalize();
            float div = ray.Dir * normal;
            if (MathF.Abs(div) < Global.EPS) return null; // Parallel ray
            float t = (c2 - ray.Start) * normal / div;
            if (t < Global.EPS) return null;
            Vec3 pt = ray.Start + ray.Dir * t;
            if ((pt - c2).Length > r2) return null;
            return new Intersection(this, ray, t, normal, mat);
        }

        public Intersection Intersect(Ray ray)
        {
            // Try to intersect side, top cap, bottom cap and get minimum
            Intersection imin = null;
            Intersection side = IntersectSide(ray);
            Intersection top = IntersectTopCap(ray);
            Intersection bottom = IntersectBottomCap(ray);

            if (side != null && (imin == null || side.T < imin.T)) imin = side;
            if (top != null && (imin == null || top.T < imin.T)) imin = top;
            if (bottom != null && (imin == null || bottom.T < imin.T)) imin = bottom;

            return imin;
        }
    }
}
