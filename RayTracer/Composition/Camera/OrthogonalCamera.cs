using RayTracer.Common;
using System;

namespace RayTracer.Composition.Camera
{
    public sealed class OrthogonalCamera : ICamera
    {
        private readonly Vec3 eye;
        private readonly Vec3 lookat;
        private readonly Vec3 right;
        private readonly Vec3 up;
        private readonly int screenWidth;
        private readonly int screenHeight;

        /// <summary>
        /// Create a new orthogonal camera
        /// </summary>
        /// <param name="eye">Position of the camera</param>
        /// <param name="lookat">Point where camera is looking</param>
        /// <param name="width">Width of hypothetical sensor (defines field of view)</param>
        /// <param name="focalDist">Focal distance (defines the focal plane)</param>
        /// <param name="screenWidth">Width of the screen (in pixels)</param>
        /// <param name="screenHeight">Height of the screen (in pixels)</param>
        public OrthogonalCamera(Vec3 eye, Vec3 lookat, float width, int screenWidth, int screenHeight, float? focalDist = null)
        {
            Vec3 vup = new(0, 0, 1);
            this.eye = eye;
            float fd = focalDist ?? (lookat - eye).Length;
            this.lookat = eye + (lookat - eye).Normalize() * fd;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            Vec3 w = eye - this.lookat;
            right = vup % w;
            right = right.Normalize() * (width / 2);
            up = w % right;
            up = up.Normalize() * (width / 2) * (screenHeight / (float)screenWidth);
        }

        /// <inheritdoc/>
        public Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f) =>
            lookat + right * (2 * (x + xOffset) / screenWidth - 1) + up * (2 * (y + yOffset) / screenHeight - 1);

        /// <inheritdoc/>
        public Vec3 GetEye(int x, int y, float xOffset = 0.5F, float yOffset = 0.5F) =>
            eye + right * (2 * (x + xOffset) / screenWidth - 1) + up * (2 * (y + yOffset) / screenHeight - 1);

        /// <inheritdoc/>
        public Vec3 Up => up;

        /// <inheritdoc/>
        public Vec3 Right => right;

        /// <inheritdoc/>
        public int ScreenWidth => screenWidth;

        /// <inheritdoc/>
        public int ScreenHeight => screenHeight;
    }
}
