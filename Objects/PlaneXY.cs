using System;
using RayTracer.Common;

namespace RayTracer.Objects
{
    public class PlaneXY : ObjectBase
    {
        public PlaneXY(Material material) : base(material)
        {
        }

        public override Intersection Intersect(Ray ray)
        {
            if (MathF.Abs(ray.Dir.Z) < Global.EPS) return null;
            float t = -ray.Start.Z / ray.Dir.Z;
            if (t < 0) return null;
            Vec3 n = new Vec3(0, 0, 1);
            return new Intersection(this, ray, t, n);
        }
    }
}
