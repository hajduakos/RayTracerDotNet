using RayTracer.Common;

namespace RayTracer.Objects
{
    public abstract class ObjectBase
    {
        public Material Material { get; }

        public ObjectBase(Material material)
        {
            this.Material = material;
        }

        public abstract Intersection Intersect(Ray ray);
    }
}
