using RayTracer.Common;

namespace RayTracer.Objects
{
    /// <summary>
    /// Base interface for objects
    /// </summary>
    public interface IObject
    {
        /// <summary>
        /// Intersect object with a given ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Intersection data or null (if ray does not intersect with object)</returns>
        public abstract Intersection Intersect(Ray ray);
    }
}
