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
            if (MathF.Abs(ray.Dir.Z) < Global.EPS) return Intersection.Miss();
            float t = -ray.Start.Z / ray.Dir.Z;
            if (t < 0) return Intersection.Miss();
            Vec3 n = new Vec3(0, 0, 1);
            return Intersection.Hit(t, n);
        }
    }
}
