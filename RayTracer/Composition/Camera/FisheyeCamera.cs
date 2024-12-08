using RayTracer.Common;
using System;

namespace RayTracer.Composition.Camera
{
    /// <summary>
    /// Camera with fisheye projection
    /// </summary>
    public sealed class FisheyeCamera : ICamera
    {
        private readonly Vec3 eye;
        private readonly Vec3 dir;
        private readonly Vec3 right;
        private readonly Vec3 up;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int screenSize;
        private readonly float focalDist;
        private readonly float normalizer;

        /// <summary>
        /// Create a new fisheye camera
        /// </summary>
        /// <param name="eye">Position of the camera</param>
        /// <param name="lookat">Point where camera is looking</param>
        /// <param name="focalDist">Focal distance (defines the focal plane/sphere)</param>
        /// <param name="screenWidth">Width of the screen (in pixels)</param>
        /// <param name="screenHeight">Height of the screen (in pixels)</param>
        /// <param name="diagonal">Make the camera diagonal instead of circular</param>
        public FisheyeCamera(Vec3 eye, Vec3 lookat, int screenWidth, int screenHeight, float? focalDist = null, bool diagonal = false)
        {
            Vec3 vup = new(0, 0, 1);
            this.eye = eye;
            this.dir = (lookat - eye).Normalize();
            this.focalDist = focalDist ?? (lookat - eye).Length;
            this.screenSize = Math.Min(screenHeight, screenWidth);
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            if (diagonal)
                this.normalizer = MathF.Sqrt(
                    this.screenWidth * this.screenWidth / (float)screenSize / screenSize +
                    this.screenHeight * this.screenHeight / (float)screenSize / screenSize);
            else
                this.normalizer = 1.0f;
            Vec3 w = eye - lookat;
            right = (vup % w).Normalize();
            up = (w % right).Normalize();
        }

        /// <inheritdoc/>
        public Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f)
        {
            float xNorm = ((x + xOffset) - screenWidth / 2.0f) / ( screenSize / 2.0f) / normalizer;
            float yNorm = ((y + yOffset) - screenHeight / 2.0f) / (screenSize / 2.0f) / normalizer;
            float diag = xNorm * xNorm + yNorm * yNorm;
            if (diag > 1) return null;
            float z = MathF.Sqrt(1 - diag);
            return eye + (right * xNorm + up * yNorm + dir * z) * focalDist;
        }

        /// <inheritdoc/>
        public Vec3 GetEye(int x, int y, float xOffset = 0.5F, float yOffset = 0.5F) => eye;

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
