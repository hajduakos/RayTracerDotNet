using RayTracer.Common;
using RayTracer.Composition;
using System;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    class SchlickToneMapper : IToneMapper
    {
        public void ToneMap(RawImage image)
        {
            float imax = 0;
            float imin = image[0, 0].R;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => imax = Math.Max(imax, Math.Max(image[x, y].R, Math.Max(image[x, y].G, image[x, y].B))));
                Parallel.For(0, image.Height, y => imin = Math.Min(imin, Math.Min(image[x, y].R, Math.Min(image[x, y].G, image[x, y].B))));
            }
            float p = (imax - imin) / (255 * imin - imin);
            Color i = new Color(imax, imax, imax);
            for (int x = 0; x < image.Width; ++x)
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] * p / (image[x, y] * (p - 1) - i));
        }
    }
}
