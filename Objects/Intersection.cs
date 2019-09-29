using RayTracer.Common;

namespace RayTracer.Objects
{
    public readonly struct Intersection
    {
        public bool IsHit { get; }
        public float T { get; }
        public Vec3 Normal { get; }
    
        private Intersection(bool isHit, float t, Vec3 n)
        {
            this.IsHit = isHit;
            this.T = t;
            this.Normal = n;
        }

        public static Intersection Hit(float t, Vec3 n) => new Intersection(true, t, n);
        public static Intersection Miss() => new Intersection(false, 0, new Vec3());
    }
}
