using RayTracer.Common;
using System;

namespace RayTracer.Composition
{
    /// <summary>
    /// Camera
    /// </summary>
    public sealed class Camera
    {
        private readonly Vec3 eye;
        private readonly Vec3 lookat;
        private readonly Vec3 right;
        private readonly Vec3 up;
        private readonly int w;
        private readonly int h;

        /// <summary>
        /// Create a new camera
        /// </summary>
        /// <param name="eye">Position of the camera</param>
        /// <param name="lookat">Point where camera is looking</param>
        /// <param name="hfov">Horizontal field of view (in radians)</param>
        /// <param name="screenWidthPx">Width of the screen (in pixels)</param>
        /// <param name="screenHeightPx">Height of the screen (in pixels)</param>
        public Camera(Vec3 eye, Vec3 lookat, float hfov, int screenWidthPx, int screenHeightPx)
        {
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            this.lookat = lookat;
            this.w = screenWidthPx;
            this.h = screenHeightPx;
            Vec3 w = eye - lookat;
            float f = w.Length;
            right = vup % w;
            right = right.Normalize() * f * MathF.Tan(hfov / 2);
            up = w % right;
            up = up.Normalize() * f * MathF.Tan(hfov / 2) * (screenHeightPx / (float)screenWidthPx);
        }

        /// <summary>
        /// Create a ray corresponding to a pixel
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <returns>Ray</returns>
        public Ray GetRay(int x, int y)
        {
            Vec3 dir = lookat + right * (2 * (x + .5f) / w - 1) + up * (2 * (y + .5f) / h - 1) - eye;
            return new Ray(eye, dir);
        }
    }
}
