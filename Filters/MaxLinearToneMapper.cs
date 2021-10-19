using RayTracer.Composition;
using RayTracer.Reporting;
using System;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    public class MaxLinearToneMapper : IToneMapper
    {
        public IReporter Reporter { get; set; }

        public void ToneMap(RawImage image)
        {
            Reporter?.Restart("Tone mapping");
            float max = 0;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y =>
                max = Math.Max(max, Math.Max(NoNaN(image[x, y].R), Math.Max(NoNaN(image[x, y].G), NoNaN(image[x, y].B)))));
                Reporter?.Report(x, image.Width * 2 - 1, "Tone mapping");
            }

            float div = 1 / max;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] * div);
                Reporter?.Report(image.Width + x, image.Width * 2 - 1, "Tone mapping");
            }
            Reporter?.End("Tone mapping");
        }

        private float NoNaN(float f)
        {
            if (Single.IsNaN(f)) return 0;
            return f;
        }
    }
}
