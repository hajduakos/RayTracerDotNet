using RayTracer.Common;

namespace RayTracer.Objects
{
    /// <summary>
    /// Intersection data for a ray and an object
    /// </summary>
    public readonly struct Intersection
    {
        /// <summary> Is it an intersection </summary>
        public bool IsHit { get; }

        /// <summary> Parameter value for the ray at the intersection </summary>
        public float T { get; }

        /// <summary> Normal at the intersection </summary>
        public Vec3 Normal { get; }
    
        private Intersection(bool isHit, float t, Vec3 n)
        {
            this.IsHit = isHit;
            this.T = t;
            this.Normal = n;
        }

        /// <summary>
        /// Create a new actual intersection
        /// </summary>
        public static Intersection Hit(float t, Vec3 n) => new Intersection(true, t, n);

        /// <summary>
        /// Create a new intersection that is a miss
        /// </summary>
        public static Intersection Miss() => new Intersection(false, 0, new Vec3());
    }
}
