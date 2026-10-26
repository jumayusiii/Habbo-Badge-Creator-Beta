using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>
    /// A 40 x 40 ARGB pixel document with undo history. Drawing happens on the active
    /// layer; what you see (and what is saved or exported) is the source-over composite
    /// of every visible layer, bottom to top.
    /// </summary>
    internal class PixelDoc
    {
        public const int N = 40;
        public const int Count = N * N;
        public const int MaxLayers = 5;

        /// <summary>How a layer's pixels combine with what is below it.</summary>
        internal enum LayerBlend { Normal, Multiply, Screen }

        internal class Layer
        {
            public int[] Px = new int[Count];
            public bool Visible = true;
            public bool Locked;
            public int Opacity = 255;             // 0 = invisible, 255 = fully opaque
            public LayerBlend Mode = LayerBlend.Normal;
            public string Name = "Layer";
        }

        class Snapshot
        {
            public int[][] Px;
            public bool[] Vis;
            public bool[] Lock;
            public int[] Op;
            public LayerBlend[] Mode;
            public string[] Name;
            public int Active;
        }

        readonly List<Layer> layers = new List<Layer>();
        readonly List<Snapshot> undo = new List<Snapshot>();
        readonly List<Snapshot> redo = new List<Snapshot>();
        const int HistoryLimit = 80;

        int[] px;                 // pixels of the active layer
        int active;
        int layerNo;              // source of the default layer names
        int[] flat;               // cached composite
        int flatVersion = -1;

        public event Action Changed;

        public int Version { get; private set; }

        public PixelDoc()
        {
            layers.Add(NewLayer());
            px = layers[0].Px;
            Touch();
        }

        Layer NewLayer()
        {
            Layer l = new Layer();
            l.Name = "Layer " + (++layerNo);
            return l;
        }

        void Seed(int value)
        {
            for (int i = 0; i < Count; i++) px[i] = value;
        }

        void Reseed(int value)
        {
            Seed(value);
            Touch();
        }

        /// <summary>Marks the document as changed so every view repaints.</summary>
        public void Touch()
        {
            flatVersion = -1;
            Version++;
            if (Changed != null) Changed();
        }

        public int this[int x, int y]
        {
            get
            {
                if (x < 0 || y < 0 || x >= N || y >= N) return 0;
                return px[y * N + x];
            }
        }

        void RawSet(int x, int y, int argb)
        {
            if (x < 0 || y < 0 || x >= N || y >= N) return;
            px[y * N + x] = argb;
        }

        public void Set(int x, int y, int argb)
        {
            if (ActiveLocked || !InBounds(x, y)) return;
            RawSet(x, y, argb);
            Touch();
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < N && y < N;
        }

        public void Clear()
        {
            if (ActiveLocked) return;
            PushUndo();
            Seed(0);
            Touch();
        }

        // ------------------------------------------------------------ history
        Snapshot Capture()
        {
            Snapshot s = new Snapshot();
            s.Px = new int[layers.Count][];
            s.Vis = new bool[layers.Count];
            s.Lock = new bool[layers.Count];
            s.Op = new int[layers.Count];
            s.Mode = new LayerBlend[layers.Count];
            s.Name = new string[layers.Count];
            for (int i = 0; i < layers.Count; i++)
            {
                s.Px[i] = (int[])layers[i].Px.Clone();
                s.Vis[i] = layers[i].Visible;
                s.Lock[i] = layers[i].Locked;
                s.Op[i] = layers[i].Opacity;
                s.Mode[i] = layers[i].Mode;
                s.Name[i] = layers[i].Name;
            }
            s.Active = active;
            return s;
        }

        void Restore(Snapshot s)
        {
            while (layers.Count > s.Px.Length) layers.RemoveAt(layers.Count - 1);
            while (layers.Count < s.Px.Length) layers.Add(NewLayer());
            for (int i = 0; i < s.Px.Length; i++)
            {
                Array.Copy(s.Px[i], layers[i].Px, Count);
                layers[i].Visible = s.Vis[i];
                layers[i].Locked = s.Lock[i];
                layers[i].Opacity = s.Op[i];
                layers[i].Mode = s.Mode[i];
                layers[i].Name = s.Name[i];
            }
            active = Math.Max(0, Math.Min(layers.Count - 1, s.Active));
            px = layers[active].Px;
            Touch();
        }

        void PushUndo()
        {
            undo.Add(Capture());
            if (undo.Count > HistoryLimit) undo.RemoveAt(0);
            redo.Clear();
        }

        public bool CanUndo { get { return undo.Count > 0; } }

        /// <summary>True when the active layer refuses edits (Photoshop-style lock).</summary>
        public bool ActiveLocked { get { return layers[active].Locked; } }
        public bool CanRedo { get { return redo.Count > 0; } }

        public void Undo()
        {
            if (undo.Count == 0) return;
            Snapshot snap = undo[undo.Count - 1];
            undo.RemoveAt(undo.Count - 1);
            redo.Add(Capture());
            Restore(snap);
        }

        public void Redo()
        {
            if (redo.Count == 0) return;
            Snapshot snap = redo[redo.Count - 1];
            redo.RemoveAt(redo.Count - 1);
            undo.Add(Capture());
            Restore(snap);
        }

        /// <summary>Marks the start of an interactive stroke.</summary>
        public void BeginStroke()
        {
            PushUndo();
            InStroke = true;
        }

        /// <summary>Marks the end of an interactive stroke.</summary>
        public void EndStroke()
        {
            InStroke = false;
            Touch();
        }

        // ------------------------------------------------------------ drawing
        public void SetPixels(int x, int y, int argb, bool mirror)
        {
            if (ActiveLocked) return;
            RawSet(x, y, argb);
            if (mirror) RawSet(N - 1 - x, y, argb);
            Touch();
        }

        /// <summary>Bresenham line so fast mouse moves leave no gaps.</summary>
        public void StrokeLine(int x0, int y0, int x1, int y1, int argb, bool mirror)
        {
            if (ActiveLocked) return;
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                RawSet(x0, y0, argb);
                if (mirror) RawSet(N - 1 - x0, y0, argb);
                if (x0 == x1 && y0 == y1) break;
                int e2 = err * 2;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
            Touch();
        }

        public void FloodFill(int x, int y, int argb)
        {
            if (ActiveLocked || !InBounds(x, y)) return;
            int target = this[x, y];
            if (target == argb) return;
            Stack<int> stack = new Stack<int>();
            stack.Push(y * N + x);
            while (stack.Count > 0)
            {
                int idx = stack.Pop();
                if (px[idx] != target) continue;
                int cx = idx % N, cy = idx / N;
                px[idx] = argb;
                if (cx > 0) stack.Push(idx - 1);
                if (cx < N - 1) stack.Push(idx + 1);
                if (cy > 0) stack.Push(idx - N);
                if (cy < N - 1) stack.Push(idx + N);
            }
            Touch();
        }

        /// <summary>Mirrors every layer, left/right or top/bottom. Undoable.</summary>
        public void Flip(bool horizontal)
        {
            PushUndo();
            foreach (Layer l in layers)
            {
                int[] src = (int[])l.Px.Clone();
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                        l.Px[y * N + x] = horizontal ? src[y * N + (N - 1 - x)] : src[(N - 1 - y) * N + x];
            }
            Touch();
        }

        /// <summary>Rotates the whole artwork by 90 degrees. Undoable.</summary>
        public void Rotate(bool clockwise)
        {
            PushUndo();
            foreach (Layer l in layers)
            {
                int[] src = (int[])l.Px.Clone();
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        int sx = clockwise ? y : N - 1 - y;
                        int sy = clockwise ? N - 1 - x : x;
                        l.Px[y * N + x] = src[sy * N + sx];
                    }
            }
            Touch();
        }

        /// <summary>Shifts the active layer by whole pixels. Undoable.</summary>
        public void Nudge(int dx, int dy)
        {
            if (dx == 0 && dy == 0 || ActiveLocked) return;
            PushUndo();
            int[] src = (int[])px.Clone();
            Array.Clear(px, 0, Count);
            for (int y = 0; y < N; y++)
            {
                int sy = y - dy;
                if (sy < 0 || sy >= N) continue;
                for (int x = 0; x < N; x++)
                {
                    int sx = x - dx;
                    if (sx < 0 || sx >= N) continue;
                    px[y * N + x] = src[sy * N + sx];
                }
            }
            Touch();
        }

        // ------------------------------------------------------------ layers
        public int LayerCount { get { return layers.Count; } }

        public int ActiveLayer
        {
            get { return active; }
            set
            {
                int i = Math.Max(0, Math.Min(layers.Count - 1, value));
                if (i == active) return;
                active = i;
                px = layers[active].Px;
                Touch();
            }
        }

        public bool LayerVisible(int i)
        {
            return i >= 0 && i < layers.Count && layers[i].Visible;
        }

        public void SetLayerVisible(int i, bool on)
        {
            if (i < 0 || i >= layers.Count || layers[i].Visible == on) return;
            PushUndo();
            layers[i].Visible = on;
            Touch();
        }

        bool Valid(int i) { return i >= 0 && i < layers.Count; }

        public string LayerName(int i) { return Valid(i) ? layers[i].Name : ""; }
        public int LayerOpacity(int i) { return Valid(i) ? layers[i].Opacity : 255; }
        public bool LayerLocked(int i) { return Valid(i) && layers[i].Locked; }
        public LayerBlend LayerMode(int i) { return Valid(i) ? layers[i].Mode : LayerBlend.Normal; }

        /// <summary>Renames a layer. Undoable.</summary>
        public bool SetLayerName(int i, string name)
        {
            if (!Valid(i)) return false;
            string clean = (name ?? "").Trim();
            if (clean.Length > 22) clean = clean.Substring(0, 22);
            if (clean.Length == 0 || clean == layers[i].Name) return false;
            PushUndo();
            layers[i].Name = clean;
            Touch();
            return true;
        }

        /// <summary>Snapshots the document once, before a live drag (opacity slider).</summary>
        public void PushLayerUndo()
        {
            PushUndo();
        }

        /// <summary>Sets opacity during a drag, without pushing a snapshot per mouse move.</summary>
        public void SetLayerOpacityLive(int i, int opacity)
        {
            if (!Valid(i)) return;
            int a = Math.Max(0, Math.Min(255, opacity));
            if (a == layers[i].Opacity) return;
            layers[i].Opacity = a;
            Touch();
        }

        /// <summary>Sets a layer's opacity, 0…255. Undoable.</summary>
        public bool SetLayerOpacity(int i, int opacity)
        {
            if (!Valid(i)) return false;
            int a = Math.Max(0, Math.Min(255, opacity));
            if (a == layers[i].Opacity) return false;
            PushUndo();
            layers[i].Opacity = a;
            Touch();
            return true;
        }

        /// <summary>Locks or unlocks a layer. Undoable.</summary>
        public bool SetLayerLocked(int i, bool locked)
        {
            if (!Valid(i) || layers[i].Locked == locked) return false;
            PushUndo();
            layers[i].Locked = locked;
            Touch();
            return true;
        }

        /// <summary>Changes how a layer blends with the stack below. Undoable.</summary>
        public bool SetLayerMode(int i, LayerBlend mode)
        {
            if (!Valid(i) || layers[i].Mode == mode) return false;
            PushUndo();
            layers[i].Mode = mode;
            Touch();
            return true;
        }

        /// <summary>True between BeginStroke and EndStroke; views use it to defer work.</summary>
        public bool InStroke { get; private set; }

        public bool AddLayer()
        {
            if (layers.Count >= MaxLayers) return false;
            PushUndo();
            layers.Insert(active + 1, NewLayer());
            active++;
            px = layers[active].Px;
            Touch();
            return true;
        }

        public bool DeleteLayer()
        {
            if (layers.Count <= 1) return false;
            PushUndo();
            layers.RemoveAt(active);
            active = Math.Min(active, layers.Count - 1);
            px = layers[active].Px;
            Touch();
            return true;
        }

        /// <summary>Copies the active layer and puts the copy above it.</summary>
        public bool DuplicateLayer()
        {
            if (layers.Count >= MaxLayers) return false;
            PushUndo();
            Layer copy = NewLayer();
            Array.Copy(layers[active].Px, copy.Px, Count);
            copy.Visible = layers[active].Visible;
            copy.Opacity = layers[active].Opacity;
            copy.Mode = layers[active].Mode;
            string baseName = layers[active].Name;
            copy.Name = baseName.Length > 15 ? baseName.Substring(0, 15) + " copy" : baseName + " copy";
            layers.Insert(active + 1, copy);
            active++;
            px = layers[active].Px;
            Touch();
            return true;
        }

        /// <summary>Merges the active layer into the one below it.</summary>
        public bool MergeDown()
        {
            if (active <= 0) return false;
            if (layers[active].Locked || layers[active - 1].Locked) return false;
            PushUndo();
            Layer lower = layers[active - 1];
            Layer upper = layers[active];
            if (upper.Visible && upper.Opacity > 0)
                for (int i = 0; i < Count; i++)
                    lower.Px[i] = BlendLayer(lower.Px[i], WithOpacity(upper.Px[i], upper.Opacity), upper.Mode);
            lower.Visible = true;
            layers.RemoveAt(active);
            active--;
            px = layers[active].Px;
            Touch();
            return true;
        }

        /// <summary>Moves the active layer one step up (+1) or down (-1) in the stack.</summary>
        public bool MoveLayer(int delta)
        {
            int target = active + delta;
            if (target < 0 || target >= layers.Count) return false;
            PushUndo();
            Layer l = layers[active];
            layers.RemoveAt(active);
            layers.Insert(target, l);
            active = target;
            px = layers[active].Px;
            Touch();
            return true;
        }

        /// <summary>Merges every layer into one, keeping exactly what is visible.</summary>
        public void Flatten()
        {
            if (layers.Count <= 1) return;
            PushUndo();
            int[] merged = (int[])Composite().Clone();
            layers.Clear();
            Layer l = NewLayer();
            l.Name = "Flattened";
            Array.Copy(merged, l.Px, Count);
            layers.Add(l);
            active = 0;
            px = layers[0].Px;
            Touch();
        }

        /// <summary>Composite of all visible layers, bottom to top, with each layer's opacity and blend mode.</summary>
        public int[] Composite()
        {
            if (flat != null && flatVersion == Version) return flat;
            if (flat == null) flat = new int[Count];
            Array.Clear(flat, 0, Count);
            foreach (Layer l in layers)
            {
                if (!l.Visible || l.Opacity <= 0) continue;
                for (int i = 0; i < Count; i++)
                    flat[i] = BlendLayer(flat[i], WithOpacity(l.Px[i], l.Opacity), l.Mode);
            }
            flatVersion = Version;
            return flat;
        }

        /// <summary>Scales a pixel's alpha by a layer opacity (0…255).</summary>
        static int WithOpacity(int src, int opacity)
        {
            if (opacity >= 255) return src;
            int a = ((src >> 24) & 0xFF) * opacity / 255;
            return (a << 24) | (src & 0x00FFFFFF);
        }

        /// <summary>Source-over with an optional blend mode; the colour is mixed first, then composited normally.</summary>
        static int BlendLayer(int dst, int src, LayerBlend mode)
        {
            if (mode == LayerBlend.Normal) return Blend(dst, src);
            int sa = (src >> 24) & 0xFF;
            if (sa == 0) return dst;
            int da = (dst >> 24) & 0xFF;
            if (da == 0) return src;
            int sr = (src >> 16) & 0xFF, sg = (src >> 8) & 0xFF, sb = src & 0xFF;
            int dr = (dst >> 16) & 0xFF, dg = (dst >> 8) & 0xFF, db = dst & 0xFF;
            int r, g, b;
            if (mode == LayerBlend.Multiply)
            {
                r = sr * dr / 255; g = sg * dg / 255; b = sb * db / 255;
            }
            else
            {
                r = 255 - (255 - sr) * (255 - dr) / 255;
                g = 255 - (255 - sg) * (255 - dg) / 255;
                b = 255 - (255 - sb) * (255 - db) / 255;
            }
            return Blend(dst, (sa << 24) | (r << 16) | (g << 8) | b);
        }

        public int CompositeAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= N || y >= N) return 0;
            return Composite()[y * N + x];
        }

        static int Blend(int dst, int src)
        {
            int sa = (src >> 24) & 0xFF;
            if (sa == 0) return dst;
            if (sa == 255) return src;
            int da = (dst >> 24) & 0xFF;
            float fs = sa / 255f, fd = da / 255f;
            float fo = fs + fd * (1f - fs);
            if (fo <= 0f) return 0;
            int r = (int)((((src >> 16) & 0xFF) * fs + ((dst >> 16) & 0xFF) * fd * (1f - fs)) / fo + 0.5f);
            int g = (int)((((src >> 8) & 0xFF) * fs + ((dst >> 8) & 0xFF) * fd * (1f - fs)) / fo + 0.5f);
            int b = (int)(((src & 0xFF) * fs + (dst & 0xFF) * fd * (1f - fs)) / fo + 0.5f);
            int a = (int)(fo * 255f + 0.5f);
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public void MirrorCopyToRight()
        {
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N / 2; x++)
                    px[y * N + (N - 1 - x)] = px[y * N + x];
        }

        public bool IsBlank()
        {
            int[] c = Composite();
            for (int i = 0; i < Count; i++) if ((c[i] >> 24 & 0xFF) != 0) return false;
            return true;
        }

        // ------------------------------------------------------------ bitmaps
        public Bitmap ToBitmap()
        {
            Bitmap bmp = new Bitmap(N, N, PixelFormat.Format32bppArgb);
            BitmapData d = bmp.LockBits(new Rectangle(0, 0, N, N), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe_copy(Composite(), d.Scan0, d.Stride);
            bmp.UnlockBits(d);
            return bmp;
        }

        void unsafe_copy(int[] src, IntPtr dest, int stride)
        {
            int[] row = new int[N];
            for (int y = 0; y < N; y++)
            {
                Array.Copy(src, y * N, row, 0, N);
                System.Runtime.InteropServices.Marshal.Copy(row, 0, new IntPtr(dest.ToInt64() + y * stride), N);
            }
        }

        public void CopyFromBitmap(Bitmap bmp)
        {
            using (Bitmap scaled = ScaleToCanvas(bmp))
            {
                BitmapData d = scaled.LockBits(new Rectangle(0, 0, N, N), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int[] row = new int[N];
                for (int y = 0; y < N; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(new IntPtr(d.Scan0.ToInt64() + y * d.Stride), row, 0, N);
                    for (int x = 0; x < N; x++) px[y * N + x] = row[x];
                }
                scaled.UnlockBits(d);
            }
            Touch();
        }

        /// <summary>Scale any image into the 40x40 canvas, preserving aspect ratio.</summary>
        public static Bitmap ScaleToCanvas(Image src)
        {
            Bitmap dst = new Bitmap(N, N, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = (src.Width <= N * 2 && src.Height <= N * 2)
                    ? InterpolationMode.NearestNeighbor
                    : InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float scale = Math.Min((float)N / src.Width, (float)N / src.Height);
                int w = Math.Max(1, (int)Math.Round(src.Width * scale));
                int h = Math.Max(1, (int)Math.Round(src.Height * scale));
                int x = (N - w) / 2, y = (N - h) / 2;
                g.DrawImage(src, new Rectangle(x, y, w, h));
            }
            return dst;
        }

        public void LoadBitmap(Bitmap bmp)
        {
            PushUndo();
            CopyFromBitmap(bmp);
        }
    }

    /// <summary>A named custom-theme palette the designer can recall later.</summary>
    internal class ThemePreset
    {
        public string Name = "";
        public string Surface = "#12203A";
        public string Accent = "#F6C452";
        public string Text = "#E6EFFC";

        public ThemePreset() { }

        public ThemePreset(string name, Color surface, Color accent, Color text)
        {
            Name = name;
            Surface = AppSettings.ToHex(surface);
            Accent = AppSettings.ToHex(accent);
            Text = AppSettings.ToHex(text);
        }

        public Color SurfaceColor { get { return AppSettings.ParseHex(Surface, Color.FromArgb(18, 32, 58)); } }
        public Color AccentColor { get { return AppSettings.ParseHex(Accent, Color.FromArgb(246, 196, 82)); } }
        public Color TextColor { get { return AppSettings.ParseHex(Text, Color.FromArgb(230, 239, 252)); } }

        public ThemePreset Clone()
        {
            return new ThemePreset { Name = Name, Surface = Surface, Accent = Accent, Text = Text };
        }
    }

    /// <summary>Pure list rules for named theme presets, kept out of the form so they can be tested.</summary>
    internal static class PresetOps
    {
        public const int MaxName = 24;

        /// <summary>Trims, caps and falls back for an empty preset name.</summary>
        public static string CleanName(string name, string fallback)
        {
            string s = name == null ? "" : name.Trim();
            if (s.Length == 0) s = fallback == null ? "" : fallback.Trim();
            if (s.Length > MaxName) s = s.Substring(0, MaxName).TrimEnd();
            return s;
        }

        /// <summary>The requested name, with " 2", " 3"… appended while another preset uses it.</summary>
        public static string UniqueName(List<ThemePreset> presets, string wanted, int ignoreIndex)
        {
            string baseName = CleanName(wanted, "Preset");
            if (presets == null) return baseName;

            string candidate = baseName;
            for (int n = 2; Taken(presets, candidate, ignoreIndex); n++)
            {
                string suffix = " " + n;
                int room = MaxName - suffix.Length;
                string stem = baseName.Length > room ? baseName.Substring(0, room).TrimEnd() : baseName;
                candidate = stem + suffix;
            }
            return candidate;
        }

        static bool Taken(List<ThemePreset> presets, string name, int ignoreIndex)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (i == ignoreIndex || presets[i] == null) continue;
                if (string.Equals(presets[i].Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>Moves one preset to another slot, preserving the rest of the order.</summary>
        public static void Move(List<ThemePreset> presets, int from, int to)
        {
            if (presets == null || from < 0 || from >= presets.Count) return;
            if (to < 0 || to >= presets.Count || to == from) return;
            ThemePreset p = presets[from];
            presets.RemoveAt(from);
            presets.Insert(to, p);
        }
    }

    internal class AppSettings
    {
        public string Lang = "en";
        public bool LightTheme = false;
        public bool NavyTheme = false;
        public bool CustomTheme = false;
        public string CustomSurface = "#12203A";
        public string CustomAccent = "#F6C452";
        public string CustomText = "#E6EFFC";
        public List<ThemePreset> Presets = new List<ThemePreset>();
        // on by default once the build carries its own Discord application: then there is
        // nothing to set up and presence simply works while Discord is open
        public bool DiscordEnabled = DiscordRpc.HasBuiltInAppId;
        public string DiscordAppId = "";   // empty = use the publisher application from Discord.cs
        public string DiscordImageKey = "logo";
        public bool LayersBeta = false;
        public bool PerfMode = false;

        /// <summary>The palette the settings describe; custom wins over navy, navy over light.</summary>
        public ThemeMode Mode
        {
            get
            {
                return CustomTheme ? ThemeMode.Custom
                    : NavyTheme ? ThemeMode.Navy
                    : LightTheme ? ThemeMode.Light
                    : ThemeMode.Dark;
            }
        }

        /// <summary>Parsed custom theme colours, falling back to the defaults on a bad value.</summary>
        public Color SurfaceColor { get { return ParseHex(CustomSurface, Color.FromArgb(18, 32, 58)); } }
        public Color AccentColor { get { return ParseHex(CustomAccent, Color.FromArgb(246, 196, 82)); } }
        public Color TextColor { get { return ParseHex(CustomText, Color.FromArgb(230, 239, 252)); } }

        public static Color ParseHex(string value, Color fallback)
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            try
            {
                Color c = ColorTranslator.FromHtml(value[0] == '#' ? value : "#" + value);
                return Color.FromArgb(255, c.R, c.G, c.B);
            }
            catch { return fallback; }
        }

        public static string ToHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public AppSettings Clone()
        {
            AppSettings s = new AppSettings();
            s.Lang = Lang;
            s.LightTheme = LightTheme;
            s.NavyTheme = NavyTheme;
            s.CustomTheme = CustomTheme;
            s.CustomSurface = CustomSurface;
            s.CustomAccent = CustomAccent;
            s.CustomText = CustomText;
            s.Presets = new List<ThemePreset>();
            foreach (ThemePreset p in Presets) s.Presets.Add(p.Clone());
            s.DiscordEnabled = DiscordEnabled;
            s.DiscordAppId = DiscordAppId;
            s.DiscordImageKey = DiscordImageKey;
            s.LayersBeta = LayersBeta;
            s.PerfMode = PerfMode;
            return s;
        }
    }

    internal class BadgeEntry
    {
        public string Id;
        public string Name;
        public string Desc;
        public DateTime Created;
        public string PngPath;
        public Bitmap Thumb;
    }

    /// <summary>
    /// Badge gallery + draft persistence under %APPDATA%\HabboBadgeCreatorBeta.
    /// Beta builds keep their own folder so they can never read or overwrite the
    /// released app's badges, draft or settings.
    /// </summary>
    internal static class Store
    {
        public static string Root
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HabboBadgeCreatorBeta");
            }
        }

        public static string GalleryDir { get { return Path.Combine(Root, "badges"); } }
        public static string DraftPng { get { return Path.Combine(Root, "draft.png"); } }
        public static string DraftMeta { get { return Path.Combine(Root, "draft.json"); } }

        public static void Ensure()
        {
            try { Directory.CreateDirectory(GalleryDir); }
            catch { }
        }

        public static string SafeName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "badge";
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' ') sb.Append(c);
            }
            string r = sb.ToString().Trim();
            if (r.Length == 0) r = "badge";
            if (r.Length > 48) r = r.Substring(0, 48).TrimEnd();
            return r;
        }

        public static BadgeEntry Save(string name, string desc, Bitmap bmp)
        {
            Ensure();
            DateTime now = DateTime.Now;
            string id = now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4);
            BadgeEntry e = new BadgeEntry();
            e.Id = id;
            e.Name = name;
            e.Desc = desc;
            e.Created = now;
            e.PngPath = Path.Combine(GalleryDir, id + ".png");
            bmp.Save(e.PngPath, ImageFormat.Png);
            WriteMeta(Path.Combine(GalleryDir, id + ".json"), name, desc, now);
            e.Thumb = (Bitmap)bmp.Clone();
            return e;
        }

        static void WriteMeta(string path, string name, string desc, DateTime created)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"name\": \"").Append(Esc(name)).Append("\",\n");
            sb.Append("  \"desc\": \"").Append(Esc(desc)).Append("\",\n");
            sb.Append("  \"created\": \"").Append(created.ToString("yyyy-MM-ddTHH:mm:ss")).Append("\"\n");
            sb.Append("}\n");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        static string Esc(string s)
        {
            if (s == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append(' ');
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        static string GetStr(string json, string key)
        {
            string tag = "\"" + key + "\"";
            int i = json.IndexOf(tag, StringComparison.Ordinal);
            if (i < 0) return "";
            i = json.IndexOf(':', i + tag.Length);
            if (i < 0) return "";
            i = json.IndexOf('"', i + 1);
            if (i < 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int j = i + 1; j < json.Length; j++)
            {
                char c = json[j];
                if (c == '\\' && j + 1 < json.Length)
                {
                    char n = json[++j];
                    if (n == 'n') sb.Append('\n');
                    else if (n == 'r') sb.Append('\r');
                    else if (n == 't') sb.Append('\t');
                    else sb.Append(n);
                }
                else if (c == '"') break;
                else sb.Append(c);
            }
            return sb.ToString();
        }

        public static List<BadgeEntry> List()
        {
            List<BadgeEntry> list = new List<BadgeEntry>();
            try
            {
                Ensure();
                foreach (string f in Directory.GetFiles(GalleryDir, "*.json"))
                {
                    try
                    {
                        string json = File.ReadAllText(f);
                        string id = Path.GetFileNameWithoutExtension(f);
                        string png = Path.Combine(GalleryDir, id + ".png");
                        if (!File.Exists(png)) continue;
                        BadgeEntry e = new BadgeEntry();
                        e.Id = id;
                        e.Name = GetStr(json, "name");
                        e.Desc = GetStr(json, "desc");
                        DateTime dt;
                        if (DateTime.TryParse(GetStr(json, "created"), out dt)) e.Created = dt;
                        else e.Created = File.GetLastWriteTime(png);
                        e.PngPath = png;
                        e.Thumb = LoadBitmapFree(png);
                        list.Add(e);
                    }
                    catch { }
                }
            }
            catch { }
            list.Sort(delegate (BadgeEntry a, BadgeEntry b) { return b.Created.CompareTo(a.Created); });
            return list;
        }

        /// <summary>Reads a PNG without keeping the file locked.</summary>
        public static Bitmap LoadBitmapFree(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image img = Image.FromStream(fs))
            {
                return new Bitmap(img);
            }
        }

        public static void Delete(BadgeEntry e)
        {
            try { if (File.Exists(e.PngPath)) File.Delete(e.PngPath); } catch { }
            try
            {
                string meta = Path.Combine(GalleryDir, e.Id + ".json");
                if (File.Exists(meta)) File.Delete(meta);
            }
            catch { }
        }

        public static void SaveDraft(Bitmap bmp, string name, string desc)
        {
            try
            {
                Ensure();
                bmp.Save(DraftPng, ImageFormat.Png);
                WriteMeta(DraftMeta, name, desc, DateTime.Now);
            }
            catch { }
        }

        public static bool LoadDraft(out Bitmap bmp, out string name, out string desc)
        {
            bmp = null;
            name = "";
            desc = "";
            try
            {
                if (!File.Exists(DraftPng)) return false;
                bmp = LoadBitmapFree(DraftPng);
                if (File.Exists(DraftMeta))
                {
                    string json = File.ReadAllText(DraftMeta);
                    name = GetStr(json, "name");
                    desc = GetStr(json, "desc");
                }
                return true;
            }
            catch { return false; }
        }

        public static void OpenFolder(string path)
        {
            try
            {
                Ensure();
                System.Diagnostics.Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch { }
        }

        public static string ExportPng(Bitmap bmp, string path)
        {
            bmp.Save(path, ImageFormat.Png);
            return path;
        }

        public static string SettingsFile { get { return Path.Combine(Root, "settings.json"); } }

        static bool GetBool(string json, string key)
        {
            string tag = "\"" + key + "\"";
            int i = json.IndexOf(tag, StringComparison.Ordinal);
            if (i < 0) return false;
            i = json.IndexOf(':', i + tag.Length);
            if (i < 0) return false;
            i++;
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            return string.Compare(json, i, "true", 0, 4, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static AppSettings LoadSettings()
        {
            AppSettings s = new AppSettings();
            try
            {
                if (!File.Exists(SettingsFile)) return s;
                string json = File.ReadAllText(SettingsFile);
                string lang = GetStr(json, "lang");
                if (lang == "de" || lang == "en") s.Lang = lang;
                s.DiscordAppId = GetStr(json, "discordAppId");
                string key = GetStr(json, "discordImageKey");
                if (key.Length > 0) s.DiscordImageKey = key;
                s.DiscordEnabled = GetBool(json, "discordEnabled");
                s.LayersBeta = GetBool(json, "layersBeta");
                s.PerfMode = GetBool(json, "perfMode");
                string theme = GetStr(json, "theme");
                if (theme == "light") s.LightTheme = true;
                // "glass" is the retired name of the navy palette, so old settings still load
                if (theme == "navy" || theme == "glass") { s.NavyTheme = true; s.LightTheme = false; }
                if (theme == "custom") { s.CustomTheme = true; s.LightTheme = false; s.NavyTheme = false; }
                string surface = GetStr(json, "customSurface");
                if (surface.Length > 0) s.CustomSurface = surface;
                string accent = GetStr(json, "customAccent");
                if (accent.Length > 0) s.CustomAccent = accent;
                string ink = GetStr(json, "customText");
                if (ink.Length > 0) s.CustomText = ink;
                ParsePresets(json, s);
            }
            catch { }
            return s;
        }

        /// <summary>Reads the saved custom-theme presets; a broken entry is skipped, not fatal.</summary>
        static void ParsePresets(string json, AppSettings s)
        {
            int i = json.IndexOf("\"presets\"", StringComparison.Ordinal);
            if (i < 0) return;
            int open = json.IndexOf('[', i);
            if (open < 0) return;
            int close = json.IndexOf(']', open);
            if (close < 0) return;
            string block = json.Substring(open + 1, close - open - 1);
            int at = 0;
            while (s.Presets.Count < 12)
            {
                int os = block.IndexOf('{', at);
                if (os < 0) break;
                int oe = block.IndexOf('}', os);
                if (oe < 0) break;
                string obj = block.Substring(os + 1, oe - os - 1);
                string name = GetStr(obj, "name").Trim();
                string surface = GetStr(obj, "surface");
                if (name.Length > 0 && surface.Length > 0)
                {
                    s.Presets.Add(new ThemePreset
                    {
                        Name = name,
                        Surface = surface,
                        Accent = GetStr(obj, "accent"),
                        Text = GetStr(obj, "text")
                    });
                }
                at = oe + 1;
            }
        }

        public static void SaveSettings(AppSettings s)
        {
            try
            {
                Ensure();
                StringBuilder sb = new StringBuilder();
                sb.Append("{\n");
                sb.Append("  \"lang\": \"").Append(Esc(s.Lang)).Append("\",\n");
                sb.Append("  \"theme\": \"").Append(s.CustomTheme ? "custom" : s.NavyTheme ? "navy" : s.LightTheme ? "light" : "dark").Append("\",\n");
                sb.Append("  \"customSurface\": \"").Append(Esc(s.CustomSurface)).Append("\",\n");
                sb.Append("  \"customAccent\": \"").Append(Esc(s.CustomAccent)).Append("\",\n");
                sb.Append("  \"customText\": \"").Append(Esc(s.CustomText)).Append("\",\n");
                sb.Append("  \"presets\": [");
                for (int i = 0; i < s.Presets.Count; i++)
                {
                    ThemePreset p = s.Presets[i];
                    sb.Append(i == 0 ? "\n" : ",\n");
                    sb.Append("    { \"name\": \"").Append(Esc(p.Name))
                      .Append("\", \"surface\": \"").Append(Esc(p.Surface))
                      .Append("\", \"accent\": \"").Append(Esc(p.Accent))
                      .Append("\", \"text\": \"").Append(Esc(p.Text)).Append("\" }");
                }
                sb.Append(s.Presets.Count > 0 ? "\n  ],\n" : "],\n");
                sb.Append("  \"discordEnabled\": ").Append(s.DiscordEnabled ? "true" : "false").Append(",\n");
                sb.Append("  \"discordAppId\": \"").Append(Esc(s.DiscordAppId)).Append("\",\n");
                sb.Append("  \"discordImageKey\": \"").Append(Esc(s.DiscordImageKey)).Append("\",\n");
                sb.Append("  \"layersBeta\": ").Append(s.LayersBeta ? "true" : "false").Append(",\n");
                sb.Append("  \"perfMode\": ").Append(s.PerfMode ? "true" : "false").Append("\n");
                sb.Append("}\n");
                File.WriteAllText(SettingsFile, sb.ToString(), new UTF8Encoding(false));
            }
            catch { }
        }
    }
}
