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
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Create a new camera
        /// </summary>
        /// <param name="eye">Position of the camera</param>
        /// <param name="lookat">Point where camera is looking)</param>
        /// <param name="hfov">Horizontal field of view (in radians)</param>
        /// <param name="focalDist">Focal distance (defines the focal plane)</param>
        /// <param name="screenWidthPx">Width of the screen (in pixels)</param>
        /// <param name="screenHeightPx">Height of the screen (in pixels)</param>
        public Camera(Vec3 eye, Vec3 lookat, float hfov, float focalDist, int screenWidthPx, int screenHeightPx)
        {
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            this.lookat = eye + (lookat - eye).Normalize() * focalDist;
            this.width = screenWidthPx;
            this.height = screenHeightPx;
            Vec3 w = eye - this.lookat;
            float f = w.Length;
            right = vup % w;
            right = right.Normalize() * f * MathF.Tan(hfov / 2);
            up = w % right;
            up = up.Normalize() * f * MathF.Tan(hfov / 2) * (screenHeightPx / (float)screenWidthPx);
        }

        public static Camera FromAngle(Vec3 lookat, float xyangle, float zangle, float hfov, float dist, float focalDist, int screenWidthPx, int screenHeightPx)
        {
            Vec3 diff = new Vec3(MathF.Cos(zangle) * MathF.Sin(xyangle), MathF.Cos(zangle) * MathF.Cos(xyangle), MathF.Sin(zangle)) * dist;
            return new Camera(lookat + diff, lookat, hfov, focalDist, screenWidthPx, screenHeightPx);
        }

        /// <summary>
        /// Create endpoint of a ray corresponding to a pixel
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <param name="xOffset">X offset within the pixel [0;1]</param>
        /// <param name="yOffset">Y offset within the pixel [0;1]</param>
        /// <returns>Ray endpoint</returns>
        public Vec3 GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f)
        {
            return lookat + right * (2 * (x + xOffset) / width - 1) + up * (2 * (y + yOffset) / height - 1);
        }

        public Vec3 Up { get { return up; } }

        public Vec3 Right { get { return right; } }

        public Vec3 Eye { get { return eye; } }
    }
}
