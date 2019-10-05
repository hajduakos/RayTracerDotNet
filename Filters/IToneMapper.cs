using RayTracer.Composition;
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer.Filters
{
    public interface IToneMapper
    {
        void ToneMap(RawImage image);
    }
}
