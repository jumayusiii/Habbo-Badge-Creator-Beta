using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>The palettes the app can wear; Custom is derived from three picked colours.</summary>
    internal enum ThemeMode { Dark, Light, Navy, Custom }

    /// <summary>Colours, fonts, scale helpers and GDI+ primitives.</summary>
    internal static class T
    {
        public static float S = 1f;

        public static int P(float v) { return (int)Math.Round(v * S); }
        public static float F(float v) { return v * S; }

        // Flat material surfaces: the window is one base tone, cards step up one level.
        // Every colour below is a token of the active palette (see Apply).
        public static bool Light;
        public static ThemeMode Mode;

        // custom (beta) theme: the three colours the user picked, kept here so Apply(Custom)
        // can rebuild the palette from them at any time
        public static Color CustomSurface = Color.FromArgb(18, 32, 58);
        public static Color CustomAccent = Color.FromArgb(246, 196, 82);
        public static Color CustomText = Color.FromArgb(230, 239, 252);

        public static Color Bg1, Bg2, TitleBg, Card, CardTop, CardBot, Inset, Segment;
        public static Color Stroke, Focus, Line, LineSoft, Text, Muted, Dim;
        public static Color Gold, GoldHi, GoldText, Hover, HoverSoft, Danger;
        public static Color Check1, Check2, Teal;

        // Secondary tokens: quiet hues that used to be literals inside controls.
        public static Color StrokeHot, BtnText, IconIdle, ChipText, DotOff, TipBg;
        public static Color GridLine, FrameLine, HoverOutline, DashLine, ScrollBar;
        public static Color ToastBg, ToastText, Rim, TileRim, DangerText;
        public static Color PillBg, PillStroke, SwatchRing, LangPill, LangPillText, WinFg;

        /// <summary>Switches the whole application to the dark, light or navy palette.</summary>
        public static void Apply(bool light)
        {
            Apply(light ? ThemeMode.Light : ThemeMode.Dark);
        }

        public static void Apply(ThemeMode mode)
        {
            if (mode == ThemeMode.Custom)
            {
                ApplyCustom(CustomSurface, CustomAccent, CustomText);
                return;
            }
            Mode = mode;
            if (mode == ThemeMode.Light) ApplyLight();
            else if (mode == ThemeMode.Navy) ApplyNavy();
            else ApplyDark();

            Focus = Color.FromArgb(190, 246, 196, 82);
            GoldText = Color.FromArgb(26, 19, 5);
            BuildChecker();
        }

        /// <summary>Applies the palette a settings object describes, including its custom colours.</summary>
        public static void Apply(AppSettings s)
        {
            if (s.Mode == ThemeMode.Custom) ApplyCustom(s.SurfaceColor, s.AccentColor, s.TextColor);
            else Apply(s.Mode);
        }

        static void ApplyDark()
        {
            Light = false;
            {
                Bg1 = Color.FromArgb(12, 15, 19);
                Bg2 = Bg1;
                TitleBg = Bg1;
                Card = Color.FromArgb(23, 28, 34);
                CardTop = Card;
                CardBot = Card;
                Inset = Color.FromArgb(10, 13, 17);
                Segment = Color.FromArgb(15, 19, 24);
                Stroke = Color.FromArgb(40, 47, 58);
                Line = Color.FromArgb(31, 37, 45);
                LineSoft = Color.FromArgb(25, 30, 37);
                Text = Color.FromArgb(232, 237, 244);
                Muted = Color.FromArgb(148, 159, 174);
                Dim = Color.FromArgb(101, 113, 128);
                Gold = Color.FromArgb(246, 196, 82);
                GoldHi = Color.FromArgb(255, 213, 123);
                Hover = Color.FromArgb(32, 39, 48);
                HoverSoft = Color.FromArgb(26, 31, 39);
                Danger = Color.FromArgb(229, 72, 77);
                Check1 = Color.FromArgb(22, 27, 34);
                Check2 = Color.FromArgb(14, 18, 23);
                Teal = Color.FromArgb(79, 209, 197);

                StrokeHot = Color.FromArgb(58, 68, 82);
                BtnText = Color.FromArgb(188, 199, 214);
                IconIdle = Color.FromArgb(160, 171, 185);
                ChipText = Color.FromArgb(168, 182, 203);
                DotOff = Color.FromArgb(92, 106, 129);
                TipBg = Color.FromArgb(32, 38, 47);
                GridLine = Color.FromArgb(22, 255, 255, 255);
                FrameLine = Color.FromArgb(70, 86, 116);
                HoverOutline = Color.FromArgb(235, 255, 255, 255);
                DashLine = Color.FromArgb(48, 56, 68);
                ScrollBar = Color.FromArgb(70, 92, 124);
                ToastBg = Color.FromArgb(242, 30, 36, 44);
                ToastText = Text;
                Rim = Color.FromArgb(40, 255, 255, 255);
                TileRim = Color.FromArgb(34, 255, 255, 255);
                DangerText = Color.FromArgb(255, 140, 143);
                PillBg = Color.FromArgb(36, 60, 50);
                PillStroke = Color.FromArgb(52, 120, 98);
                SwatchRing = Color.FromArgb(190, 255, 255, 255);
                LangPill = Color.FromArgb(232, 237, 244);
                LangPillText = Color.FromArgb(17, 21, 26);
                WinFg = Color.FromArgb(178, 188, 200);
            }
        }

        static void ApplyLight()
        {
            Light = true;
            {
                Bg1 = Color.FromArgb(243, 245, 249);
                Bg2 = Bg1;
                TitleBg = Bg1;
                Card = Color.FromArgb(255, 255, 255);
                CardTop = Card;
                CardBot = Card;
                Inset = Color.FromArgb(246, 248, 251);
                Segment = Color.FromArgb(239, 242, 247);
                Stroke = Color.FromArgb(219, 225, 233);
                Line = Color.FromArgb(228, 233, 240);
                LineSoft = Color.FromArgb(235, 239, 245);
                Text = Color.FromArgb(26, 32, 40);
                Muted = Color.FromArgb(94, 105, 120);
                Dim = Color.FromArgb(134, 145, 159);
                Gold = Color.FromArgb(226, 158, 26);
                GoldHi = Color.FromArgb(244, 181, 61);
                Hover = Color.FromArgb(232, 237, 244);
                HoverSoft = Color.FromArgb(241, 244, 249);
                Danger = Color.FromArgb(212, 56, 62);
                Check1 = Color.FromArgb(232, 237, 243);
                Check2 = Color.FromArgb(250, 251, 253);
                Teal = Color.FromArgb(26, 145, 131);

                StrokeHot = Color.FromArgb(186, 196, 210);
                BtnText = Color.FromArgb(74, 86, 100);
                IconIdle = Color.FromArgb(110, 122, 138);
                ChipText = Color.FromArgb(84, 96, 112);
                DotOff = Color.FromArgb(158, 168, 180);
                TipBg = Color.FromArgb(255, 255, 255);
                GridLine = Color.FromArgb(20, 0, 0, 0);
                FrameLine = Color.FromArgb(184, 194, 207);
                HoverOutline = Color.FromArgb(120, 20, 30, 45);
                DashLine = Color.FromArgb(203, 212, 223);
                ScrollBar = Color.FromArgb(186, 196, 210);
                ToastBg = Color.FromArgb(250, 251, 253);
                ToastText = Text;
                Rim = Color.FromArgb(26, 0, 0, 0);
                TileRim = Color.FromArgb(30, 0, 0, 0);
                DangerText = Color.FromArgb(192, 42, 49);
                PillBg = Color.FromArgb(226, 244, 239);
                PillStroke = Color.FromArgb(158, 216, 203);
                SwatchRing = Color.FromArgb(140, 20, 30, 45);
                LangPill = Color.FromArgb(26, 32, 40);
                LangPillText = Color.FromArgb(246, 248, 251);
                WinFg = Color.FromArgb(78, 90, 105);
            }
        }

        /// <summary>
        /// Navy palette: the dark theme's layout and density in deep blue. The window is a
        /// navy base, cards step up one navy tone and recesses drop one, so every panel and
        /// sheet stays readable without any lighting tricks.
        /// </summary>
        static void ApplyNavy()
        {
            Light = false;

            Bg1 = Color.FromArgb(9, 18, 34);
            Bg2 = Bg1;
            TitleBg = Bg1;
            Card = Color.FromArgb(18, 33, 58);
            CardTop = Card;
            CardBot = Card;
            Inset = Color.FromArgb(11, 22, 41);
            Segment = Color.FromArgb(14, 27, 50);
            Stroke = Color.FromArgb(38, 61, 95);
            Line = Color.FromArgb(28, 46, 74);
            LineSoft = Color.FromArgb(23, 39, 65);
            Text = Color.FromArgb(230, 239, 252);
            Muted = Color.FromArgb(146, 168, 198);
            Dim = Color.FromArgb(104, 126, 158);
            Gold = Color.FromArgb(246, 196, 82);
            GoldHi = Color.FromArgb(255, 213, 123);
            Hover = Color.FromArgb(26, 46, 78);
            HoverSoft = Color.FromArgb(21, 38, 66);
            Danger = Color.FromArgb(229, 80, 86);
            Check1 = Color.FromArgb(16, 30, 52);
            Check2 = Color.FromArgb(11, 22, 40);
            Teal = Color.FromArgb(86, 206, 200);

            StrokeHot = Color.FromArgb(56, 82, 120);
            BtnText = Color.FromArgb(188, 208, 232);
            IconIdle = Color.FromArgb(160, 182, 210);
            ChipText = Color.FromArgb(172, 196, 224);
            DotOff = Color.FromArgb(88, 110, 140);
            TipBg = Color.FromArgb(23, 40, 66);
            GridLine = Color.FromArgb(24, 170, 204, 255);
            FrameLine = Color.FromArgb(68, 96, 136);
            HoverOutline = Color.FromArgb(235, 255, 255, 255);
            DashLine = Color.FromArgb(44, 66, 100);
            ScrollBar = Color.FromArgb(66, 96, 138);
            ToastBg = Color.FromArgb(242, 21, 37, 61);
            ToastText = Text;
            Rim = Color.FromArgb(38, 160, 196, 255);
            TileRim = Color.FromArgb(34, 255, 255, 255);
            DangerText = Color.FromArgb(255, 145, 148);
            PillBg = Color.FromArgb(30, 54, 74);
            PillStroke = Color.FromArgb(52, 108, 112);
            SwatchRing = Color.FromArgb(190, 255, 255, 255);
            LangPill = Color.FromArgb(230, 239, 252);
            LangPillText = Color.FromArgb(10, 20, 36);
            WinFg = Color.FromArgb(176, 196, 220);
        }

        /// <summary>
        /// Custom (beta) palette: the user picks a surface, an accent and a text colour and
        /// every other token is derived from those three, so all controls follow without
        /// per-theme special cases. Bright surfaces get the light-family derivations.
        /// </summary>
        public static void ApplyCustom(Color surface, Color accent, Color text)
        {
            Mode = ThemeMode.Custom;
            CustomSurface = surface;
            CustomAccent = accent;
            CustomText = text;

            bool dark = Lum(surface) < 0.55f;
            Light = !dark;
            Color away = dark ? Color.Black : Color.White;
            Color toward = dark ? Color.White : Color.Black;

            Bg1 = Mix(surface, away, dark ? 0.55f : 0.07f);
            Bg2 = Bg1;
            TitleBg = Bg1;
            Card = surface;
            CardTop = surface;
            CardBot = surface;
            Inset = Mix(surface, away, dark ? 0.30f : 0.04f);
            Segment = Mix(surface, toward, dark ? 0.07f : 0.05f);
            Stroke = Mix(surface, toward, dark ? 0.18f : 0.16f);
            Line = Mix(surface, toward, dark ? 0.11f : 0.09f);
            LineSoft = Mix(surface, toward, dark ? 0.07f : 0.06f);
            Text = text;
            Muted = Mix(text, surface, 0.40f);
            Dim = Mix(text, surface, 0.58f);
            Gold = accent;
            GoldHi = Mix(accent, toward, 0.20f);
            GoldText = Lum(accent) > 0.55f ? Color.FromArgb(24, 19, 7) : Color.FromArgb(250, 250, 252);
            Hover = Mix(surface, toward, dark ? 0.10f : 0.05f);
            HoverSoft = Mix(surface, toward, dark ? 0.05f : 0.03f);
            Danger = Color.FromArgb(229, 82, 88);
            Check1 = Mix(Inset, toward, dark ? 0.09f : 0.03f);
            Check2 = Mix(Inset, away, dark ? 0.10f : 0.02f);
            Teal = Mix(accent, Color.White, 0.30f);

            StrokeHot = Mix(surface, toward, dark ? 0.32f : 0.24f);
            BtnText = Mix(text, surface, 0.15f);
            IconIdle = Mix(text, surface, 0.30f);
            ChipText = Mix(text, surface, 0.24f);
            DotOff = Mix(text, surface, 0.55f);
            TipBg = Mix(surface, toward, dark ? 0.04f : 0.02f);
            GridLine = Color.FromArgb(dark ? 24 : 20, text);
            FrameLine = Mix(surface, toward, 0.38f);
            HoverOutline = Color.FromArgb(235, text);
            DashLine = Mix(surface, toward, 0.17f);
            ScrollBar = Mix(surface, toward, 0.32f);
            ToastBg = Color.FromArgb(242, Mix(Bg1, toward, 0.05f));
            ToastText = text;
            Rim = Color.FromArgb(dark ? 42 : 26, text);
            TileRim = Color.FromArgb(36, text);
            DangerText = Color.FromArgb(255, 148, 152);
            PillBg = Mix(surface, accent, 0.20f);
            PillStroke = Mix(surface, accent, 0.48f);
            SwatchRing = Color.FromArgb(dark ? 190 : 140, text);
            LangPill = text;
            LangPillText = Lum(text) > 0.55f ? Color.FromArgb(14, 18, 26) : Color.FromArgb(250, 250, 252);
            WinFg = Mix(text, surface, 0.22f);

            Focus = Color.FromArgb(190, accent);
            BuildChecker();
        }

        /// <summary>Perceived brightness of a colour, 0 (black) to 1 (white).</summary>
        public static float Lum(Color c)
        {
            return (0.2126f * c.R + 0.7152f * c.G + 0.0722f * c.B) / 255f;
        }

        // Colour maths and the two picker painters live here rather than inside a sheet: the
        // colour sheet and the theme designer share them, so the two pickers cannot drift apart.

        /// <summary>HSV to an opaque colour. Hue 0–360, saturation and value 0–1.</summary>
        public static Color HsvToRgb(float h, float s, float v)
        {
            h = ((h % 360f) + 360f) % 360f;
            float c = v * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = v - c;
            float r, g, b;
            if (h < 60f) { r = c; g = x; b = 0f; }
            else if (h < 120f) { r = x; g = c; b = 0f; }
            else if (h < 180f) { r = 0f; g = c; b = x; }
            else if (h < 240f) { r = 0f; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0f; b = c; }
            else { r = c; g = 0f; b = x; }
            return Color.FromArgb(255,
                (int)Math.Round((r + m) * 255f),
                (int)Math.Round((g + m) * 255f),
                (int)Math.Round((b + m) * 255f));
        }

        /// <summary>The hue, saturation and value behind a colour. Hue 0–360, the rest 0–1.</summary>
        public static void RgbToHsv(Color c, out float h, out float s, out float v)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float d = max - min;
            v = max;
            s = max <= 0f ? 0f : d / max;
            if (d <= 0f) { h = 0f; return; }
            if (max == r) h = 60f * (((g - b) / d) % 6f);
            else if (max == g) h = 60f * (((b - r) / d) + 2f);
            else h = 60f * (((r - g) / d) + 4f);
            if (h < 0f) h += 360f;
        }

        /// <summary>Window backdrop. One flat tone in every palette.</summary>
        public static void PaintBackdrop(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Bg1)) g.FillRectangle(b, r);
        }

        /// <summary>Backdrop for the floating sheets (Settings, Changelog).</summary>
        public static void PaintSheet(Graphics g, Rectangle r, Color fallback)
        {
            using (SolidBrush b = new SolidBrush(fallback)) g.FillRectangle(b, r);
        }

        // Preview tones for the appearance picker, so all three palettes can be drawn at once.
        public static Color MiniBg(ThemeMode m)
        {
            if (m == ThemeMode.Light) return Color.FromArgb(243, 245, 249);
            if (m == ThemeMode.Navy) return Color.FromArgb(9, 18, 34);
            if (m == ThemeMode.Custom)
                return Mix(CustomSurface, Lum(CustomSurface) < 0.55f ? Color.Black : Color.White, 0.55f);
            return Color.FromArgb(12, 15, 19);
        }
        public static Color MiniCard(ThemeMode m)
        {
            if (m == ThemeMode.Light) return Color.FromArgb(255, 255, 255);
            if (m == ThemeMode.Navy) return Color.FromArgb(18, 33, 58);
            if (m == ThemeMode.Custom) return CustomSurface;
            return Color.FromArgb(23, 28, 34);
        }
        public static Color MiniStroke(ThemeMode m)
        {
            if (m == ThemeMode.Light) return Color.FromArgb(219, 225, 233);
            if (m == ThemeMode.Navy) return Color.FromArgb(52, 78, 116);
            if (m == ThemeMode.Custom)
                return Mix(CustomSurface, Lum(CustomSurface) < 0.55f ? Color.White : Color.Black, 0.30f);
            return Color.FromArgb(40, 47, 58);
        }
        public static Color MiniText(ThemeMode m)
        {
            if (m == ThemeMode.Light) return Color.FromArgb(26, 32, 40);
            if (m == ThemeMode.Navy) return Color.FromArgb(230, 239, 252);
            if (m == ThemeMode.Custom) return CustomText;
            return Color.FromArgb(232, 237, 244);
        }
        public static Color MiniGold(ThemeMode m)
        {
            if (m == ThemeMode.Light) return Color.FromArgb(226, 158, 26);
            if (m == ThemeMode.Navy) return Color.FromArgb(246, 196, 82);
            if (m == ThemeMode.Custom) return CustomAccent;
            return Color.FromArgb(246, 196, 82);
        }

        public static Font FWord, FPageTitle, FCardTitle, FLabel, FBody, FSmall, FBtn, FMicro, FInput, FChip, FInputSm;
        public static TextureBrush CheckBrush;
        public static Bitmap Logo;
        public static Bitmap Wordmark;

        /// <summary>
        /// Creates the font set. <paramref name="fontScale"/> is only used by the debug
        /// --scale switch, which fakes a larger display DPI: at a real DPI Windows already
        /// grows the type on its own, so the normal path passes 1.
        /// </summary>
        public static void Init(float fontScale = 1f)
        {
            if (fontScale <= 0f) fontScale = 1f;
            FWord = new Font("Segoe UI Black", 15f * fontScale, FontStyle.Regular);
            FPageTitle = new Font("Segoe UI", 15.5f * fontScale, FontStyle.Bold);
            FCardTitle = new Font("Segoe UI Semibold", 12f * fontScale, FontStyle.Regular);
            FLabel = new Font("Segoe UI Semibold", 8.75f * fontScale, FontStyle.Regular);
            FBody = new Font("Segoe UI", 9.5f * fontScale, FontStyle.Regular);
            FSmall = new Font("Segoe UI", 8.25f * fontScale, FontStyle.Regular);
            FBtn = new Font("Segoe UI Semibold", 9f * fontScale, FontStyle.Regular);
            FMicro = new Font("Segoe UI", 7.75f * fontScale, FontStyle.Regular);
            FInput = new Font("Segoe UI", 9.5f * fontScale, FontStyle.Regular);
            FInputSm = new Font("Segoe UI", 8.75f * fontScale, FontStyle.Regular);
            FChip = new Font("Segoe UI Semibold", 8f * fontScale, FontStyle.Regular);

            Apply(Light);

            Logo = Load("logo.png");
            Wordmark = Load("habbo-wordmark.png");
        }

        /// <summary>Rebuilds the transparency checkerboard for the active palette.</summary>
        static void BuildChecker()
        {
            int c = Math.Max(4, P(5));
            Bitmap b = new Bitmap(c * 2, c * 2);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Check1);
                using (SolidBrush br = new SolidBrush(Check2))
                {
                    g.FillRectangle(br, 0, 0, c, c);
                    g.FillRectangle(br, c, c, c, c);
                }
            }
            if (CheckBrush != null)
            {
                Image old = CheckBrush.Image;
                CheckBrush.Dispose();
                if (old != null) old.Dispose();
            }
            CheckBrush = new TextureBrush(b);
        }

        static Bitmap Load(string name)
        {
            try
            {
                using (Stream s = typeof(T).Assembly.GetManifestResourceStream(name))
                {
                    if (s != null)
                        using (Image img = Image.FromStream(s))
                            return new Bitmap(img);
                }
            }
            catch { }
            return null;
        }

        // ---------------------------------------------------------------- shapes
        public static GraphicsPath Round(RectangleF r, float rad)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(rad * 2f, Math.Min(r.Width, r.Height));
            if (d <= 0.5f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180f, 90f);
            p.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
            p.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath RoundRect(Rectangle r, float rad)
        {
            return Round(new RectangleF(r.X, r.Y, r.Width, r.Height), rad);
        }

        public static void FillRound(Graphics g, RectangleF r, float rad, Color c)
        {
            if (c.A == 0 || r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath p = Round(r, rad))
            using (SolidBrush b = new SolidBrush(c))
                g.FillPath(b, p);
        }

        public static void GradientRound(Graphics g, RectangleF r, float rad, Color top, Color bottom)
        {
            if (r.Width <= 1f || r.Height <= 1f) return;
            using (GraphicsPath p = Round(r, rad))
            using (LinearGradientBrush b = new LinearGradientBrush(
                new RectangleF(r.X, r.Y - 1f, r.Width, r.Height + 2f), top, bottom, 90f))
                g.FillPath(b, p);
        }

        /// <summary>Cheap soft drop shadow built from stacked translucent outlines.</summary>
        public static void Shadow(Graphics g, RectangleF r, float rad, int spread, int alpha)
        {
            if (spread <= 0) return;
            for (int i = spread; i >= 1; i--)
            {
                int a = (int)(alpha * (1f - (float)i / (spread + 1f)) / (spread * 0.8f));
                if (a <= 0) continue;
                RectangleF rr = new RectangleF(r.X - i, r.Y - i + spread * 0.55f, r.Width + i * 2, r.Height + i * 2);
                using (GraphicsPath p = Round(rr, rad + i))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(a, 0, 0, 0)))
                    g.FillPath(b, p);
            }
        }

        public static void StrokeRound(Graphics g, RectangleF r, float rad, Color c, float w)
        {
            if (c.A == 0 || r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath p = Round(r, rad))
            using (Pen pen = new Pen(c, w))
                g.DrawPath(pen, p);
        }

        public static void Checker(Graphics g, RectangleF r)
        {
            GraphicsState st = g.Save();
            using (GraphicsPath p = Round(r, F(2f)))
            {
                g.SetClip(p, CombineMode.Intersect);
                RectangleF big = new RectangleF(r.X - 16f, r.Y - 16f, r.Width + 32f, r.Height + 32f);
                g.FillRectangle(CheckBrush, big);
            }
            g.Restore(st);
        }

        /// <summary>
        /// Saturation/value square, as used by every picker in the app: a hue fill, a white wash
        /// running right and a black wash running down, with the crosshair on the picked spot.
        /// </summary>
        public static void PaintSvSquare(Graphics g, RectangleF r, float hue, float sat, float val, Color current)
        {
            float rad = F(8f);
            using (GraphicsPath clip = Round(r, rad))
            {
                GraphicsState st = g.Save();
                g.SetClip(clip, CombineMode.Intersect);
                using (SolidBrush hueFill = new SolidBrush(HsvToRgb(hue, 1f, 1f)))
                    g.FillRectangle(hueFill, r);
                using (LinearGradientBrush white = new LinearGradientBrush(
                    new RectangleF(r.X, r.Y, r.Width + 1f, r.Height), Color.White, Color.FromArgb(255, 255, 255, 255), 0f))
                {
                    white.InterpolationColors = new ColorBlend(2)
                    {
                        Colors = new[] { Color.White, Color.FromArgb(0, 255, 255, 255) },
                        Positions = new[] { 0f, 1f }
                    };
                    g.FillRectangle(white, r);
                }
                using (LinearGradientBrush black = new LinearGradientBrush(
                    new RectangleF(r.X, r.Y, r.Width, r.Height + 1f), Color.FromArgb(0, 0, 0, 0), Color.Black, 90f))
                    g.FillRectangle(black, r);
                g.Restore(st);
            }
            StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f), rad, Stroke, 1f);

            float kx = r.X + sat * r.Width;
            float ky = r.Y + (1f - val) * r.Height;
            using (SolidBrush knob = new SolidBrush(current)) g.FillEllipse(knob, kx - P(6), ky - P(6), P(12), P(12));
            using (Pen ring = new Pen(Color.White, 2f)) g.DrawEllipse(ring, kx - P(6), ky - P(6), P(12), P(12));
            using (Pen dark = new Pen(Color.FromArgb(120, 0, 0, 0), 1f)) g.DrawEllipse(dark, kx - P(7), ky - P(7), P(14), P(14));
        }

        /// <summary>
        /// Vertical hue rail for the pickers: seven evenly spaced stops from red back to red,
        /// with the marker drawn across the rail at the picked hue.
        /// </summary>
        public static void PaintHueRail(Graphics g, RectangleF r, float hue)
        {
            float rad = F(9f);
            using (GraphicsPath clip = Round(r, rad))
            {
                GraphicsState st = g.Save();
                g.SetClip(clip, CombineMode.Intersect);
                using (LinearGradientBrush lb = new LinearGradientBrush(
                    new RectangleF(r.X, r.Y, r.Width, r.Height + 1f), Color.Red, Color.Red, 90f))
                {
                    lb.InterpolationColors = new ColorBlend(7)
                    {
                        Colors = new[]
                        {
                            Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 255, 255, 0),
                            Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 0, 255, 255),
                            Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 255, 0, 255),
                            Color.FromArgb(255, 255, 0, 0)
                        },
                        Positions = new[] { 0f, 0.17f, 0.33f, 0.5f, 0.67f, 0.83f, 1f }
                    };
                    g.FillRectangle(lb, r);
                }
                g.Restore(st);
            }
            StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f), rad, Stroke, 1f);

            float hy = r.Y + hue / 360f * r.Height;
            using (Pen mark = new Pen(Color.White, 2.4f))
                g.DrawLine(mark, r.X - P(2), hy, r.Right + P(2), hy);
        }

        public static void HLine(Graphics g, int x1, int x2, int y, Color c)
        {
            using (Pen p = new Pen(c, 1f))
                g.DrawLine(p, x1, y + 0.5f, x2, y + 0.5f);
        }

        public static void VLine(Graphics g, int x, int y1, int y2, Color c)
        {
            using (Pen p = new Pen(c, 1f))
                g.DrawLine(p, x + 0.5f, y1, x + 0.5f, y2);
        }

        /// <summary>Draws the app logo clipped into a rounded tile.</summary>
        public static void LogoTile(Graphics g, RectangleF r, float rad)
        {
            if (Logo == null) { HTile(g, r, rad); return; }
            GraphicsState st = g.Save();
            using (GraphicsPath p = Round(r, rad))
            {
                g.SetClip(p, CombineMode.Intersect);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(Logo, r);
            }
            g.Restore(st);
            using (GraphicsPath p = Round(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f), rad))
            using (Pen pen = new Pen(TileRim, 1f))
                g.DrawPath(pen, p);
        }

        /// <summary>Habbo-style yellow tile with a blocky "H".</summary>
        public static void HTile(Graphics g, RectangleF r, float rad)
        {
            using (GraphicsPath p = Round(r, rad))
            using (LinearGradientBrush b = new LinearGradientBrush(
                new RectangleF(r.X, r.Y - 2f, r.Width, r.Height + 4f),
                Color.FromArgb(255, 220, 128), Color.FromArgb(230, 160, 30), 90f))
            {
                g.FillPath(b, p);
            }
            float hw = r.Width * 0.50f, hh = r.Height * 0.52f;
            float x = r.X + (r.Width - hw) / 2f, y = r.Y + (r.Height - hh) / 2f;
            float bar = hw * 0.29f, cross = hh * 0.26f;
            using (SolidBrush b = new SolidBrush(Color.FromArgb(35, 26, 6)))
            {
                g.FillRectangle(b, x, y, bar, hh);
                g.FillRectangle(b, x + hw - bar, y, bar, hh);
                g.FillRectangle(b, x, y + (hh - cross) / 2f, hw, cross);
            }
        }

        // ---------------------------------------------------------------- text
        public const TextFormatFlags FL = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
        public const TextFormatFlags FC = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
        public const TextFormatFlags FR = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

        public static void Str(Graphics g, string s, Font f, Color c, Rectangle r, TextFormatFlags fl)
        {
            TextRenderer.DrawText(g, s, f, r, c, fl);
        }

        public static void StrAbove(Graphics g, string s, Font f, Color c, int x, int y)
        {
            TextRenderer.DrawText(g, s, f, new Point(x, y), c,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        public static void Wrap(Graphics g, string s, Font f, Color c, Rectangle r)
        {
            TextRenderer.DrawText(g, s, f, r, c,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        public static void StrEll(Graphics g, string s, Font f, Color c, Rectangle r, TextFormatFlags fl)
        {
            TextRenderer.DrawText(g, s, f, r, c, fl | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }

        public static Size Meas(string s, Font f)
        {
            return TextRenderer.MeasureText(s, f, new Size(10000, 10000),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        // ---------------------------------------------------------------- icons
        /// <summary>Vector icons drawn in a 24x24 design grid.</summary>
        public static void Icon(Graphics g, string n, RectangleF r, Color c)
        {
            float u = Math.Min(r.Width, r.Height) / 24f;
            GraphicsState st = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(r.X, r.Y);
            g.ScaleTransform(u, u);
            float pw = Math.Max(1.15f, 1.85f * Math.Min(1f, u));
            using (Pen p = new Pen(c, pw))
            using (SolidBrush b = new SolidBrush(c))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
                switch (n)
                {
                    case "pencil":
                        g.DrawLine(p, 7.4f, 16.6f, 18f, 6f);
                        g.FillPolygon(b, new PointF[] { new PointF(4.6f, 19.4f), new PointF(5.9f, 15.4f), new PointF(8.6f, 18.1f) });
                        break;
                    case "eraser":
                        GraphicsState es = g.Save();
                        g.TranslateTransform(12f, 12f);
                        g.RotateTransform(-40f);
                        g.TranslateTransform(-12f, -12f);
                        using (GraphicsPath ep = Round(new RectangleF(4.8f, 8.6f, 14.4f, 7.2f), 2.2f))
                            g.FillPath(b, ep);
                        using (Pen q = new Pen(Segment, 1.8f))
                            g.DrawLine(q, 11.6f, 9.4f, 11.6f, 15f);
                        g.Restore(es);
                        break;
                    case "bucket":
                        // paint drop
                        g.FillEllipse(b, 6.4f, 8.4f, 11.2f, 11.2f);
                        g.FillPolygon(b, new PointF[] { new PointF(12f, 2.2f), new PointF(7.4f, 12.4f), new PointF(16.6f, 12.4f) });
                        break;
                    case "picker":
                        g.DrawLine(p, 6.2f, 17.8f, 15.4f, 8.6f);
                        g.FillEllipse(b, 14.2f, 3.8f, 6f, 6f);
                        g.FillPolygon(b, new PointF[] { new PointF(3.8f, 20.2f), new PointF(5.1f, 16.2f), new PointF(7.8f, 18.9f) });
                        break;
                    case "mirror":
                        using (Pen q = new Pen(c, 1.4f))
                        {
                            q.DashStyle = DashStyle.Dash;
                            q.DashPattern = new float[] { 2.2f, 2.2f };
                            g.DrawLine(q, 12f, 3.4f, 12f, 20.6f);
                        }
                        g.FillPolygon(b, new PointF[] { new PointF(3.4f, 7f), new PointF(9.6f, 12f), new PointF(3.4f, 17f) });
                        g.DrawPolygon(p, new PointF[] { new PointF(20.6f, 7f), new PointF(14.4f, 12f), new PointF(20.6f, 17f) });
                        break;
                    case "flipH":
                        // offset opposite arrows (swap glyph), clearly apart from the mirror icon
                        g.DrawLine(p, 4.4f, 8.4f, 18.4f, 8.4f);
                        g.FillPolygon(b, new PointF[] { new PointF(21.2f, 8.4f), new PointF(16.6f, 5.4f), new PointF(16.6f, 11.4f) });
                        g.DrawLine(p, 19.6f, 15.6f, 5.6f, 15.6f);
                        g.FillPolygon(b, new PointF[] { new PointF(2.8f, 15.6f), new PointF(7.4f, 12.6f), new PointF(7.4f, 18.6f) });
                        break;
                    case "flipV":
                        g.DrawLine(p, 8.4f, 19.6f, 8.4f, 5.6f);
                        g.FillPolygon(b, new PointF[] { new PointF(8.4f, 2.8f), new PointF(5.4f, 7.4f), new PointF(11.4f, 7.4f) });
                        g.DrawLine(p, 15.6f, 4.4f, 15.6f, 18.4f);
                        g.FillPolygon(b, new PointF[] { new PointF(15.6f, 21.2f), new PointF(12.6f, 16.6f), new PointF(18.6f, 16.6f) });
                        break;
                    case "undo":
                    case "redo":
                        bool flip = (n == "redo");
                        GraphicsState s2 = g.Save();
                        if (flip) { g.TranslateTransform(24f, 0f); g.ScaleTransform(-1f, 1f); }
                        g.DrawArc(p, 5.5f, 6.5f, 13f, 13f, 205f, 250f);
                        g.FillPolygon(b, new PointF[] { new PointF(2.6f, 8.2f), new PointF(9.4f, 8.4f), new PointF(4.6f, 13.4f) });
                        g.Restore(s2);
                        break;
                    case "plus":
                        g.DrawLine(p, 12f, 5.5f, 12f, 18.5f);
                        g.DrawLine(p, 5.5f, 12f, 18.5f, 12f);
                        break;
                    case "minus":
                        g.DrawLine(p, 5.5f, 12f, 18.5f, 12f);
                        break;
                    case "grid":
                        using (Pen q = new Pen(c, Math.Max(1f, pw * 0.8f)))
                        {
                            g.DrawRectangle(q, 4f, 4f, 16f, 16f);
                            g.DrawLine(q, 9.3f, 4f, 9.3f, 20f);
                            g.DrawLine(q, 14.7f, 4f, 14.7f, 20f);
                            g.DrawLine(q, 4f, 9.3f, 20f, 9.3f);
                            g.DrawLine(q, 4f, 14.7f, 20f, 14.7f);
                        }
                        break;
                    case "crosshair":
                        using (Pen q = new Pen(c, Math.Max(1f, pw * 0.8f)))
                        {
                            g.DrawEllipse(q, 6.4f, 6.4f, 11.2f, 11.2f);
                            g.DrawLine(q, 12f, 3.2f, 12f, 8.4f);
                            g.DrawLine(q, 12f, 15.6f, 12f, 20.8f);
                            g.DrawLine(q, 3.2f, 12f, 8.4f, 12f);
                            g.DrawLine(q, 15.6f, 12f, 20.8f, 12f);
                        }
                        break;
                    case "layers":
                        g.DrawPolygon(p, new PointF[] { new PointF(12f, 3.4f), new PointF(20.6f, 7.8f), new PointF(12f, 12.2f), new PointF(3.4f, 7.8f) });
                        g.DrawLine(p, 3.4f, 12.2f, 12f, 16.6f);
                        g.DrawLine(p, 20.6f, 12.2f, 12f, 16.6f);
                        g.DrawLine(p, 3.4f, 16.2f, 12f, 20.6f);
                        g.DrawLine(p, 20.6f, 16.2f, 12f, 20.6f);
                        break;
                    case "copy":
                        using (GraphicsPath cp = Round(new RectangleF(3.6f, 3.6f, 9.6f, 9.6f), 2.4f))
                            g.DrawPath(p, cp);
                        using (GraphicsPath cp2 = Round(new RectangleF(10.8f, 10.8f, 9.6f, 9.6f), 2.4f))
                            g.DrawPath(p, cp2);
                        break;
                    case "merge":
                        g.DrawLine(p, 12f, 3.6f, 12f, 12.4f);
                        g.FillPolygon(b, new PointF[] { new PointF(12f, 17.2f), new PointF(7.8f, 11.6f), new PointF(16.2f, 11.6f) });
                        g.DrawLine(p, 4.4f, 20.2f, 19.6f, 20.2f);
                        break;
                    case "up":
                        g.DrawLine(p, 6.2f, 14.8f, 12f, 8.6f);
                        g.DrawLine(p, 17.8f, 14.8f, 12f, 8.6f);
                        break;
                    case "down":
                        g.DrawLine(p, 6.2f, 9.2f, 12f, 15.4f);
                        g.DrawLine(p, 17.8f, 9.2f, 12f, 15.4f);
                        break;
                    case "lock":
                    case "unlock":
                        // padlock; the open variant leaves the right shackle leg out
                        using (GraphicsPath bp = Round(new RectangleF(5f, 10.4f, 14f, 9.6f), 2.2f))
                            g.DrawPath(p, bp);
                        using (GraphicsPath sp = new GraphicsPath())
                        {
                            sp.AddArc(8f, 3.2f, 8f, 9f, 180f, 180f);
                            g.DrawPath(p, sp);
                        }
                        g.DrawLine(p, 8f, 7.6f, 8f, 10.4f);
                        if (n == "lock") g.DrawLine(p, 16f, 7.6f, 16f, 10.4f);
                        g.FillEllipse(b, 11f, 13.6f, 2.2f, 2.2f);
                        break;
                    case "rotate":
                        g.DrawArc(p, 4.4f, 4.4f, 15.2f, 15.2f, 40f, 265f);
                        g.FillPolygon(b, new PointF[] { new PointF(16.4f, 2.4f), new PointF(21.4f, 5.6f), new PointF(16.2f, 8.4f) });
                        break;
                    case "eye":
                    case "eyeOff":
                        using (GraphicsPath ep = new GraphicsPath())
                        {
                            ep.AddBezier(2.8f, 12f, 5.8f, 7.4f, 18.2f, 7.4f, 21.2f, 12f);
                            ep.AddBezier(21.2f, 12f, 18.2f, 16.6f, 5.8f, 16.6f, 2.8f, 12f);
                            ep.CloseFigure();
                            g.DrawPath(p, ep);
                        }
                        g.FillEllipse(b, 9.6f, 9.6f, 4.8f, 4.8f);
                        if (n == "eyeOff")
                        {
                            using (Pen q = new Pen(c, Math.Max(1f, pw * 0.9f)))
                                g.DrawLine(q, 4.4f, 19.6f, 19.6f, 4.4f);
                        }
                        break;
                    case "download":
                        g.DrawLine(p, 12f, 4f, 12f, 15f);
                        g.DrawLine(p, 7.4f, 10.6f, 12f, 15.2f);
                        g.DrawLine(p, 16.6f, 10.6f, 12f, 15.2f);
                        using (Pen q = new Pen(c, pw))
                        {
                            q.StartCap = LineCap.Round; q.EndCap = LineCap.Round;
                            g.DrawLine(q, 4.6f, 19.6f, 19.4f, 19.6f);
                        }
                        break;
                    case "folder":
                        g.DrawPath(p, FolderPath(3.4f, 6.4f, 17.2f, 12.8f));
                        break;
                    case "trash":
                        g.DrawLine(p, 3.8f, 6.6f, 20.2f, 6.6f);
                        g.DrawLine(p, 9f, 3.8f, 15f, 3.8f);
                        using (Pen q = new Pen(c, Math.Max(1f, pw * 0.9f)))
                        {
                            q.StartCap = LineCap.Round; q.EndCap = LineCap.Round; q.LineJoin = LineJoin.Round;
                            g.DrawPath(q, TrashPath());
                        }
                        break;
                    case "image":
                        using (Pen q = new Pen(c, pw * 0.9f))
                            g.DrawRectangle(q, 3.6f, 4.6f, 16.8f, 14.8f);
                        g.FillEllipse(b, 7f, 8f, 2.8f, 2.8f);
                        g.FillPolygon(b, new PointF[] { new PointF(5f, 18.2f), new PointF(10.4f, 11.6f), new PointF(14.6f, 16.4f), new PointF(17.2f, 13.6f), new PointF(19f, 18.2f) });
                        break;
                    case "discord":
                        // Filled Clyde mark: the official outline plus two eye cut-outs
                        // (alternate fill turns the nested eye figures into holes).
                        using (GraphicsPath dp = new GraphicsPath(FillMode.Alternate))
                        {
                            dp.StartFigure();
                            dp.AddBezier(19.98f, 4.74f, 18.47f, 4.05f, 16.88f, 3.56f, 15.24f, 3.28f);
                            dp.AddBezier(15.24f, 3.28f, 15.01f, 3.68f, 14.81f, 4.1f, 14.63f, 4.52f);
                            dp.AddBezier(14.63f, 4.52f, 12.88f, 4.26f, 11.11f, 4.26f, 9.36f, 4.52f);
                            dp.AddBezier(9.36f, 4.52f, 9.18f, 4.1f, 8.98f, 3.69f, 8.76f, 3.28f);
                            dp.AddBezier(8.76f, 3.28f, 7.12f, 3.56f, 5.52f, 4.06f, 4.01f, 4.75f);
                            dp.AddBezier(4.01f, 4.75f, 1f, 9.19f, 0.19f, 13.52f, 0.6f, 17.79f);
                            dp.AddBezier(0.6f, 17.79f, 2.36f, 19.09f, 4.32f, 20.08f, 6.42f, 20.72f);
                            dp.AddBezier(6.42f, 20.72f, 6.89f, 20.08f, 7.31f, 19.41f, 7.66f, 18.71f);
                            dp.AddBezier(7.66f, 18.71f, 6.98f, 18.45f, 6.33f, 18.14f, 5.7f, 17.77f);
                            dp.AddBezier(5.7f, 17.77f, 5.87f, 17.65f, 6.03f, 17.53f, 6.18f, 17.41f);
                            dp.AddBezier(6.18f, 17.41f, 9.87f, 19.14f, 14.13f, 19.14f, 17.82f, 17.41f);
                            dp.AddBezier(17.82f, 17.41f, 17.98f, 17.54f, 18.14f, 17.66f, 18.3f, 17.77f);
                            dp.AddBezier(18.3f, 17.77f, 17.67f, 18.14f, 17.01f, 18.45f, 16.33f, 18.71f);
                            dp.AddBezier(16.33f, 18.71f, 16.69f, 19.41f, 17.11f, 20.08f, 17.58f, 20.72f);
                            dp.AddBezier(17.58f, 20.72f, 19.67f, 20.08f, 21.64f, 19.1f, 23.4f, 17.8f);
                            dp.AddBezier(23.4f, 17.8f, 23.88f, 12.84f, 22.59f, 8.55f, 19.98f, 4.74f);
                            dp.CloseFigure();
                            dp.StartFigure();
                            dp.AddBezier(8.18f, 15.17f, 7.05f, 15.17f, 6.11f, 14.14f, 6.11f, 12.87f);
                            dp.AddBezier(6.11f, 12.87f, 6.11f, 11.61f, 7.01f, 10.57f, 8.18f, 10.57f);
                            dp.AddBezier(8.18f, 10.57f, 9.34f, 10.57f, 10.27f, 11.61f, 10.25f, 12.87f);
                            dp.AddBezier(10.25f, 12.87f, 10.23f, 14.14f, 9.34f, 15.17f, 8.18f, 15.17f);
                            dp.CloseFigure();
                            dp.StartFigure();
                            dp.AddBezier(15.82f, 15.17f, 14.68f, 15.17f, 13.75f, 14.14f, 13.75f, 12.87f);
                            dp.AddBezier(13.75f, 12.87f, 13.75f, 11.61f, 14.66f, 10.57f, 15.82f, 10.57f);
                            dp.AddBezier(15.82f, 10.57f, 16.99f, 10.57f, 17.91f, 11.61f, 17.89f, 12.87f);
                            dp.AddBezier(17.89f, 12.87f, 17.87f, 14.14f, 16.98f, 15.17f, 15.82f, 15.17f);
                            dp.CloseFigure();
                            g.FillPath(b, dp);
                        }
                        break;
                    case "check":
                        g.DrawLine(p, 5.4f, 12.4f, 10f, 17f);
                        g.DrawLine(p, 10f, 17f, 18.6f, 7f);
                        break;
                    case "close":
                        g.DrawLine(p, 6.5f, 6.5f, 17.5f, 17.5f);
                        g.DrawLine(p, 17.5f, 6.5f, 6.5f, 17.5f);
                        break;
                    case "min":
                        g.DrawLine(p, 6f, 12f, 18f, 12f);
                        break;
                    case "max":
                        using (Pen q = new Pen(c, Math.Max(1f, pw * 0.85f)))
                            g.DrawRectangle(q, 6.5f, 6.5f, 11f, 11f);
                        break;
                    case "restore":
                        using (Pen q = new Pen(c, Math.Max(1f, pw * 0.85f)))
                        {
                            g.DrawRectangle(q, 4.6f, 8.4f, 10.4f, 10.4f);
                            g.DrawLine(q, 8.2f, 8.2f, 8.2f, 5f);
                            g.DrawLine(q, 8.2f, 5f, 18.6f, 5f);
                            g.DrawLine(q, 18.6f, 5f, 18.6f, 15.4f);
                            g.DrawLine(q, 18.6f, 15.4f, 15.2f, 15.4f);
                        }
                        break;
                    case "crown":
                        g.FillPolygon(b, new PointF[] { new PointF(3.6f, 17.6f), new PointF(5.4f, 7.4f), new PointF(9.6f, 11.8f), new PointF(12f, 5.6f), new PointF(14.4f, 11.8f), new PointF(18.6f, 7.4f), new PointF(20.4f, 17.6f) });
                        break;
                    case "sparkle":
                        g.FillPolygon(b, new PointF[] { new PointF(12f, 3.4f), new PointF(14.2f, 9.8f), new PointF(20.6f, 12f), new PointF(14.2f, 14.2f), new PointF(12f, 20.6f), new PointF(9.8f, 14.2f), new PointF(3.4f, 12f), new PointF(9.8f, 9.8f) });
                        break;
                    case "gear":
                        using (Pen q = new Pen(c, Math.Max(1.4f, pw * 1.1f)))
                        {
                            q.StartCap = LineCap.Round;
                            q.EndCap = LineCap.Round;
                            for (int i = 0; i < 8; i++)
                            {
                                GraphicsState gs = g.Save();
                                g.TranslateTransform(12f, 12f);
                                g.RotateTransform(i * 45f);
                                g.DrawLine(q, 0f, -7.4f, 0f, -10.2f);
                                g.Restore(gs);
                            }
                            g.DrawEllipse(q, 6.4f, 6.4f, 11.2f, 11.2f);
                            g.DrawEllipse(q, 10.3f, 10.3f, 3.4f, 3.4f);
                        }
                        break;
                    case "sun":
                        g.DrawEllipse(p, 8.2f, 8.2f, 7.6f, 7.6f);
                        for (int i = 0; i < 8; i++)
                        {
                            GraphicsState gs = g.Save();
                            g.TranslateTransform(12f, 12f);
                            g.RotateTransform(i * 45f);
                            g.DrawLine(p, 0f, -6.6f, 0f, -9.4f);
                            g.Restore(gs);
                        }
                        break;
                    case "moon":
                        using (GraphicsPath mp = new GraphicsPath())
                        {
                            mp.AddArc(4f, 3.4f, 15f, 15f, 60f, 250f);
                            mp.AddArc(9.4f, 3.4f, 12.4f, 14.2f, 300f, -190f);
                            mp.CloseFigure();
                            g.FillPath(b, mp);
                        }
                        break;
                    case "brush":
                        g.DrawLine(p, 6f, 18f, 13.6f, 10.4f);
                        g.FillPolygon(b, new PointF[] { new PointF(14.6f, 4.4f), new PointF(19.6f, 9.4f), new PointF(16.8f, 12.2f), new PointF(11.8f, 7.2f) });
                        g.FillPolygon(b, new PointF[] { new PointF(3.4f, 20.6f), new PointF(4.6f, 16.2f), new PointF(7.8f, 19.4f) });
                        break;
                }
            }
            g.Restore(st);
        }

        static GraphicsPath FolderPath(float x, float y, float w, float h)
        {
            GraphicsPath p = new GraphicsPath();
            float tab = h * 0.22f;
            p.AddLine(x, y + h, x, y + tab * 0.4f);
            p.AddLine(x, y + tab * 0.4f, x + w * 0.34f, y + tab * 0.4f);
            p.AddLine(x + w * 0.34f, y + tab * 0.4f, x + w * 0.44f, y + tab * 1.5f);
            p.AddLine(x + w * 0.44f, y + tab * 1.5f, x + w, y + tab * 1.5f);
            p.AddLine(x + w, y + tab * 1.5f, x + w, y + h);
            p.CloseFigure();
            return p;
        }

        static GraphicsPath TrashPath()
        {
            GraphicsPath p = new GraphicsPath();
            p.AddLine(6.2f, 7.6f, 6.9f, 20.2f);
            p.AddLine(6.9f, 20.2f, 17.1f, 20.2f);
            p.AddLine(17.1f, 20.2f, 17.8f, 7.6f);
            return p;
        }

        // ---------------------------------------------------------------- misc
        /// <summary>Paints a rounded button face and returns the content colour.</summary>
        public static Color ButtonFace(Graphics g, RectangleF r, float rad, bool gold, bool hot, bool down)
        {
            if (gold)
            {
                Color fill = down ? Color.FromArgb(226, 176, 62) : (hot ? GoldHi : Gold);
                FillRound(g, r, rad, fill);
                return GoldText;
            }
            // Outlined tonal button: transparent until hovered, then a quiet surface step.
            FillRound(g, r, rad, hot ? HoverSoft : Color.FromArgb(0, 0, 0, 0));
            StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f),
                rad, hot ? StrokeHot : Stroke, 1f);
            return hot ? Text : BtnText;
        }

        public static Color Mix(Color a, Color b, float t)
        {
            if (t < 0f) t = 0f; if (t > 1f) t = 1f;
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        public static Color Gold14 { get { return Color.FromArgb(38, 247, 197, 74); } }
        public static Color Gold28 { get { return Color.FromArgb(70, 247, 197, 74); } }
    }
}
