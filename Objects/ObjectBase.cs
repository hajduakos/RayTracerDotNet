using RayTracer.Common;

namespace RayTracer.Objects
{
    /// <summary>
    /// Base class for objects
    /// </summary>
    public abstract class ObjectBase
    {
        /// <summary>
        /// Material of the object
        /// </summary>
        public Material Material { get; }

        public ObjectBase(Material material)
        {
            this.Material = material;
        }

        /// <summary>
        /// Intersect object with a given ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Intersection data or null (if ray does not intersect with object)</returns>
        public abstract Intersection Intersect(Ray ray);
    }
}
