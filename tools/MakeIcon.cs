using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

/// <summary>
/// Builds assets/habbo.ico from assets/logo.png.
/// Every entry is stored as a 32bpp BMP (DIB) with an AND mask, which every
/// Windows and .NET icon API can read (PNG-compressed entries are not).
/// Usage: MakeIcon <logo.png> <out.ico> [previewDir]
/// </summary>
internal static class MakeIcon
{
    static readonly int[] Sizes = new int[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 };

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: MakeIcon <logo.png> <out.ico> [previewDir]");
            return 2;
        }
        string src = args[0], dst = args[1];
        string preview = args.Length > 2 ? args[2] : null;

        using (Image logo = Image.FromFile(src))
        {
            Console.WriteLine("source: {0}x{1}", logo.Width, logo.Height);
            List<byte[]> blobs = new List<byte[]>();
            List<int> sizes = new List<int>();

            foreach (int s in Sizes)
            {
                using (Bitmap b = Scale(logo, s))
                {
                    blobs.Add(Dib(b));
                    sizes.Add(s);
                    if (preview != null)
                    {
                        b.Save(Path.Combine(preview, "icon_" + s + ".png"), ImageFormat.Png);
                    }
                    Console.WriteLine("  {0}x{0} -> {1} bytes", s, blobs[blobs.Count - 1].Length);
                }
            }

            int count = sizes.Count;
            int offset = 6 + 16 * count;
            using (FileStream fs = new FileStream(dst, FileMode.Create, FileAccess.Write))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                w.Write((short)0);
                w.Write((short)1);
                w.Write((short)count);
                for (int i = 0; i < count; i++)
                {
                    int s = sizes[i];
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
            Console.WriteLine("wrote " + Path.GetFileName(dst) + " (" + new FileInfo(dst).Length + " bytes)");
        }
        return 0;
    }

    /// <summary>Progressive halving keeps small sizes crisp instead of mushy.</summary>
    static Bitmap Scale(Image src, int size)
    {
        Bitmap cur = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(cur))
        {
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));
        }
        while (cur.Width / 2 >= size)
        {
            int nw = Math.Max(size, cur.Width / 2);
            Bitmap half = Resize(cur, nw);
            cur.Dispose();
            cur = half;
        }
        if (cur.Width != size)
        {
            Bitmap exact = Resize(cur, size);
            cur.Dispose();
            cur = exact;
        }
        return cur;
    }

    static Bitmap Resize(Image src, int size)
    {
        Bitmap b = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(b))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(src, new Rectangle(0, 0, size, size));
        }
        return b;
    }

    /// <summary>32bpp bottom-up DIB with the required AND mask.</summary>
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
