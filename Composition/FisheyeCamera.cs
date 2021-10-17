﻿using RayTracer.Common;
using System;

namespace RayTracer.Composition
{
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


        public FisheyeCamera(Vec3 eye, Vec3 lookat, float focalDist, int screenWidthPx, int screenHeightPx, bool diagonal = false)
        {
            Vec3 vup = new Vec3(0, 0, 1);
            this.eye = eye;
            this.dir = (lookat - eye).Normalize();
            this.focalDist = focalDist;
            this.screenSize = Math.Min(screenHeightPx, screenWidthPx);
            this.width = screenWidthPx;
            this.height = screenHeightPx;
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

        public Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f)
        {
            float xNorm = ((x + xOffset) - width / 2.0f) / ( screenSize / 2.0f) / normalizer;
            float yNorm = ((y + yOffset) - height / 2.0f) / (screenSize / 2.0f) / normalizer;
            float diag = xNorm * xNorm + yNorm * yNorm;
            if (diag > 1) return null;
            float z = MathF.Sqrt(1 - diag);
            return eye + (right * xNorm + up * yNorm + dir * z) * focalDist;
        }


        public Vec3 Up { get { return up; } }

        public Vec3 Right { get { return right; } }

        public Vec3 Eye { get { return eye; } }
    }
}