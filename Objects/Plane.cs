﻿using RayTracer.Common;
using System;

namespace RayTracer.Objects
{
    /// <summary>
    /// Plane
    /// </summary>
    public class Plane : ObjectBase
    {
        private readonly Vec3 center;
        private readonly Vec3 normal;

        /// <summary>
        /// Create a new plane with a given center and normal
        /// </summary>
        /// <param name="center">Center</param>
        /// <param name="normal">Normal</param>
        /// <param name="material">Material</param>
        public Plane(Vec3 center, Vec3 normal, Material material) : base(material)
        {
            this.center = center;
            this.normal = normal.Normalize();
        }

        public override Intersection Intersect(Ray ray)
        {
            float div = ray.Dir * normal;
            if (MathF.Abs(div) < Global.EPS) return null; // Parallel ray
            float t = (center - ray.Start) * normal / div;
            if (t < Global.EPS) return null;
            return new Intersection(this, ray, t, normal);
        }
    }
}