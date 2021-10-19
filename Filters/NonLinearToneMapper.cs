using RayTracer.Common;
using RayTracer.Composition;
using RayTracer.Reporting;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    public class NonLinearToneMapper : IToneMapper
    {
        public IReporter Reporter { get; set; }

        private readonly float p;

        public NonLinearToneMapper(float p = 1)
        {
            this.p = p;
        }
        public void ToneMap(RawImage image)
        {
            Reporter?.Restart("Tone mapping");
            Color one = new Color(1, 1, 1);
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] / (image[x, y] * p + one));
                Reporter?.Report(x, image.Width - 1, "Tone mapping");
            }
            Reporter?.End("Tone mapping");
        }
    }
}