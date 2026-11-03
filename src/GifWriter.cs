using System;
using System.Collections.Generic;
using System.IO;

namespace HabboBadgeCreator
{
    /// <summary>
    /// Writes a still GIF89a from 32-bit ARGB pixels. GIF has no partial transparency and at
    /// most 256 palette entries, so one slot is reserved for transparent pixels (everything
    /// below <see cref="AlphaCutoff"/>) and badges with more than 255 colours are reduced
    /// with median-cut quantization first. Kept free of System.Drawing so the writer can be
    /// exercised without a window: the bitmap overload lives in <see cref="Store"/>.
    /// </summary>
    internal static class GifWriter
    {
        /// <summary>Pixels with less alpha than this become the transparent index.</summary>
        public const int AlphaCutoff = 128;

        /// <summary>Colour slots left for the artwork: 256 entries minus the transparency.</summary>
        public const int MaxColors = 255;

        /// <summary>Palette slot reserved for the transparent pixels.</summary>
        public const int TransparentIndex = 0;

        class ColorCount
        {
            public int Rgb;
            public int R, G, B;
            public int Count;
        }

        /// <summary>A median-cut box: a group of colours that will share one palette entry.</summary>
        class Box
        {
            public readonly List<ColorCount> Colors = new List<ColorCount>();
            public int RMin, RMax, GMin, GMax, BMin, BMax;
            public long Weight;

            public void Measure()
            {
                RMin = GMin = BMin = 255;
                RMax = GMax = BMax = 0;
                Weight = 0;
                foreach (ColorCount c in Colors)
                {
                    if (c.R < RMin) RMin = c.R;
                    if (c.R > RMax) RMax = c.R;
                    if (c.G < GMin) GMin = c.G;
                    if (c.G > GMax) GMax = c.G;
                    if (c.B < BMin) BMin = c.B;
                    if (c.B > BMax) BMax = c.B;
                    Weight += c.Count;
                }
            }

            /// <summary>The widest colour axis and how wide it is on this box.</summary>
            public int LongestSide(out int channel)
            {
                int dr = RMax - RMin, dg = GMax - GMin, db = BMax - BMin;
                if (dr >= dg && dr >= db) { channel = 0; return dr; }
                if (dg >= db) { channel = 1; return dg; }
                channel = 2;
                return db;
            }
        }

        static int Channel(ColorCount c, int channel)
        {
            return channel == 0 ? c.R : channel == 1 ? c.G : c.B;
        }

        /// <summary>Splits one box at the weighted median of its widest axis and returns the new half.</summary>
        static Box Split(Box box, int channel)
        {
            box.Colors.Sort(delegate(ColorCount a, ColorCount b)
            {
                return Channel(a, channel).CompareTo(Channel(b, channel));
            });

            long half = box.Weight / 2, run = 0;
            int cut = 1;
            for (int i = 0; i < box.Colors.Count - 1; i++)
            {
                run += box.Colors[i].Count;
                cut = i + 1;
                if (run >= half) break;
            }
            if (cut < 1) cut = 1;
            if (cut >= box.Colors.Count) cut = box.Colors.Count - 1;

            Box right = new Box();
            right.Colors.AddRange(box.Colors.GetRange(cut, box.Colors.Count - cut));
            box.Colors.RemoveRange(cut, box.Colors.Count - cut);
            box.Measure();
            right.Measure();
            return right;
        }

        /// <summary>Median-cut palette for the colours of a busy badge.</summary>
        static List<int[]> Quantize(List<ColorCount> colors, int max)
        {
            List<Box> boxes = new List<Box>();
            Box first = new Box();
            first.Colors.AddRange(colors);
            first.Measure();
            boxes.Add(first);

            while (boxes.Count < max)
            {
                int pick = -1, widest = 0;
                for (int i = 0; i < boxes.Count; i++)
                {
                    int channel;
                    int range = boxes[i].LongestSide(out channel);
                    if (boxes[i].Colors.Count < 2 || range <= 0) continue;
                    if (range > widest) { widest = range; pick = i; }
                }
                if (pick < 0) break;

                int axis;
                boxes[pick].LongestSide(out axis);
                boxes.Add(Split(boxes[pick], axis));
            }

            List<int[]> palette = new List<int[]>();
            foreach (Box b in boxes)
            {
                long r = 0, g = 0, bl = 0, weight = 0;
                foreach (ColorCount c in b.Colors)
                {
                    r += (long)c.R * c.Count;
                    g += (long)c.G * c.Count;
                    bl += (long)c.B * c.Count;
                    weight += c.Count;
                }
                if (weight <= 0) weight = 1;
                palette.Add(new int[] { (int)(r / weight), (int)(g / weight), (int)(bl / weight) });
            }
            if (palette.Count == 0) palette.Add(new int[] { 0, 0, 0 });
            return palette;
        }

