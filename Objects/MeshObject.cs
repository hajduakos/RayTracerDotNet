using System;
using System.Collections.Generic;
using System.Text;
using RayTracer.Common;

namespace RayTracer.Objects
{
    public class MeshObject : ObjectBase
    {
        protected readonly struct Triangle
        {
            public Vec3 A { get; }
            public Vec3 B { get; }
            public Vec3 C { get; }

            public Triangle(Vec3 a, Vec3 b, Vec3 c)
            {
                this.A = a;
                this.B = b;
                this.C = c;
            }

            public float Intersect(Ray ray)
            {
                Vec3 x = (B - A) % (C - A);
                float t = (x * A - x * ray.Start) / (x * ray.Dir);
                if (t < Global.EPS) return -1;
                Vec3 p = ray.Start + ray.Dir * t;
                if ((((B - A) % (p - A)) * x) < 0) return -1;
                if ((((C - B) % (p - B)) * x) < 0) return -1;
                if ((((A - C) % (p - C)) * x) < 0) return -1;

                return t;
            }

            public Vec3 GetNormal() => ((B - A) % (C - A)).Normalize();
        }

        private List<Triangle> triangles;
        
        public MeshObject(Material material) : base(material)
        {
            triangles = new List<Triangle>();
        }

        protected void AddTriangle(Triangle t) => triangles.Add(t);

        public override Intersection Intersect(Ray ray)
        {
            bool ints = false;
            float tmin = 0;
            int imin = 0;

            for(int i = 0; i < triangles.Count; ++i)
            {
                float t = triangles[i].Intersect(ray);
                if (t > Global.EPS && (!ints || t < tmin))
                {
                    tmin = t;
                    imin = i;
                    ints = true;
                }
            }
            if (ints) return new Intersection(this, ray, tmin, triangles[imin].GetNormal());
            return null;
        }
    }
}
