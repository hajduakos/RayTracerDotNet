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
        private readonly int width;
        private readonly int height;
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
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            this.dir = (lookat - eye).Normalize();
            this.focalDist = focalDist.HasValue ? focalDist.Value : (lookat - eye).Length;
            this.screenSize = Math.Min(screenHeight, screenWidth);
            this.width = screenWidth;
            this.height = screenHeight;
            if (diagonal)
                this.normalizer = MathF.Sqrt(
                    width * width / (float)screenSize / screenSize +
                    height * height / (float)screenSize / screenSize);
            else
                this.normalizer = 1.0f;
            Vec3 w = eye - lookat;
            right = (vup % w).Normalize();
            up = (w % right).Normalize();
        }

        /// <inheritdoc/>
        public Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f)
        {
            float xNorm = ((x + xOffset) - width / 2.0f) / ( screenSize / 2.0f) / normalizer;
            float yNorm = ((y + yOffset) - height / 2.0f) / (screenSize / 2.0f) / normalizer;
            float diag = xNorm * xNorm + yNorm * yNorm;
            if (diag > 1) return null;
            float z = MathF.Sqrt(1 - diag);
            return eye + (right * xNorm + up * yNorm + dir * z) * focalDist;
        }

        /// <inheritdoc/>
        public Vec3 Up { get { return up; } }

        /// <inheritdoc/>
        public Vec3 Right { get { return right; } }

        /// <inheritdoc/>
        public Vec3 Eye { get { return eye; } }

        /// <inheritdoc/>
        public int ScreenWidth => width;

        /// <inheritdoc/>
        public int ScreenHeight => height;
    }
}
