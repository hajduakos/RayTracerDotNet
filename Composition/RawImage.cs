using RayTracer.Common;
using System;
using System.Threading.Tasks;

namespace RayTracer.Composition
{
    public class RawImage
    {
        public int W { get; }
        public int H { get; }

        private readonly Color[,] img;

        public RawImage(int w, int h)
        {
            this.W = w;
            this.H = h;
            this.img = new Color[w, h];
        }

        public Color this[int x, int y]
        {
            get { return img[x, y]; }
            set { img[x, y] = value; }
        }

        public void ToneMap()
        {
            float max = 0;
            for(int x = 0; x < W; ++x)
                Parallel.For(0, H, y => max = Math.Max(max, img[x, y].Lum));

            float div = 1 / max;
            for (int x = 0; x < W; ++x)
                Parallel.For(0, H, y => img[x, y] = img[x, y] * div);
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(W, H);
            for (int x = 0; x < W; ++x)
                for (int y = 0; y < H; ++y)
                    bmp.SetPixel(x, H - y - 1, System.Drawing.Color.FromArgb(
                        Math.Clamp((int)(img[x, y].R * 255), 0, 255),
                        Math.Clamp((int)(img[x, y].G * 255), 0, 255),
                        Math.Clamp((int)(img[x, y].B * 255), 0, 255)));
            return bmp;
        }
    }
}
