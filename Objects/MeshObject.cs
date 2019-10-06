using RayTracer.Common;
using System;
using System.Collections.Generic;

namespace RayTracer.Objects
{
    /// <summary>
    /// Base class for objects consisting of a triangular mesh.
    /// Supports shading normals and bounding box.
    /// </summary>
    public class MeshObject : IObject
    {
        /// <summary> A triple of shading normals </summary>
        protected class ShadingNormals
        {
            public Vec3 A { get; }
            public Vec3 B { get; }
            public Vec3 C { get; }
            public ShadingNormals(Vec3 a, Vec3 b, Vec3 c)
            {
                this.A = a;
                this.B = b;
                this.C = c;
            }
        }

        /// <summary> Triangle with optional shading normals </summary>
        protected readonly struct Triangle
        {
            public Vec3 A { get; }
            public Vec3 B { get; }
            public Vec3 C { get; }

            public ShadingNormals SN { get; }

            public Triangle(Vec3 a, Vec3 b, Vec3 c, ShadingNormals sn = null)
            {
                this.A = a;
                this.B = b;
                this.C = c;
                this.SN = sn;
            }

            /// <summary>
            /// Intersect triangle with a ray
            /// </summary>
            /// <param name="ray">Ray</param>
            /// <returns>Intersection parameter value, or a negative number if there is no intersection</returns>
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

            /// <summary>
            /// Get the normal of the triangle at a given point
            /// </summary>
            /// <param name="at">Point (should be on the triangle)</param>
            /// <returns></returns>
            public Vec3 GetNormal(Vec3 at)
            {
                if (SN == null) // No shading normals
                {
                    return ((B - A) % (C - A)).Normalize();
                }
                else // Shading normals: interpolate
                {
                    float area = Area(A, B, C);
                    float wa = Area(B, C, at) / area;
                    float wb = Area(A, C, at) / area;
                    float wc = 1 - wa - wb;
                    return (SN.A * wa + SN.B * wb + SN.C * wc).Normalize();
                }
            }

            private float Area(Vec3 pa, Vec3 pb, Vec3 pc)
            {
                float a = (pb - pc).Length;
                float b = (pc - pa).Length;
                float c = (pa - pb).Length;
                float s = (a + b + c) / 2;
                return MathF.Sqrt(s * (s - a) * (s - b) * (s - c));
            }
        }

        private readonly List<Triangle> triangles;
        private IObject bound;
        private readonly Material mat;

        public MeshObject(Material material)
        {
            triangles = new List<Triangle>();
            bound = null;
            mat = material;
        }

        /// <summary>
        /// Add a new triangle to the mesh
        /// </summary>
        /// <param name="triangle">Triangle</param>
        protected void AddTriangle(Triangle triangle) => triangles.Add(triangle);

        /// <summary>
        /// Set the bounding box
        /// </summary>
        /// <param name="bound">Bounding box</param>
        protected void SetBound(IObject bound) => this.bound = bound;

        public Intersection Intersect(Ray ray)
        {
            // Check bound (if set)
            if (bound != null && bound.Intersect(ray) == null) return null;

            bool ints = false;
            float tmin = 0;
            int imin = 0;

            // Get the first intersection among triangles
            for (int i = 0; i < triangles.Count; ++i)
            {
                float t = triangles[i].Intersect(ray);
                if (t > Global.EPS && (!ints || t < tmin))
                {
                    tmin = t;
                    imin = i;
                    ints = true;
                }
            }
            if (ints) return new Intersection(this, ray, tmin, triangles[imin].GetNormal(ray.Start + ray.Dir * tmin), mat);
            return null;
        }
    }
}
