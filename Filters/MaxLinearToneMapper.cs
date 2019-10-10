﻿using RayTracer.Composition;
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
            if (Reporter != null) Reporter.Restart("Tone mapping");
            float max = 0;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => max = Math.Max(max, Math.Max(image[x, y].R, Math.Max(image[x, y].G, image[x, y].B))));
                if (Reporter != null) Reporter.Report(x, image.Width * 2 - 1, "Tone mapping");
            }

            float div = 1 / max;
            for (int x = 0; x < image.Width; ++x)
            {
                Parallel.For(0, image.Height, y => image[x, y] = image[x, y] * div);
                if (Reporter != null) Reporter.Report(image.Width + x, image.Width * 2 - 1, "Tone mapping");
            }
            if (Reporter != null) Reporter.End("Tone mapping");
        }
    }
}
