using RayTracer.Common;
using RayTracer.Filters;
using System;
using System.Threading.Tasks;

namespace RayTracer.Composition
{
    /// <summary>
    /// Raw image
    /// </summary>
    public sealed class RawImage
    {
        public int Width { get; }
        public int Height { get; }

        private readonly Color[,] img; // Internal image

        public RawImage(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            this.img = new Color[w, h];
        }

        /// <summary>
        /// Get the color for a given pixel
        /// </summary>
        public Color this[int x, int y]
        {
            get { return img[x, y]; }
            set { img[x, y] = value; }
        }

        /// <summary>
        /// Perform tone mapping, the original image is modified
        /// </summary>
        public void ToneMap()
        {
            new NonLinearToneMapper().ToneMap(this);
            new MaxLinearToneMapper().ToneMap(this);
        }

        /// <summary>
        /// Convert to a bitmap
        /// </summary>
        /// <returns>Bitmap</returns>
        public System.Drawing.Bitmap ToBitmap()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Width, Height);
            // Map [0,1] float to [0,255] integer
            for (int x = 0; x < Width; ++x)
                for (int y = 0; y < Height; ++y)
                    bmp.SetPixel(x, Height - y - 1, System.Drawing.Color.FromArgb(
                        Math.Clamp((int)(img[x, y].R * 255), 0, 255),
                        Math.Clamp((int)(img[x, y].G * 255), 0, 255),
                        Math.Clamp((int)(img[x, y].B * 255), 0, 255)));
            return bmp;
        }
    }
}
