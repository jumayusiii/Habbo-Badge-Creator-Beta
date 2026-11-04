using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace HabboBadgeCreator
{
    /// <summary>
    /// Writes the 40 x 40 badge as an SVG. Every horizontal run of one colour becomes a single
    /// rect, so a flat badge stays a few hundred bytes and the file still scales without blur.
    /// </summary>
    internal static class SvgWriter
    {
        public static void Save(int[] pixels, int w, int h, string path)
        {
            StringBuilder sb = new StringBuilder(4096);
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"").Append(w)
              .Append("\" height=\"").Append(h).Append("\" viewBox=\"0 0 ")
              .Append(w).Append(' ').Append(h).Append("\" shape-rendering=\"crispEdges\">\n");
            for (int y = 0; y < h; y++)
            {
                int x = 0;
                while (x < w)
                {
                    int argb = pixels[y * w + x];
                    int run = 1;
                    while (x + run < w && pixels[y * w + x + run] == argb) run++;
                    if (((argb >> 24) & 0xFF) != 0) sb.Append(Rect(x, y, run, argb));
                    x += run;
                }
            }
            sb.Append("</svg>\n");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        static string Rect(int x, int y, int w, int argb)
        {
            int a = (argb >> 24) & 0xFF;
            string hex = "#"
                + ((argb >> 16) & 0xFF).ToString("X2", CultureInfo.InvariantCulture)
                + ((argb >> 8) & 0xFF).ToString("X2", CultureInfo.InvariantCulture)
                + (argb & 0xFF).ToString("X2", CultureInfo.InvariantCulture);
            return "<rect x=\"" + x + "\" y=\"" + y + "\" width=\"" + w + "\" height=\"1\" fill=\"" + hex + "\""
                + (a == 255 ? "" : " fill-opacity=\"" + Math.Round(a / 255.0, 3).ToString("0.###", CultureInfo.InvariantCulture) + "\"")
                + "/>\n";
        }
    }

    /// <summary>
    /// Writes a Windows icon with 16, 32, 48 and 64 px entries. Every entry is a 32bpp bottom-up
    /// DIB with the required AND mask, which every Windows and .NET icon API can read.
    /// </summary>
    internal static class IcoWriter
    {
        static readonly int[] Sizes = new int[] { 16, 32, 48, 64 };

        public static void Save(Bitmap bmp, string path)
        {
            List<byte[]> blobs = new List<byte[]>();
            foreach (int s in Sizes)
            {
                using (Bitmap scaled = Scale(bmp, s)) blobs.Add(Dib(scaled));
            }

            int offset = 6 + 16 * Sizes.Length;
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                w.Write((short)0);
                w.Write((short)1);
                w.Write((short)Sizes.Length);
                for (int i = 0; i < Sizes.Length; i++)
                {
                    int s = Sizes[i];
                    w.Write((byte)(s >= 256 ? 0 : s));
                    w.Write((byte)(s >= 256 ? 0 : s));
                    w.Write((byte)0);
                    w.Write((byte)0);
                    w.Write((short)1);
                    w.Write((short)32);
                    w.Write(blobs[i].Length);
                    w.Write(offset);
                    offset += blobs[i].Length;
                }
                foreach (byte[] b in blobs) w.Write(b);
            }
        }

        /// <summary>Nearest neighbour keeps pixel art crisp at every icon size.</summary>
        static Bitmap Scale(Bitmap src, int size)
        {
            Bitmap b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawImage(src, new Rectangle(0, 0, size, size));
            }
            return b;
        }

        /// <summary>32bpp bottom-up DIB with the AND mask an .ico entry requires.</summary>
        static byte[] Dib(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            int stride = ((w + 31) / 32) * 4;
            int px = w * h * 4;
            byte[] data = new byte[40 + px + stride * h];
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(40);
                bw.Write(w);
                bw.Write(h * 2);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(0);
                bw.Write(px + stride * h);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);

                Color[] row = new Color[w];
                for (int y = h - 1; y >= 0; y--)
                {
                    for (int x = 0; x < w; x++) row[x] = b.GetPixel(x, y);
                    for (int x = 0; x < w; x++)
                    {
                        bw.Write(row[x].B);
                        bw.Write(row[x].G);
                        bw.Write(row[x].R);
                        bw.Write(row[x].A);
                    }
                }
                for (int y = h - 1; y >= 0; y--)
                {
                    byte[] mask = new byte[stride];
                    for (int x = 0; x < w; x++)
                        if (b.GetPixel(x, y).A < 128) mask[x / 8] |= (byte)(1 << (7 - x % 8));
                    bw.Write(mask);
                }
            }
            return data;
        }
    }
}
