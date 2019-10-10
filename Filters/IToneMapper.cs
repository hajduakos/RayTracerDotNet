using RayTracer.Composition;
using RayTracer.Reporting;

namespace RayTracer.Filters
{
    public interface IToneMapper
    {
        void ToneMap(RawImage image);

        IReporter Reporter { get; set; }
    }
}
