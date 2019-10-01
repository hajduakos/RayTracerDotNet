namespace RayTracer.Common
{
    /// <summary>
    /// Ray with a starting point and a direction
    /// </summary>
    public readonly struct Ray
    {
        public Vec3 Start { get; }
        public Vec3 Dir { get; }

        public Ray(Vec3 start, Vec3 dir)
        {
            this.Start = start;
            this.Dir = dir.Normalize();
        }

        /// <summary>
        /// Create a new ray with the starting point shifted slightly towards the direction
        /// </summary>
        /// <returns></returns>
        public Ray Offset()
        {
            return new Ray(Start + Dir * Global.EPS, Dir);
        }
    }
}
