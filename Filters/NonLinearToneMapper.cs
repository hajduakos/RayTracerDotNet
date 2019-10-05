using RayTracer.Common;
using RayTracer.Composition;
using System.Threading.Tasks;

namespace RayTracer.Filters
{
    class NonLinearToneMapper : IToneMapper
    {
        public void ToneMap(RawImage image)
        {
            Color one = new Color(1, 1, 1);
            for (int x = 0; x < image.Width; ++x)
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] / (image[x, y] + one));
        }
    }
}
