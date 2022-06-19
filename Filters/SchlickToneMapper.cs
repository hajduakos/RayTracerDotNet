using RayTracer.Common;
using RayTracer.Composition;
using RayTracer.Reporting;
using System;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    public class SchlickToneMapper : IToneMapper
    {
        public IReporter Reporter { get; set; }
        public void ToneMap(RawImage image)
        {
            Reporter?.Restart("Tone mapping");
            float imax = 0;
            float imin = image[0, 0].R;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => imax = Math.Max(imax, Math.Max(image[x, y].R, Math.Max(image[x, y].G, image[x, y].B))));
                Parallel.For(0, image.Height, y => imin = Math.Min(imin, Math.Min(image[x, y].R, Math.Min(image[x, y].G, image[x, y].B))));
                Reporter?.Report(x, image.Width * 2 - 1, "Tone mapping");
            }
            float p = (imax - imin) / (255 * imin - imin);
            Color i = new(imax, imax, imax);
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] * p / (image[x, y] * (p - 1) - i));
                Reporter?.Report(image.Width + x, image.Width * 2 - 1, "Tone mapping");
            }
            Reporter?.End("Tone mapping");
        }
    }
}
