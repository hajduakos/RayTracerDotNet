using RayTracer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer.Composition
{
    public sealed class PointLight
    {
        public Vec3 Pos { get; }
        public Color Lum { get; }

        public PointLight(Vec3 pos, Color lum)
        {
            this.Pos = pos;
            this.Lum = lum;
        }
    }
}
