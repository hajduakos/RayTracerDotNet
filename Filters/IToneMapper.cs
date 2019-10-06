using RayTracer.Composition;

namespace RayTracer.Filters
{
    public interface IToneMapper
    {
        void ToneMap(RawImage image);
    }
}
