using RayTracer.Common;
using System;

namespace RayTracer.Objects
{
    /// <summary>
    /// Cyliner
    /// </summary>
    public class Cylinder : IObject
    {
        private readonly Vec3 c1;
        private readonly Vec3 c2;
        private readonly float r;
        private Material mat;

        /// <summary>
        /// Create new cylinder
        /// </summary>
        /// <param name="cap1center">Center of one cap</param>
        /// <param name="cap2center">Center of the other cap</param>
        /// <param name="r">Radius</param>
        /// <param name="material">Material</param>
        public Cylinder(Vec3 cap1center, Vec3 cap2center, float r, Material material)
        {
            this.c1 = cap1center;
            this.c2 = cap2center;
            this.r = r;
            this.mat = material;
        }

        private Intersection IntersectSide(Ray ray)
        {
            Vec3 va = (c2 - c1).Normalize();
            Vec3 dp = ray.Start - c1;
            float a = (ray.Dir - va * (ray.Dir * va)) * (ray.Dir - va * (ray.Dir * va));
            float b = (ray.Dir - va * (ray.Dir * va)) * (dp - va * (dp * va)) * 2;
            float c = (dp - va * (dp * va)) * (dp - va * (dp * va)) - r * r;
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
            Vec3 n = (pt - (c1 + va * MathF.Sqrt(MathF.Pow((pt - c1).Length, 2) - r * r))).Normalize();
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
            if ((pt - c1).Length > r) return null;
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
            if ((pt - c2).Length > r) return null;
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
