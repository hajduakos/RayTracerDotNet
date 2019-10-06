using RayTracer.Common;

namespace RayTracer.Objects
{
    /// <summary>
    /// Intersection data for a ray and an object
    /// </summary>
    public sealed class Intersection
    {
        /// <summary> Object hit by the intersection </summary>
        public IObject Obj { get; }

        /// <summary> Ray </summary>
        public Ray Ray { get; }

        /// <summary> Parameter value for the ray at the intersection </summary>
        public float T { get; }

        /// <summary> Intersection point </summary>
        public Vec3 IntsPt { get; }

        /// <summary> Material at the intersection point </summary>
        public Material Mat { get; }

        /// <summary> Normal at the intersection </summary>
        public Vec3 Normal { get; set; }

        public Intersection(IObject obj, Ray ray, float t, Vec3 n, Material mat)
        {
            this.Obj = obj;
            this.Ray = ray;
            this.T = t;
            this.IntsPt = ray.Start + ray.Dir * t;
            this.Normal = n;
            this.Mat = mat;

        }
    }
}
