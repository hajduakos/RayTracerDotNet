using RayTracer.Composition;
using System;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    public class MaxLinearToneMapper : IToneMapper
    {
        public void ToneMap(RawImage image)
        {
            float max = 0;
            for (int x = 0; x < image.Width; ++x)
                Parallel.For(0, image.Height, y => max = Math.Max(max, Math.Max(image[x, y].R, Math.Max(image[x, y].G, image[x, y].B))));

            float div = 1 / max;
            for (int x = 0; x < image.Width; ++x)
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] * div);
        }
    }
}
