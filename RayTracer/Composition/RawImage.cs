using RayTracer.Common;
using System;
using System.Diagnostics.Contracts;
using System.IO;

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

        public RawImage(int width, int height)
        {
            Contract.Requires(width > 0, "Width must be greater than 0");
            Contract.Requires(height > 0, "Height must be greater than 0");
            this.Width = width;
            this.Height = height;
            this.img = new Color[width, height];
        }

        /// <summary>
        /// Get the color for a given pixel
        /// </summary>
        public Color this[int x, int y]
        {
            get { return img[x, y]; }
            set { img[x, y] = value; }
        }

        // ----------------------------------------------------------------------
        // BMP codec below
        // ----------------------------------------------------------------------

        private const uint BMP_HEADER_SIZE = 14;
        private const uint DIB_HEADER_SIZE = 40;
        private const uint RESOLUTION = 2835; // 72 DPI converted to pixels/metre

        private uint GetRowSize()
        {
            uint w = (uint)Width * 3;
            if (w % 4 == 0) return w;
            return w + 4 - (w % 4);
        }

        private static void WriteLittleEndianBytes(Stream stream, byte[] bytes)
        {
            if (!System.BitConverter.IsLittleEndian) System.Array.Reverse(bytes);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteUint(Stream stream, uint i) =>
            WriteLittleEndianBytes(stream, System.BitConverter.GetBytes(i));

        private static void WriteUshort(Stream stream, ushort i) =>
            WriteLittleEndianBytes(stream, System.BitConverter.GetBytes(i));

        private static void WriteZeros(Stream stream, int n)
        {
            for (int i = 0; i < n; i++) stream.WriteByte(0);
        }

        private void WriteBMPHeader(Stream stream)
        {
            stream.WriteByte(0x42); // B (1 byte)
            stream.WriteByte(0x4D); // M (1 byte)
            uint size = BMP_HEADER_SIZE + DIB_HEADER_SIZE + (uint)Height * GetRowSize();
            WriteUint(stream, size); // Total size of file (4 bytes)
            WriteZeros(stream, 2); // Unused app specific (2 bytes)
            WriteZeros(stream, 2); // Unused app specific (2 bytes)
            WriteUint(stream, BMP_HEADER_SIZE + DIB_HEADER_SIZE); // Offset to pixel array (4 bytes)
        }

        private void WriteDIBHeader(Stream stream)
        {
            WriteUint(stream, DIB_HEADER_SIZE); // DIB header size (4 bytes)
            WriteUint(stream, (uint)Width); // Width (4 bytes)
            WriteUint(stream, (uint)Height); // Height, positive for bottom to top order (4 bytes)
            WriteUshort(stream, 1); // Color planes: 1 (2 bytes)
            WriteUshort(stream, 24); // Bits per pixel (2 bytes)
            WriteZeros(stream, 4); // BI_RGB, no compression (4 bytes)
            WriteUint(stream, (uint)Height * GetRowSize()); // Size of pixel array (4 bytes)
            WriteUint(stream, RESOLUTION); // Horizontal resolution (4 bytes)
            WriteUint(stream, RESOLUTION); // Vertical resolution (4 bytes)
            WriteZeros(stream, 4); // 0 colors in palette (4 bytes)
            WriteZeros(stream, 4); // 0 important colors (4 bytes)
        }

        private void WritePixelArray(Stream stream)
        {
            int padding = (int)GetRowSize() - Width * 3;
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // BGR order
                    stream.WriteByte((byte)Math.Clamp((int)(img[x, y].B * 255), 0, 255));
                    stream.WriteByte((byte)Math.Clamp((int)(img[x, y].G * 255), 0, 255));
                    stream.WriteByte((byte)Math.Clamp((int)(img[x, y].R * 255), 0, 255));
                }
                WriteZeros(stream, padding);
            }
        }

        /// <summary>
        /// Write to a bitmap file.
        /// </summary>
        /// <param name="filename">Path to file</param>
        public void WriteToFile(string filename)
        {
            using Stream stream = File.OpenWrite(filename);
            WriteBMPHeader(stream);
            WriteDIBHeader(stream);
            WritePixelArray(stream);
        }
    }
}
