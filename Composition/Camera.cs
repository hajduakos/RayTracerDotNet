using RayTracer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer.Composition
{
    public sealed class Camera
    {
        private readonly Vec3 eye;
        private readonly Vec3 lookat;
        private readonly Vec3 right;
        private readonly Vec3 up;
        private readonly int w;
        private readonly int h;

        public Camera(Vec3 eye, Vec3 lookat, float hfov, int screenWidth, int screenHeight)
        {
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            this.lookat = lookat;
            this.w = screenWidth;
            this.h = screenHeight;
            Vec3 w = eye - lookat;
            float f = w.Length;
            right = vup % w;
            right = right.Normalize() * f * MathF.Tan(hfov / 2);
            up = w % right;
            up = up.Normalize() * f * MathF.Tan(hfov / 2) * (screenHeight / (float)screenWidth);
        }

        public Ray GetRay(int x, int y)
        {
            Vec3 dir = lookat + right * (2 * (x + .5f) / w - 1) + up * (2 * (y + .5f) / h - 1) - eye;
            return new Ray(eye, dir);
        }
    }
}
