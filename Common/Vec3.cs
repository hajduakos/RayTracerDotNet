using System;

namespace RayTracer.Common
{
    /// <summary>
    /// 3D vector
    /// </summary>
    public readonly struct Vec3
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vec3(float x = 0, float y = 0, float z = 0)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public float Length { get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } }
        public Vec3 Normalize() => this * (1f / Length);

        public static Vec3 operator -(Vec3 a) => a * (-1);
        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 a, float f) => new Vec3(a.X * f, a.Y * f, a.Z * f);

        /// <summary>  Dot product </summary>
        public static float operator *(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary> Cross product </summary>
        public static Vec3 operator %(Vec3 a, Vec3 b) => new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

        /// <summary> Dot product with itself </summary>
        public float Square() => this * this;

        public static Vec3 FromAngle(Vec3 origin, float xyangle, float zangle, float dist)
        {
            return origin +  new Vec3(
                MathF.Cos(zangle) * MathF.Sin(xyangle),
                MathF.Cos(zangle) * MathF.Cos(xyangle),
                MathF.Sin(zangle)) * dist;
        }
    }
}
