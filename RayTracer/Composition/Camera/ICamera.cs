using RayTracer.Common;

namespace RayTracer.Composition.Camera
{
    /// <summary>
    /// Common interface for cameras
    /// </summary>
    public interface ICamera
    {

        /// <summary>
        /// Upwards direction
        /// </summary>
        Vec3 Up { get; }

        /// <summary>
        /// Right direction
        /// </summary>
        Vec3 Right { get; }

        /// <summary>
        /// Widht of the screen in pixels
        /// </summary>
        int ScreenWidth { get; }

        /// <summary>
        /// Height of the screen in pixels
        /// </summary>
        int ScreenHeight { get; }

        /// <summary>
        /// Get the endpoint of a ray, corresponding to a pixel,
        /// or null if the position is invalid
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <param name="xOffset">X offset within the pixel [0;1]</param>
        /// <param name="yOffset">Y offset within the pixel [0;1]</param>
        /// <returns>Ray endpoint</returns>
        Vec3? GetRayEnd(int x, int y, float xOffset = .5f, float yOffset = .5f);


        /// <summary>
        /// Get the eye position (where rays originate from)
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <param name="xOffset">X offset within the pixel [0;1]</param>
        /// <param name="yOffset">Y offset within the pixel [0;1]</param>
        /// <returns>Ray start point</returns>
        Vec3 GetEye(int x, int y, float xOffset = .5f, float yOffset = .5f);
    }
}
