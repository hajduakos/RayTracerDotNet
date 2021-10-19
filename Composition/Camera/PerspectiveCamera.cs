using RayTracer.Common;
using System;

namespace RayTracer.Composition.Camera
{
    /// <summary>
    /// Camera with perspective projection
    /// </summary>
    public sealed class PerspectiveCamera : ICamera
    {
        private readonly Vec3 eye;
        private readonly Vec3 lookat;
        private readonly Vec3 right;
        private readonly Vec3 up;
        private readonly int screenWidth;
        private readonly int screenHeight;

        /// <summary>
        /// Create a new perspective camera
        /// </summary>
        /// <param name="eye">Position of the camera</param>
        /// <param name="lookat">Point where camera is looking</param>
        /// <param name="hfov">Horizontal field of view (in degrees)</param>
        /// <param name="focalDist">Focal distance (defines the focal plane)</param>
        /// <param name="screenWidth">Width of the screen (in pixels)</param>
        /// <param name="screenHeight">Height of the screen (in pixels)</param>
        public PerspectiveCamera(Vec3 eye, Vec3 lookat, float hfov, int screenWidth, int screenHeight, float? focalDist = null)
        {
            float hfovRadTan = MathF.Tan(hfov * MathF.PI / 180 / 2);
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            float fd = focalDist ?? (lookat - eye).Length;
            this.lookat = eye + (lookat - eye).Normalize() * fd;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            Vec3 w = eye - this.lookat;
            float f = w.Length;
            right = vup % w;
            right = right.Normalize() * f * hfovRadTan;
            up = w % right;
            up = up.Normalize() * f * hfovRadTan * (screenHeight / (float)screenWidth);
        }

        /// <inheritdoc/>
        public Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f)
        {
            return lookat + right * (2 * (x + xOffset) / screenWidth - 1) + up * (2 * (y + yOffset) / screenHeight - 1);
        }

        /// <inheritdoc/>
        public Vec3 Up => up;

        /// <inheritdoc/>
        public Vec3 Right => right;

        /// <inheritdoc/>
        public Vec3 Eye => eye;

        /// <inheritdoc/>
        public int ScreenWidth => screenWidth;

        /// <inheritdoc/>
        public int ScreenHeight => screenHeight;
    }
}
