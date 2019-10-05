using RayTracer.Common;
using System;

namespace RayTracer.Objects
{
    /// <summary>
    /// Checkerboard plane with alternating material
    /// </summary>
    public class CheckerBoard : IObject
    {
        private readonly Vec3 center;
        private readonly Vec3 normal;
        private readonly Vec3 mv1;
        private readonly Vec3 mv2;
        private readonly Vec3 matDir;
        private readonly Material mat1;
        private readonly Material mat2;

        /// <summary>
        /// Create a new checkerboard plane
        /// </summary>
        /// <param name="center">Center of the plance</param>
        /// <param name="normal">Normal of the plane</param>
        /// <param name="matDir">Pattern direction (will be projected to plane)</param>
        /// <param name="mat1">First material</param>
        /// <param name="mat2">Second material</param>
        public CheckerBoard(Vec3 center, Vec3 normal, Vec3 matDir, Material mat1, Material mat2)
        {
            this.center = center;
            this.normal = normal.Normalize();
            this.matDir = matDir;
            // Project material directon to two perpendiculars
            this.mv1 = (matDir % normal).Normalize();
            this.mv2 = (normal % mv1).Normalize();
            this.mat1 = mat1;
            this.mat2 = mat2;
        }

        public Intersection Intersect(Ray ray)
        {
            float div = ray.Dir * normal;
            if (MathF.Abs(div) < Global.EPS) return null; // Parallel ray
            float t = (center - ray.Start) * normal / div;
            if (t < Global.EPS) return null;

            Vec3 mp = ray.Start + ray.Dir * t - center;
            int w1 = (int)MathF.Floor(mp * mv1 / matDir.Length);
            int w2 = (int)MathF.Floor(mp * mv2 / matDir.Length);
            Material mat = Mod(w1, 2) == Mod(w2, 2) ? mat1 : mat2;

            return new Intersection(this, ray, t, normal, mat);
        }

        private int Mod(int k, int n) => ((k %= n) < 0) ? k + n : k;
    }
}
