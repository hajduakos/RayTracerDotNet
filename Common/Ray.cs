namespace RayTracer.Common
{
    public readonly struct Ray
    {
        public Vec3 Start { get; }
        public Vec3 Dir { get; }

        public Ray(Vec3 start, Vec3 dir)
        {
            this.Start = start;
            this.Dir = dir.Normalize();
        }

        public Ray Offset()
        {
            return new Ray(Start + Dir * Global.EPS, Dir);
        }
    }
}
