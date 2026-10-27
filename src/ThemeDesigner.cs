using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>
    /// Beta custom-theme designer. Three targets (surface, accent, text) share one
    /// saturation/value square and hue rail, plus a hex box and a preset palette. Every
    /// change previews live on the app behind the sheet.
    /// </summary>
    internal class ThemeDesignerForm : Form
    {
        enum Target { Surface, Accent, Text }

        public Color Surface, Accent, Ink;
        public bool Saved;
        public Action<Color, Color, Color> Preview;
        public List<ThemePreset> Presets = new List<ThemePreset>();

        const int MaxPresets = 5;
        Target target = Target.Surface;
        float hue, sat, val;
        bool dragSv, dragHue;
        bool savingPreset;
        int hoverPreset = -1;
        int renamingPreset = -1;
        int pressPreset = -1;
        int dragPreset = -1;
        int dragTarget = -1;
        Point pressPoint, dragPoint;
        FieldBox hex, nameField;
        FlatBtn resetBtn, cancelBtn, saveBtn, presetSaveBtn, presetOkBtn;
        WinBtn closeBtn, presetCancelBtn;

        static readonly int[] Swatches = new int[]
        {
            0x0B1220, 0x12203A, 0x18263F, 0x1E2A3A, 0x243447, 0x2C3A4E,
            0xF3F5F9, 0xFFFFFF, 0xE7ECF4, 0xCBD5E4, 0x8FA1B8, 0x5C7086,
            0xF6C452, 0xFF9F43, 0xF2566B, 0xD6336C, 0x9C7CF4, 0x5B3FBF,
            0x4FD1C5, 0x2FA37A, 0x6FBF46, 0x1E7F79, 0x3B82F6, 0x22D3EE
        };

        public ThemeDesignerForm(Color surface, Color accent, Color text)
            : this(surface, accent, text, null)
        {
        }

        public ThemeDesignerForm(Color surface, Color accent, Color text, List<ThemePreset> presets)
        {
            Surface = surface;
            Accent = accent;
            Ink = text;
            if (presets != null)
            {
                // older settings may carry more than the row can show; keep the newest five
                for (int i = 0; i < presets.Count && Presets.Count < MaxPresets; i++)
                    Presets.Add(presets[i].Clone());
            }

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = T.Card;
            ClientSize = new Size(T.P(560), T.P(620));
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            BuildUi();
            SyncHsv();
        }

        void BuildUi()
        {
            hex = new FieldBox(false);
            hex.BackColor = BackColor;
            hex.SetPlaceholder("#RRGGBB");
            hex.Value = AppSettings.ToHex(Current);
            hex.Box.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CommitHex(); }
            };
            hex.Box.LostFocus += delegate { CommitHex(); };
            Controls.Add(hex);

            resetBtn = new FlatBtn();
            resetBtn.Label = L.T("Reset", "Zurücksetzen");
            resetBtn.BackColor = BackColor;
            resetBtn.Click += delegate
            {
                Surface = Color.FromArgb(18, 32, 58);
                Accent = Color.FromArgb(246, 196, 82);
                Ink = Color.FromArgb(230, 239, 252);
                SyncHsv();
                Changed();
            };
            Controls.Add(resetBtn);

            cancelBtn = new FlatBtn();
            cancelBtn.Label = L.T("Cancel", "Abbrechen");
            cancelBtn.BackColor = BackColor;
            cancelBtn.Click += delegate { Close(); };
            Controls.Add(cancelBtn);

            saveBtn = new FlatBtn();
            saveBtn.Label = L.T("Use theme", "Design übernehmen");
            saveBtn.Icon = "check";
            saveBtn.Primary = true;
            saveBtn.BackColor = BackColor;
            saveBtn.Click += delegate { Saved = true; Close(); };
            Controls.Add(saveBtn);

            closeBtn = new WinBtn();
            closeBtn.Icon = "close";
            closeBtn.BackColor = BackColor;
            closeBtn.Click += delegate { Close(); };
            Controls.Add(closeBtn);

            // presets: save the current trio under a name, click a chip to recall it
            presetSaveBtn = new FlatBtn();
            presetSaveBtn.Label = L.T("Save preset", "Vorlage sichern");
            presetSaveBtn.Icon = "plus";
            presetSaveBtn.BackColor = BackColor;
            presetSaveBtn.Click += delegate { BeginSavePreset(); };
            Controls.Add(presetSaveBtn);

            nameField = new FieldBox(false);
            nameField.BackColor = BackColor;
            nameField.SetPlaceholder(L.T("Preset name", "Name der Vorlage"));
            nameField.Box.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CommitPreset(); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CancelSavePreset(); }
            };
            nameField.Visible = false;
            Controls.Add(nameField);

            presetOkBtn = new FlatBtn();
            presetOkBtn.Label = L.T("Save", "Sichern");
            presetOkBtn.Icon = "check";
            presetOkBtn.Primary = true;
            presetOkBtn.BackColor = BackColor;
            presetOkBtn.Click += delegate { CommitPreset(); };
            presetOkBtn.Visible = false;
            Controls.Add(presetOkBtn);

            presetCancelBtn = new WinBtn();
            presetCancelBtn.Icon = "close";
            presetCancelBtn.BackColor = BackColor;
            presetCancelBtn.Click += delegate { CancelSavePreset(); };
            presetCancelBtn.Visible = false;
            Controls.Add(presetCancelBtn);

            LayoutAll();
        }

        void LayoutAll()
        {
            if (hex == null) return;
            int pad = Pad;
            int w = ClientSize.Width - pad * 2;
            int by = ClientSize.Height - pad - T.P(38);
            saveBtn.SetBounds(ClientSize.Width - pad - T.P(132), by, T.P(132), T.P(38));
            cancelBtn.SetBounds(saveBtn.Left - T.P(10) - T.P(108), by, T.P(108), T.P(38));
            resetBtn.SetBounds(pad, by, T.P(96), T.P(38));
            closeBtn.SetBounds(ClientSize.Width - T.P(44), 0, T.P(44), T.P(38));

            hex.SetBounds(pad + T.P(382), T.P(204), T.P(134), T.P(36));

            presetSaveBtn.SetBounds(ClientSize.Width - pad - T.P(132), T.P(464), T.P(132), T.P(26));
            presetSaveBtn.Visible = !savingPreset && Presets.Count < MaxPresets;

            int rowY = T.P(496), rowH = T.P(32);
            int cancelW = T.P(30), okW = T.P(78), gap = T.P(8);
            nameField.SetBounds(pad, rowY, w - okW - cancelW - gap * 2, rowH);
            presetOkBtn.SetBounds(nameField.Right + gap, rowY, okW, rowH);
            presetCancelBtn.SetBounds(presetOkBtn.Right + gap, rowY, cancelW, rowH);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyWindowShape();
            LayoutAll();
        }

        /// <summary>Esc closes without applying — or leaves the preset name row first.</summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button != MouseButtons.Left || savingPreset) return;
            int i = PresetAt(e.Location);
            if (i < 0 || PresetRemoveRect(i).Contains(e.Location)) return;
            pressPreset = -1;
            dragPreset = -1;
            BeginRenamePreset(i);
        }

        /// <summary>Esc closes without applying — or leaves the preset name row first.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                if (savingPreset) CancelSavePreset();
                else Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyWindowShape();
        }

        void ApplyWindowShape()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        int Pad { get { return T.P(22); } }

        RectangleF SvRect { get { return new RectangleF(Pad, T.P(176), T.P(342), T.P(186)); } }

        RectangleF HueRect { get { return new RectangleF(Pad + T.P(354), T.P(176), T.P(18), T.P(186)); } }

        Rectangle TargetRect(int i)
        {
            int w = (ClientSize.Width - Pad * 2 - T.P(12)) / 3;
            return new Rectangle(Pad + i * (w + T.P(6)), T.P(90), w, T.P(64));
        }

        Rectangle SwatchRect(int i)
        {
            int cols = 12, s = T.P(24), g = T.P(6);
            int row = i / cols, col = i % cols;
            return new Rectangle(Pad + col * (s + g), T.P(404) + row * (s + g), s, s);
        }

        Color Current
        {
            get { return target == Target.Surface ? Surface : target == Target.Accent ? Accent : Ink; }
        }

        void SetCurrent(Color c)
        {
            if (target == Target.Surface) Surface = c;
            else if (target == Target.Accent) Accent = c;
            else Ink = c;
            SyncHsv();
            Changed();
        }

        void SyncHsv()
        {
            float h, s, v;
            T.RgbToHsv(Current, out h, out s, out v);
            hue = h; sat = s; val = v;
            if (hex != null) hex.Value = AppSettings.ToHex(Current);
        }

        void Changed()
        {
            if (hex != null) hex.Value = AppSettings.ToHex(Current);
            if (Preview != null) Preview(Surface, Accent, Ink);
            ApplyPalette();
        }

        /// <summary>Repaints the sheet's own chrome after a live palette change.</summary>
        void ApplyPalette()
        {
            BackColor = T.Card;
            foreach (Control c in Controls) c.BackColor = T.Card;
            if (hex != null) hex.ApplyTheme();
            if (nameField != null) nameField.ApplyTheme();
            Invalidate(true);
        }

        // ------------------------------------------------------------- presets
        Rectangle PresetRect(int i)
        {
            int gap = T.P(6);
            int w = (ClientSize.Width - Pad * 2 - gap * (MaxPresets - 1)) / MaxPresets;
            return new Rectangle(Pad + i * (w + gap), T.P(496), w, T.P(32));
        }

        Rectangle PresetRemoveRect(int i)
        {
            Rectangle r = PresetRect(i);
            return new Rectangle(r.Right - T.P(22), r.Y, T.P(22), r.Height);
        }

        bool PresetIsCurrent(ThemePreset p)
        {
            return p.SurfaceColor.ToArgb() == Surface.ToArgb()
                && p.AccentColor.ToArgb() == Accent.ToArgb()
                && p.TextColor.ToArgb() == Ink.ToArgb();
        }

        string SuggestedPresetName()
        {
            return L.T("Preset ", "Vorlage ") + (Presets.Count + 1);
        }

        /// <summary>The chip under the pointer, or -1. Chips are inert while the name row is open.</summary>
        int PresetAt(Point p)
        {
            if (savingPreset) return -1;
            for (int i = 0; i < Presets.Count; i++)
                if (PresetRect(i).Contains(p)) return i;
            return -1;
        }

        /// <summary>Nearest chip slot for a drag at x, clamped to the presets that exist.</summary>
        int PresetDropIndex(int x)
        {
            if (Presets.Count == 0) return -1;
            int gap = T.P(6);
            int w = (ClientSize.Width - Pad * 2 - gap * (MaxPresets - 1)) / MaxPresets;
            int step = w + gap;
            int idx = (int)Math.Floor((x - Pad + step / 2f) / step);
            return Math.Max(0, Math.Min(Presets.Count - 1, idx));
        }

        void ApplyPreset(int i)
        {
            if (i < 0 || i >= Presets.Count) return;
            Surface = Presets[i].SurfaceColor;
            Accent = Presets[i].AccentColor;
            Ink = Presets[i].TextColor;
            SyncHsv();
            Changed();
        }

        void RemovePreset(int i)
        {
            if (i < 0 || i >= Presets.Count) return;
            Presets.RemoveAt(i);
            hoverPreset = -1;
            pressPreset = -1;
            dragPreset = -1;
            dragTarget = -1;
            LayoutAll();
            Invalidate(true);
        }

        void BeginSavePreset()
        {
            if (savingPreset || Presets.Count >= MaxPresets) return;
            savingPreset = true;
            renamingPreset = -1;
            nameField.Value = SuggestedPresetName();
            ShowNameRow();
        }

        void BeginRenamePreset(int i)
        {
            if (savingPreset || i < 0 || i >= Presets.Count) return;
            savingPreset = true;
            renamingPreset = i;
            nameField.Value = Presets[i].Name;
            ShowNameRow();
        }

        void ShowNameRow()
        {
            nameField.Visible = true;
            presetOkBtn.Visible = true;
            presetCancelBtn.Visible = true;
            LayoutAll();
            Invalidate(true);
            nameField.Box.Focus();
            nameField.Box.SelectAll();
        }

        void CommitPreset()
        {
            if (!savingPreset) return;
            string name = (nameField.Value ?? "").Trim();

            if (renamingPreset >= 0 && renamingPreset < Presets.Count)
            {
                if (name.Length == 0) name = Presets[renamingPreset].Name;
                // renaming only relabels the palette; its colours are left untouched
                Presets[renamingPreset].Name = PresetOps.UniqueName(Presets, name, renamingPreset);
                EndSavePreset();
                return;
            }

            name = PresetOps.CleanName(name, SuggestedPresetName());
            ThemePreset existing = Presets.Find(delegate(ThemePreset p)
            {
                return string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase);
            });
            if (existing != null)
            {
                existing.Surface = AppSettings.ToHex(Surface);
                existing.Accent = AppSettings.ToHex(Accent);
                existing.Text = AppSettings.ToHex(Ink);
            }
            else if (Presets.Count < MaxPresets)
            {
                Presets.Add(new ThemePreset(name, Surface, Accent, Ink));
            }
            EndSavePreset();
        }

        void CancelSavePreset()
        {
            if (!savingPreset) return;
            EndSavePreset();
        }

        void EndSavePreset()
        {
            savingPreset = false;
            renamingPreset = -1;
            nameField.Visible = false;
            presetOkBtn.Visible = false;
            presetCancelBtn.Visible = false;
            LayoutAll();
            Invalidate(true);
        }

        void CommitHex()
        {
            if (hex == null) return;
            string t = hex.Value.Trim();
            if (t.Length == 0) { hex.Value = AppSettings.ToHex(Current); return; }
            Color c = AppSettings.ParseHex(t, Current);
            if (c == Current) return;
            SetCurrent(c);
        }

        // ------------------------------------------------------------- mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Y < T.P(44) && e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
                return;
            }
            if (e.Button != MouseButtons.Left) return;

            int chip = PresetAt(e.Location);
            if (chip >= 0)
            {
                if (PresetRemoveRect(chip).Contains(e.Location))
                {
                    RemovePreset(chip);
                    return;
                }
                // recall happens on release; moving the pointer first turns this into a reorder
                pressPreset = chip;
                pressPoint = e.Location;
                dragPoint = e.Location;
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                if (TargetRect(i).Contains(e.Location))
                {
                    target = (Target)i;
                    SyncHsv();
                    hex.Value = AppSettings.ToHex(Current);
                    Invalidate();
                    return;
                }
            }

            if (SvRect.Contains(e.Location))
            {
                dragSv = true;
                Capture = true;
                PickSv(e.Location);
                return;
            }
            if (HueRect.Contains(e.Location))
            {
                dragHue = true;
                Capture = true;
                PickHue(e.Location);
                return;
            }
            for (int i = 0; i < Swatches.Length; i++)
            {
                if (SwatchRect(i).Contains(e.Location))
                {
                    SetCurrent(Color.FromArgb(255, (Swatches[i] >> 16) & 0xFF, (Swatches[i] >> 8) & 0xFF, Swatches[i] & 0xFF));
                    return;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragSv) { PickSv(e.Location); return; }
            if (dragHue) { PickHue(e.Location); return; }

            if (pressPreset >= 0)
            {
                if (dragPreset < 0)
                {
                    int dx = e.X - pressPoint.X, dy = e.Y - pressPoint.Y;
                    if (Math.Abs(dx) >= Math.Max(4, T.P(4)) || Math.Abs(dy) >= Math.Max(4, T.P(4)))
                    {
                        dragPreset = pressPreset;
                        dragTarget = pressPreset;
                        dragPoint = e.Location;
                        hoverPreset = -1;
                        Cursor = Cursors.SizeWE;
                        Capture = true;
                        Invalidate();
                        return;
                    }
                }
                else
                {
                    dragPoint = e.Location;
                    dragTarget = PresetDropIndex(e.X);
                    Cursor = Cursors.SizeWE;
                    Invalidate();
                    return;
                }
            }

            int over = PresetAt(e.Location);
            if (over != hoverPreset)
            {
                hoverPreset = over;
                Invalidate();
            }
            Cursor = over >= 0 ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (dragPreset >= 0 || pressPreset >= 0) return;
            if (hoverPreset != -1) { hoverPreset = -1; Invalidate(); }
            Cursor = Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragSv = false;
            dragHue = false;

            if (dragPreset >= 0)
            {
                int from = dragPreset, to = dragTarget;
                dragPreset = -1;
                dragTarget = -1;
                pressPreset = -1;
                Capture = false;
                Cursor = Cursors.Default;
                if (to >= 0 && to != from) PresetOps.Move(Presets, from, to);
                hoverPreset = -1;
                LayoutAll();
                Invalidate(true);
                return;
            }

            if (pressPreset >= 0)
            {
                int chip = pressPreset;
                pressPreset = -1;
                if (chip < Presets.Count && PresetRect(chip).Contains(e.Location) &&
                    !PresetRemoveRect(chip).Contains(e.Location))
                {
                    ApplyPreset(chip);
                }
                hoverPreset = -1;
                Invalidate();
            }

            Capture = false;
        }

        void PickSv(Point p)
        {
            RectangleF r = SvRect;
            sat = Math.Max(0f, Math.Min(1f, (p.X - r.X) / r.Width));
            val = 1f - Math.Max(0f, Math.Min(1f, (p.Y - r.Y) / r.Height));
            SetCurrent(T.HsvToRgb(hue, sat, val));
        }

        void PickHue(Point p)
        {
            RectangleF r = HueRect;
            hue = Math.Max(0f, Math.Min(1f, (p.Y - r.Y) / r.Height)) * 360f;
            SetCurrent(T.HsvToRgb(hue, sat, val));
        }

        // ------------------------------------------------------------- paint
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(T.Card)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int pad = Pad;
            int tile = T.P(30);
            T.LogoTile(g, new RectangleF(pad, T.P(15), tile, tile), T.F(7f));
            T.Str(g, L.T("Custom theme", "Eigenes Design"), T.FPageTitle, T.Text,
                new Rectangle(pad + tile + T.P(12), T.P(11), T.P(300), T.P(28)), T.FL);
            T.Str(g, L.T("Pick a surface, an accent and a text colour — everything else is derived.",
                    "Fläche, Akzent und Textfarbe wählen – alles andere wird abgeleitet."),
                T.FSmall, T.Muted, new Rectangle(pad + tile + T.P(12), T.P(37), T.P(430), T.P(18)), T.FL);

            // BETA pill in the header
            string beta = "BETA";
            int bw = T.Meas(beta, T.FLabel).Width + T.P(14);
            RectangleF tag = new RectangleF(ClientSize.Width - T.P(56) - bw, T.P(18), bw, T.P(20));
            T.StrokeRound(g, tag, T.F(10f), T.Gold, 1f);
            T.Str(g, beta, T.FLabel, T.Gold, Rectangle.Round(tag), T.FC);
            T.HLine(g, 0, ClientSize.Width, T.P(70), T.LineSoft);

            DrawTargets(g);
            DrawPicker(g);
            DrawPresets(g);
            DrawPresetChips(g);

            T.Str(g, L.T("Preview updates behind this sheet", "Die Vorschau dahinter aktualisiert sich live"),
                T.FMicro, T.Dim, new Rectangle(pad, ClientSize.Height - T.P(82), T.P(330), T.P(16)), T.FL);

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Rim, 1f))
                g.DrawPath(pen, rim);
        }

        void DrawTargets(Graphics g)
        {
            string[] names = { L.T("Surface", "Fläche"), L.T("Accent", "Akzent"), L.T("Text", "Text") };
            Color[] values = { Surface, Accent, Ink };
            for (int i = 0; i < 3; i++)
            {
                Rectangle r = TargetRect(i);
                bool on = (int)target == i;
                RectangleF rr = new RectangleF(r.X, r.Y, r.Width, r.Height);
                T.FillRound(g, rr, T.F(10f), on ? T.HoverSoft : T.Inset);
                T.StrokeRound(g, new RectangleF(rr.X + 0.5f, rr.Y + 0.5f, rr.Width - 1f, rr.Height - 1f),
                    T.F(10f), on ? T.Gold : T.Stroke, on ? 1.6f : 1f);

                RectangleF dot = new RectangleF(r.X + T.P(12), r.Y + T.P(12), T.P(24), T.P(24));
                T.FillRound(g, dot, T.F(6f), values[i]);
                T.StrokeRound(g, new RectangleF(dot.X + 0.5f, dot.Y + 0.5f, dot.Width - 1f, dot.Height - 1f),
                    T.F(6f), T.SwatchRing, 1f);

                T.Str(g, names[i], T.FBtn, on ? T.Text : T.Muted,
                    new Rectangle(r.X + T.P(46), r.Y + T.P(10), r.Width - T.P(56), T.P(16)), T.FL);
                T.Str(g, AppSettings.ToHex(values[i]), T.FMicro, T.Dim,
                    new Rectangle(r.X + T.P(46), r.Y + T.P(28), r.Width - T.P(56), T.P(14)), T.FL);
                if (on)
                {
                    float cs = T.F(12f);
                    T.Icon(g, "check", new RectangleF(r.Right - T.P(12) - cs, r.Y + T.P(12), cs, cs), T.Gold);
                }
            }
        }

        void DrawPicker(Graphics g)
        {
            // The square and the rail come from the app-wide painters, shared with the colour
            // sheet: one definition, so the two pickers can never drift apart.
            T.PaintSvSquare(g, SvRect, hue, sat, val, Current);
            T.PaintHueRail(g, HueRect, hue);

            T.Str(g, L.T("HEX", "HEX"), T.FLabel, T.Dim,
                new Rectangle(Pad + T.P(382), T.P(182), T.P(134), T.P(14)), T.FL);
        }

        void DrawPresets(Graphics g)
        {
            T.Str(g, L.T("PALETTE", "PALETTE"), T.FLabel, T.Dim,
                new Rectangle(Pad, T.P(384), T.P(240), T.P(14)), T.FL);
            for (int i = 0; i < Swatches.Length; i++)
            {
                Rectangle r = SwatchRect(i);
                Color c = Color.FromArgb(255, (Swatches[i] >> 16) & 0xFF, (Swatches[i] >> 8) & 0xFF, Swatches[i] & 0xFF);
                T.FillRound(g, new RectangleF(r.X, r.Y, r.Width, r.Height), T.F(6f), c);
                bool on = c.ToArgb() == Current.ToArgb();
                T.StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f),
                    T.F(6f), on ? T.Gold : T.SwatchRing, on ? 2f : 1f);
                if (on)
                {
                    float cs = T.F(10f);
                    T.Icon(g, "check", new RectangleF(r.X + (r.Width - cs) / 2f, r.Y + (r.Height - cs) / 2f, cs, cs),
                        T.Lum(c) > 0.55f ? Color.FromArgb(32, 30, 24) : Color.White);
                }
            }
        }

        /// <summary>Heading, count, interaction hint and the saved palettes.</summary>
        void DrawPresetChips(Graphics g)
        {
            string heading = L.T("PRESETS", "VORLAGEN");
            T.Str(g, heading, T.FLabel, T.Dim,
                new Rectangle(Pad, T.P(470), T.P(240), T.P(16)), T.FL);

            if (!savingPreset)
            {
                T.Str(g, Presets.Count + "/" + MaxPresets, T.FMicro, T.Dim,
                    new Rectangle(Pad + T.Meas(heading, T.FLabel).Width + T.P(6), T.P(470), T.P(40), T.P(16)), T.FL);

                int right = ClientSize.Width - Pad;
                if (Presets.Count < MaxPresets) right -= T.P(132) + T.P(8);
                string hint = dragPreset >= 0
                    ? L.T("Drop to reorder", "Zum Sortieren loslassen")
                    : L.T("Drag to reorder · double-click to rename",
                          "Ziehen · Doppelklick zum Umbenennen");
                if (Presets.Count > 0)
                    T.Str(g, hint, T.FMicro, dragPreset >= 0 ? T.Muted : T.Dim,
                        new Rectangle(Pad, T.P(470), Math.Max(0, right - Pad), T.P(16)), T.FR);

                if (Presets.Count == 0)
                {
                    T.Str(g, L.T("No presets yet — save your colours to reuse them later.",
                                "Noch keine Vorlagen – Farben speichern und später wiederverwenden."),
                        T.FSmall, T.Dim,
                        new Rectangle(Pad, T.P(504), ClientSize.Width - Pad * 2, T.P(18)), T.FL);
                    return;
                }

                for (int i = 0; i < Presets.Count; i++)
                {
                    if (i == dragPreset) DrawPresetSlot(g, i);
                    else DrawPresetChip(g, i);
                }
                if (dragPreset >= 0) DrawDragFeedback(g);
                return;
            }

            T.Str(g, renamingPreset >= 0
                    ? L.T("Enter renames · Esc cancels", "Enter benennt um · Esc bricht ab")
                    : L.T("Enter saves · Esc cancels", "Enter speichert · Esc bricht ab"),
                T.FMicro, T.Dim,
                new Rectangle(Pad, T.P(470), ClientSize.Width - Pad * 2, T.P(16)), T.FR);
        }

        /// <summary>Empty dashed slot left behind by the chip being dragged.</summary>
        void DrawPresetSlot(Graphics g, int i)
        {
            Rectangle r = PresetRect(i);
            using (GraphicsPath path = T.Round(new RectangleF(r.X, r.Y, r.Width, r.Height), T.F(8f)))
            using (Pen pen = new Pen(T.DashLine, 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawPath(pen, path);
            }
        }

        /// <summary>Gold outline on the drop slot plus the chip following the cursor.</summary>
        void DrawDragFeedback(Graphics g)
        {
            if (dragPreset < 0 || dragPreset >= Presets.Count) return;
            if (dragTarget >= 0 && dragTarget < Presets.Count)
            {
                Rectangle target = PresetRect(dragTarget);
                T.FillRound(g, new RectangleF(target.X, target.Y, target.Width, target.Height), T.F(8f), T.HoverSoft);
                T.StrokeRound(g, new RectangleF(target.X + 0.5f, target.Y + 0.5f,
                    target.Width - 1f, target.Height - 1f), T.F(8f), T.Gold, 1.8f);
            }

            Rectangle drag = PresetRect(dragPreset);
            drag.Offset(dragPoint.X - pressPoint.X, dragPoint.Y - pressPoint.Y);
            T.Shadow(g, new RectangleF(drag.X, drag.Y, drag.Width, drag.Height),
                T.F(8f), Math.Max(3, T.P(6)), 90);
            DrawPresetChipAt(g, Presets[dragPreset], drag, true,
                PresetIsCurrent(Presets[dragPreset]), true);
        }

        void DrawPresetChip(Graphics g, int i)
        {
            DrawPresetChipAt(g, Presets[i], PresetRect(i), hoverPreset == i && dragPreset < 0,
                PresetIsCurrent(Presets[i]), false);
        }

        void DrawPresetChipAt(Graphics g, ThemePreset p, Rectangle r, bool hot, bool on, bool floating)
        {
            RectangleF rr = new RectangleF(r.X, r.Y, r.Width, r.Height);
            Color fill = floating ? T.Hover : on ? T.HoverSoft : hot ? T.Hover : T.Inset;

            T.FillRound(g, rr, T.F(8f), fill);
            T.StrokeRound(g, new RectangleF(rr.X + 0.5f, rr.Y + 0.5f, rr.Width - 1f, rr.Height - 1f),
                T.F(8f), floating ? T.GoldHi : on ? T.Gold : hot ? T.StrokeHot : T.Stroke,
                on || floating ? 1.6f : 1f);

            float d = T.F(10f);
            RectangleF dot = new RectangleF(r.X + T.P(9), r.Y + (r.Height - d) / 2f, d, d);
            using (SolidBrush b = new SolidBrush(p.SurfaceColor)) g.FillEllipse(b, dot);
            using (Pen ring = new Pen(T.SwatchRing, 1f)) g.DrawEllipse(ring, dot.X, dot.Y, dot.Width, dot.Height);

            int textX = (int)Math.Round(dot.Right) + T.P(6);
            int removeX = r.Right - T.P(22);
            T.StrEll(g, p.Name, T.FSmall, on || hot ? T.Text : T.Muted,
                new Rectangle(textX, r.Y, Math.Max(T.P(12), removeX - textX - T.P(3)), r.Height), T.FL);

            float cs = T.F(8f);
            RectangleF x = new RectangleF(r.Right - T.P(16) - cs, r.Y + (r.Height - cs) / 2f, cs, cs);
            T.Icon(g, "close", x, hot ? T.Text : T.Dim);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
