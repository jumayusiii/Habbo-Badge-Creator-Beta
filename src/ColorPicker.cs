using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>
    /// Beta colour sheet — the app's own picker, and the replacement for Windows' stock colour
    /// dialog, which ignored the palette completely: a pale grey window sitting on the dark
    /// editor with sliders that matched neither the theme nor the rest of the UI.
    ///
    /// It is built like the custom theme designer: one saturation/value square with the hue rail
    /// beside it, a hex field with a live R/G/B readout, before/after swatches and the editor's
    /// own palette. Every tone comes from the active palette tokens, so it fits Dark, Light, Navy
    /// and Custom, and it eases in and out with the same fade as the other sheets. Picking
    /// previews on the editor behind the sheet; Cancel puts the old colour back.
    /// </summary>
    internal class ColorSheetForm : Form
    {
        /// <summary>Live preview while the sheet is open; may be null.</summary>
        public Action<Color> Preview;
        /// <summary>The colour to commit — only meaningful once Saved is true.</summary>
        public Color Result;
        public bool Saved;

        readonly Color before;      // what the editor had when the sheet opened
        Color current;
        float hue, sat, val;
        bool dragSv, dragHue;
        int hoverSwatch = -1;
        bool hoverBefore;

        FieldBox hex;
        FlatBtn cancelBtn, useBtn;
        WinBtn closeBtn;

        /// <summary>Same palette the editor's toolbar offers, so both places agree.</summary>
        static readonly int[] Swatches = MainForm.Palette;

        public ColorSheetForm(Color initial)
            : this(initial, null)
        {
        }

        public ColorSheetForm(Color initial, Action<Color> preview)
        {
            before = Color.FromArgb(255, initial.R, initial.G, initial.B);
            current = before;
            Preview = preview;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = T.Card;
            ClientSize = new Size(T.P(520), T.P(468));
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            BuildUi();
            // the sheet is not on screen yet, so the constructor only syncs the square and rail
            float h, s, v;
            T.RgbToHsv(current, out h, out s, out v);
            hue = h; sat = s; val = v;
            hex.Value = AppSettings.ToHex(current);

            SheetFade.Attach(this);
        }

        void BuildUi()
        {
            hex = new FieldBox(false);
            hex.BackColor = BackColor;
            hex.SetPlaceholder("#RRGGBB");
            hex.Box.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CommitHex(); }
            };
            hex.Box.LostFocus += delegate { CommitHex(); };
            Controls.Add(hex);

            cancelBtn = new FlatBtn();
            cancelBtn.Label = L.T("Cancel", "Abbrechen");
            cancelBtn.BackColor = BackColor;
            cancelBtn.Click += delegate { Close(); };
            Controls.Add(cancelBtn);

            useBtn = new FlatBtn();
            useBtn.Label = L.T("Use colour", "Farbe übernehmen");
            useBtn.Icon = "check";
            useBtn.Primary = true;
            useBtn.BackColor = BackColor;
            useBtn.Click += delegate { Use(); };
            Controls.Add(useBtn);

            closeBtn = new WinBtn();
            closeBtn.Icon = "close";
            closeBtn.BackColor = BackColor;
            closeBtn.Click += delegate { Close(); };
            Controls.Add(closeBtn);

            LayoutAll();
        }

        // ------------------------------------------------------------- geometry
        int Pad { get { return T.P(22); } }

        /// <summary>Left edge of the right-hand column: swatches, hex field and readout.</summary>
        int ColX { get { return Pad + T.P(352); } }

        RectangleF SvRect { get { return new RectangleF(Pad, T.P(96), T.P(300), T.P(210)); } }

        RectangleF HueRect { get { return new RectangleF(Pad + T.P(312), T.P(96), T.P(22), T.P(210)); } }

        Rectangle BeforeRect { get { return new Rectangle(ColX, T.P(112), T.P(58), T.P(58)); } }

        Rectangle NewRect { get { return new Rectangle(ColX + T.P(66), T.P(112), T.P(58), T.P(58)); } }

        Rectangle SwatchRect(int i)
        {
            int cols = 14, s = T.P(22), g = T.P(6);
            return new Rectangle(Pad + (i % cols) * (s + g), T.P(342) + (i / cols) * (s + g), s, s);
        }

        void LayoutAll()
        {
            if (hex == null) return;   // OnResize fires while the constructor is still setting sizes
            int pad = Pad;
            hex.SetBounds(ColX, T.P(200), T.P(124), T.P(36));

            int bh = T.P(38);
            int by = ClientSize.Height - pad - bh;
            useBtn.SetBounds(ClientSize.Width - pad - T.P(132), by, T.P(132), bh);
            cancelBtn.SetBounds(useBtn.Left - T.P(10) - T.P(108), by, T.P(108), bh);
            closeBtn.SetBounds(ClientSize.Width - T.P(44), 0, T.P(44), T.P(38));
        }

        // ------------------------------------------------------------- picking
        /// <summary>Sets the picked colour, follows on the square and rail, and previews it.</summary>
        void Apply(Color c)
        {
            current = Color.FromArgb(255, c.R, c.G, c.B);
            float h, s, v;
            T.RgbToHsv(current, out h, out s, out v);
            // black, white and greys carry no hue of their own: leave the rail where the user put
            // it instead of snapping it back to red
            if (v > 0f && s > 0f) hue = h;
            sat = s;
            val = v;
            if (hex != null) hex.Value = AppSettings.ToHex(current);
            if (Preview != null) Preview(current);
            Invalidate();
        }

        void CommitHex()
        {
            if (hex == null) return;
            string t = hex.Value.Trim();
            if (t.Length == 0) { hex.Value = AppSettings.ToHex(current); return; }
            Color c = AppSettings.ParseHex(t, current);
            if (c.ToArgb() == current.ToArgb()) return;
            Apply(c);
        }

        void Use()
        {
            Result = current;
            Saved = true;
            Close();
        }

        void PickSv(Point p)
        {
            RectangleF r = SvRect;
            sat = Math.Max(0f, Math.Min(1f, (p.X - r.X) / r.Width));
            val = 1f - Math.Max(0f, Math.Min(1f, (p.Y - r.Y) / r.Height));
            Apply(T.HsvToRgb(hue, sat, val));
        }

        void PickHue(Point p)
        {
            RectangleF r = HueRect;
            hue = Math.Max(0f, Math.Min(1f, (p.Y - r.Y) / r.Height)) * 360f;
            Apply(T.HsvToRgb(hue, sat, val));
        }

        int SwatchAt(Point p)
        {
            for (int i = 0; i < Swatches.Length; i++)
                if (SwatchRect(i).Contains(p)) return i;
            return -1;
        }

        static Color SwatchColor(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        }

        // ------------------------------------------------------------- mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < T.P(44))
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
                return;
            }
            if (e.Button != MouseButtons.Left) return;

            if (SvRect.Contains(e.Location)) { dragSv = true; Capture = true; PickSv(e.Location); return; }
            if (HueRect.Contains(e.Location)) { dragHue = true; Capture = true; PickHue(e.Location); return; }
            if (BeforeRect.Contains(e.Location)) { Apply(before); return; }
            if (NewRect.Contains(e.Location)) return;

            int chip = SwatchAt(e.Location);
            if (chip >= 0) Apply(SwatchColor(Swatches[chip]));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragSv) { PickSv(e.Location); return; }
            if (dragHue) { PickHue(e.Location); return; }

            int over = SwatchAt(e.Location);
            bool overBefore = BeforeRect.Contains(e.Location);
            if (over != hoverSwatch || overBefore != hoverBefore)
            {
                hoverSwatch = over;
                hoverBefore = overBefore;
                Invalidate();
            }
            Cursor = over >= 0 || overBefore ? Cursors.Hand
                   : SvRect.Contains(e.Location) || HueRect.Contains(e.Location) ? Cursors.Cross
                   : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (dragSv || dragHue) return;
            if (hoverSwatch != -1 || hoverBefore)
            {
                hoverSwatch = -1;
                hoverBefore = false;
                Invalidate();
            }
            Cursor = Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragSv = false;
            dragHue = false;
            Capture = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Enter commits — unless the caret is in the hex field, where it parses the value
            if ((keyData & Keys.KeyCode) == Keys.Escape) { Close(); return true; }
            if ((keyData & Keys.KeyCode) == Keys.Enter && (hex == null || !hex.Box.Focused))
            {
                Use();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------- paint
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.PaintSheet(g, ClientRectangle, BackColor);

            int pad = Pad;
            int tile = T.P(30);
            T.LogoTile(g, new RectangleF(pad, T.P(15), tile, tile), T.F(7f));
            T.Str(g, L.T("Colour", "Farbe"), T.FPageTitle, T.Text,
                new Rectangle(pad + tile + T.P(12), T.P(11), T.P(240), T.P(28)), T.FL);
            T.Str(g, L.T("Brush colour — the badge behind this sheet updates while you pick",
                         "Pinselfarbe – das Badge hinter diesem Fenster folgt der Auswahl"),
                T.FSmall, T.Muted, new Rectangle(pad + tile + T.P(12), T.P(37), T.P(380), T.P(18)), T.FL);

            // the picker is new, so it carries its own BETA tag like the theme designer
            string beta = "BETA";
            int bw = T.Meas(beta, T.FLabel).Width + T.P(14);
            RectangleF tag = new RectangleF(ClientSize.Width - T.P(56) - bw, T.P(18), bw, T.P(20));
            T.StrokeRound(g, tag, T.F(10f), T.Gold, 1f);
            T.Str(g, beta, T.FLabel, T.Gold, Rectangle.Round(tag), T.FC);
            T.HLine(g, 0, ClientSize.Width, T.P(70), T.LineSoft);

            T.PaintSvSquare(g, SvRect, hue, sat, val, current);
            T.PaintHueRail(g, HueRect, hue);

            // before / after: a stray pick can be compared, and the old colour is one click away
            T.Str(g, L.T("BEFORE", "VORHER"), T.FLabel, T.Dim, AboveChip(BeforeRect), T.FC);
            T.Str(g, L.T("NEW", "NEU"), T.FLabel, T.Dim, AboveChip(NewRect), T.FC);
            DrawChip(g, BeforeRect, before, hoverBefore);
            DrawChip(g, NewRect, current, false);

            T.Str(g, "HEX", T.FLabel, T.Dim,
                new Rectangle(ColX, T.P(182), T.P(124), T.P(14)), T.FL);
            T.Str(g, "R " + current.R + "   G " + current.G + "   B " + current.B, T.FMicro, T.Muted,
                new Rectangle(ColX, T.P(242), T.P(124), T.P(16)), T.FL);

            string head = L.T("PALETTE", "PALETTE");
            T.Str(g, head, T.FLabel, T.Dim, new Rectangle(pad, T.P(318), T.P(240), T.P(16)), T.FL);
            T.Str(g, L.T("Click a swatch to start from it", "Feld anklicken, um davon zu starten"),
                T.FMicro, T.Dim,
                new Rectangle(pad + T.Meas(head, T.FLabel).Width + T.P(10), T.P(318),
                    ClientSize.Width - pad * 2 - T.Meas(head, T.FLabel).Width - T.P(10), T.P(16)), T.FR);
            DrawPalette(g);

            // wrapped, not clipped: the footer buttons leave only the left half of the width free
            T.Wrap(g, L.T("Picking previews on the badge behind — Cancel puts the old colour back.",
                          "Die Auswahl erscheint live auf dem Badge dahinter – Abbrechen stellt die alte Farbe her."),
                T.FMicro, T.Dim,
                new Rectangle(pad, ClientSize.Height - T.P(58), T.P(196), T.P(44)));

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Rim, 1f))
                g.DrawPath(pen, rim);
        }

        Rectangle AboveChip(Rectangle chip)
        {
            return new Rectangle(chip.X, T.P(92), chip.Width, T.P(16));
        }

        void DrawChip(Graphics g, Rectangle r, Color c, bool hot)
        {
            RectangleF rr = new RectangleF(r.X, r.Y, r.Width, r.Height);
            T.FillRound(g, rr, T.F(10f), c);
            bool live = c.ToArgb() == current.ToArgb();
            T.StrokeRound(g,
                new RectangleF(rr.X + 0.5f, rr.Y + 0.5f, rr.Width - 1f, rr.Height - 1f), T.F(10f),
                live ? T.Gold : (hot ? T.StrokeHot : T.SwatchRing), live ? 2f : 1f);
        }

        void DrawPalette(Graphics g)
        {
            for (int i = 0; i < Swatches.Length; i++)
            {
                Rectangle r = SwatchRect(i);
                Color c = SwatchColor(Swatches[i]);
                T.FillRound(g, new RectangleF(r.X, r.Y, r.Width, r.Height), T.F(6f), c);
                bool on = c.ToArgb() == current.ToArgb();
                bool hot = i == hoverSwatch;
                T.StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f),
                    T.F(6f), on ? T.Gold : (hot ? T.StrokeHot : T.SwatchRing), on ? 2f : 1f);
                if (on)
                {
                    float cs = T.F(10f);
                    T.Icon(g, "check",
                        new RectangleF(r.X + (r.Width - cs) / 2f, r.Y + (r.Height - cs) / 2f, cs, cs),
                        T.Lum(c) > 0.55f ? Color.FromArgb(32, 30, 24) : Color.White);
                }
            }
        }

        // ------------------------------------------------------------- window
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyWindowShape();
            LayoutAll();
        }

        void ApplyWindowShape()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyWindowShape();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