        /// <summary>Closest palette entry (the transparent slot is never a candidate).</summary>
        static int Nearest(List<int[]> palette, int r, int g, int b)
        {
            if (palette.Count <= 1) return TransparentIndex;
            int best = 1, bestDistance = int.MaxValue;
            for (int i = 1; i < palette.Count; i++)
            {
                int dr = palette[i][0] - r, dg = palette[i][1] - g, db = palette[i][2] - b;
                int distance = dr * dr + dg * dg + db * db;
                if (distance < bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        /// <summary>Encodes ARGB pixels into a complete GIF89a file.</summary>
        public static byte[] Encode(int[] pixels, int width, int height)
        {
            if (pixels == null) throw new ArgumentNullException("pixels");
            if (width <= 0 || height <= 0 || width > 65535 || height > 65535)
                throw new ArgumentOutOfRangeException("width", "GIF needs a size between 1 and 65535 pixels.");
            if (pixels.Length < width * height)
                throw new ArgumentException("Not enough pixels for this size.", "pixels");

            int total = width * height;

            // every opaque colour and how often it is used
            Dictionary<int, int> counts = new Dictionary<int, int>();
            for (int i = 0; i < total; i++)
            {
                int p = pixels[i];
                if (((p >> 24) & 0xFF) < AlphaCutoff) continue;
                int rgb = p & 0x00FFFFFF;
                int seen;
                counts.TryGetValue(rgb, out seen);
                counts[rgb] = seen + 1;
            }

            List<ColorCount> unique = new List<ColorCount>(counts.Count);
            foreach (KeyValuePair<int, int> pair in counts)
            {
                ColorCount c = new ColorCount();
                c.Rgb = pair.Key;
                c.R = (pair.Key >> 16) & 0xFF;
                c.G = (pair.Key >> 8) & 0xFF;
                c.B = pair.Key & 0xFF;
                c.Count = pair.Value;
                unique.Add(c);
            }

            // palette: slot 0 is the transparent colour, the rest is the artwork
            List<int[]> palette = new List<int[]>();
            palette.Add(new int[] { 0, 0, 0 });
            Dictionary<int, int> lookup = new Dictionary<int, int>();

            if (unique.Count <= MaxColors)
            {
                // few enough colours to keep every one of them, so the export is pixel exact
                foreach (ColorCount c in unique)
                {
                    lookup[c.Rgb] = palette.Count;
                    palette.Add(new int[] { c.R, c.G, c.B });
                }
            }
            else
            {
                palette.AddRange(Quantize(unique, MaxColors));
            }

            byte[] indices = new byte[total];
            for (int i = 0; i < total; i++)
            {
                int p = pixels[i];
                if (((p >> 24) & 0xFF) < AlphaCutoff) { indices[i] = TransparentIndex; continue; }

                int rgb = p & 0x00FFFFFF;
                int index;
                if (!lookup.TryGetValue(rgb, out index))
                {
                    index = Nearest(palette, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
                    lookup[rgb] = index;
                }
                indices[i] = (byte)index;
            }

            int sizeBits = 1;
            while ((1 << sizeBits) < palette.Count && sizeBits < 8) sizeBits++;
            int tableSize = 1 << sizeBits;

            using (MemoryStream s = new MemoryStream())
            {
                s.WriteByte((byte)'G'); s.WriteByte((byte)'I'); s.WriteByte((byte)'F');
                s.WriteByte((byte)'8'); s.WriteByte((byte)'9'); s.WriteByte((byte)'a');
                WriteShort(s, width);
                WriteShort(s, height);

                // global colour table present, 8-bit colour resolution, table size
                s.WriteByte((byte)(0x80 | 0x70 | (sizeBits - 1)));
                s.WriteByte((byte)TransparentIndex);   // background colour
                s.WriteByte(0);                        // pixel aspect ratio
                for (int i = 0; i < tableSize; i++)
                {
                    if (i < palette.Count)
                    {
                        s.WriteByte((byte)palette[i][0]);
                        s.WriteByte((byte)palette[i][1]);
                        s.WriteByte((byte)palette[i][2]);
                    }
                    else
                    {
                        s.WriteByte(0); s.WriteByte(0); s.WriteByte(0);
                    }
                }

                // graphic control extension: transparent index, no animation delay
                s.WriteByte(0x21); s.WriteByte(0xF9); s.WriteByte(0x04);
                s.WriteByte(0x01);
                WriteShort(s, 0);
                s.WriteByte((byte)TransparentIndex);
                s.WriteByte(0);

                // image descriptor: full canvas, no local palette, not interlaced
                s.WriteByte(0x2C);
                WriteShort(s, 0); WriteShort(s, 0);
                WriteShort(s, width); WriteShort(s, height);
                s.WriteByte(0);

                WriteLzw(s, indices, Math.Max(2, sizeBits));
                s.WriteByte(0x3B);                     // trailer
                return s.ToArray();
            }
        }

        /// <summary>Writes an encoded GIF to disk.</summary>
        public static void Save(int[] pixels, int width, int height, string path)
        {
            File.WriteAllBytes(path, Encode(pixels, width, height));
        }

        static void WriteShort(Stream s, int value)
        {
            s.WriteByte((byte)(value & 0xFF));
            s.WriteByte((byte)((value >> 8) & 0xFF));
        }

        /// <summary>
        /// GIF LZW with the code width growing one step behind the table, which is what
        /// decoders expect: after a code is written, a table that has outgrown the current
        /// width moves to the next one. A full table emits a clear code and starts again.
        /// </summary>
        static void WriteLzw(Stream target, byte[] indices, int minCodeSize)
        {
            target.WriteByte((byte)minCodeSize);

            int clear = 1 << minCodeSize;
            int end = clear + 1;
            int next = end + 1;
            int codeSize = minCodeSize + 1;
            int maxCode = (1 << codeSize) - 1;

            Dictionary<int, int> table = new Dictionary<int, int>();
            using (MemoryStream bits = new MemoryStream())
            {
                int acc = 0, accBits = 0;
                Emit(bits, ref acc, ref accBits, clear, codeSize);

                int prefix = indices[0];
                for (int i = 1; i < indices.Length; i++)
                {
                    int k = indices[i];
                    int key = (prefix << 8) | k;
                    int code;
                    if (table.TryGetValue(key, out code))
                    {
                        prefix = code;
                        continue;
                    }

                    Emit(bits, ref acc, ref accBits, prefix, codeSize);
                    if (next > maxCode && codeSize < 12)
                    {
                        codeSize++;
                        maxCode = (1 << codeSize) - 1;
                    }

                    if (next < 4096)
                    {
                        table[key] = next++;
                    }
                    else
                    {
                        Emit(bits, ref acc, ref accBits, clear, codeSize);
                        table.Clear();
                        next = end + 1;
                        codeSize = minCodeSize + 1;
                        maxCode = (1 << codeSize) - 1;
                    }
                    prefix = k;
                }

                Emit(bits, ref acc, ref accBits, prefix, codeSize);
                if (next > maxCode && codeSize < 12)
                {
                    codeSize++;
                    maxCode = (1 << codeSize) - 1;
                }
                Emit(bits, ref acc, ref accBits, end, codeSize);
                Flush(bits, ref acc, ref accBits);

                byte[] raw = bits.ToArray();
                for (int i = 0; i < raw.Length; i += 255)
                {
                    int n = Math.Min(255, raw.Length - i);
                    target.WriteByte((byte)n);
                    target.Write(raw, i, n);
                }
                target.WriteByte(0); // end of the sub-block chain
            }
        }

        static void Emit(Stream bits, ref int acc, ref int accBits, int code, int codeSize)
        {
            acc |= code << accBits;
            accBits += codeSize;
            while (accBits >= 8)
            {
                bits.WriteByte((byte)(acc & 0xFF));
                acc >>= 8;
                accBits -= 8;
            }
        }

        static void Flush(Stream bits, ref int acc, ref int accBits)
        {
            if (accBits <= 0) return;
            bits.WriteByte((byte)(acc & 0xFF));
            acc = 0;
            accBits = 0;
        }
    }
}
