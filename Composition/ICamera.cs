using RayTracer.Common;

namespace RayTracer.Composition
{
    public interface ICamera
    {
        Vec3 Up { get; }
        Vec3 Right { get; }
        Vec3 Eye { get; }
        Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f);
    }
}
