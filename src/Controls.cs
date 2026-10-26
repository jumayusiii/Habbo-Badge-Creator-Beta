using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>Rounded button with optional leading icon.</summary>
    internal class FlatBtn : Control
    {
        public string Label = "";
        public string Icon = null;
        public bool Primary = false;
        bool hot, down;

        public FlatBtn()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            float rad = T.F(8f);
            Color fg = T.ButtonFace(g, r, rad, Primary, hot, down);

            float iw = Icon != null ? T.F(15f) : 0f;
            float gap = Icon != null ? T.F(7f) : 0f;
            Size ts = Label.Length > 0 ? T.Meas(Label, T.FBtn) : new Size(0, 0);
            float total = iw + gap + ts.Width;
            float x = (Width - total) / 2f;
            if (Icon != null)
                T.Icon(g, Icon, new RectangleF(x, (Height - iw) / 2f - 0.5f, iw, iw), fg);
            if (Label.Length > 0)
            {
                Rectangle tr = new Rectangle((int)Math.Round(x + iw + gap), 0, (int)ts.Width + 3, Height);
                T.Str(g, Label, T.FBtn, fg, tr, T.FL);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
    }

    /// <summary>Square icon button, optionally a toggle (used for tools).</summary>
    internal class IconBtn : Control
    {
        public string Icon = "pencil";
        public bool Active = false;
        public bool Toggle = false;
        public bool Segmented = false;
        public float IconScale = 0.56f;
        public string TipTitle = "";
        public string TipKey = "";
        bool hot, down;

        public IconBtn()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            float rad = T.F(8f);
            Color fg;
            if (Active) fg = T.ButtonFace(g, r, rad, true, hot, down);
            else if (Segmented)
            {
                if (hot) T.FillRound(g, r, rad, T.HoverSoft);
                fg = hot ? T.Text : T.IconIdle;
            }
            else fg = T.ButtonFace(g, r, rad, false, hot, down);
            float s = Math.Min(Width, Height) * IconScale;
            T.Icon(g, Icon, new RectangleF((Width - s) / 2f, (Height - s) / 2f, s, s), fg);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
    }

    /// <summary>Small material tooltip that floats above the custom controls.</summary>
    internal class TipView : Control
    {
        public string Title = "";
        public string Key = "";

        public TipView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            TabStop = false;
            Visible = false;
        }

        public void ShowFor(Control anchor)
        {
            if (!(anchor is IconBtn)) return;
            IconBtn b = (IconBtn)anchor;
            if (b.TipTitle.Length == 0) { Visible = false; return; }
            ShowFor(anchor, b.TipTitle, b.TipKey);
        }

        /// <summary>Shows the tooltip under any control with an explicit label and detail.</summary>
        public void ShowFor(Control anchor, string title, string key)
        {
            ShowAt(new Rectangle(anchor.Left, anchor.Top, anchor.Width, anchor.Height), title, key);
        }

        /// <summary>Shows the tooltip under a rect given in the parent's coordinates.</summary>
        public void ShowAt(Rectangle anchor, string title, string key)
        {
            Title = title ?? "";
            Key = key ?? "";
            if (Title.Length == 0 || Parent == null) { Visible = false; return; }

            int pad = T.P(9), gap = T.P(7), dot = T.P(3), h = T.P(26);
            int tw = T.Meas(Title, T.FSmall).Width;
            int kw = Key.Length > 0 ? T.Meas(Key, T.FSmall).Width : 0;
            int w = pad * 2 + tw + (kw > 0 ? gap * 2 + dot + gap + kw : 0);
            int x = anchor.Left + (anchor.Width - w) / 2;
            x = Math.Max(T.P(16), Math.Min(Parent.ClientSize.Width - T.P(16) - w, x));
            int y = anchor.Bottom + T.P(6);
            if (y + h > Parent.ClientSize.Height - T.P(6)) y = anchor.Top - T.P(6) - h;
            Bounds = new Rectangle(x, y, w, h);
            BringToFront();
            Visible = true;
            Invalidate();
        }

        public void Dismiss()
        {
            Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(T.Card)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            T.FillRound(g, r, T.F(6f), T.TipBg);
            T.StrokeRound(g, r, T.F(6f), T.Stroke, 1f);

            int pad = T.P(9), gap = T.P(7), dot = T.P(3);
            int x = pad;
            T.Str(g, Title, T.FSmall, T.Text, new Rectangle(x, 0, Width - x - pad, Height), T.FL);
            x += T.Meas(Title, T.FSmall).Width + gap;
            if (Key.Length > 0)
            {
                using (SolidBrush b = new SolidBrush(T.Dim))
                    g.FillEllipse(b, x, (Height - dot) / 2f, dot, dot);
                x += dot + gap;
                T.Str(g, Key, T.FSmall, T.Gold, new Rectangle(x, 0, Width - x - pad, Height), T.FL);
            }
        }
    }

    /// <summary>Colour swatch for the palette.</summary>
    internal class SwatchBtn : Control
    {
        public Color Value = Color.Black;
        public bool Selected = false;
        public bool Round = false;   // the current-colour well uses a circular face
        bool hot;

        public SwatchBtn()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Round)
            {
                float inset = T.F(3.5f);
                RectangleF c = new RectangleF(inset, inset, Width - inset * 2, Height - inset * 2);
                using (SolidBrush b = new SolidBrush(Value)) g.FillEllipse(b, c);
                using (Pen pen = new Pen(hot ? T.SwatchRing : T.Mix(T.SwatchRing, T.Card, 0.45f), T.F(1.6f)))
                    g.DrawEllipse(pen, c);
                using (Pen pen = new Pen(T.Stroke, 1f))
                    g.DrawEllipse(pen, 0.5f, 0.5f, Width - 1f, Height - 1f);
                return;
            }
            float pad = T.F(1f);
            RectangleF r = new RectangleF(pad, pad, Width - pad * 2, Height - pad * 2);
            T.FillRound(g, r, T.F(5f), Value);
            if (Selected)
            {
                T.StrokeRound(g, new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(7f), T.Text, T.F(2f));
            }
            else if (hot)
                T.StrokeRound(g, r, T.F(5f), T.SwatchRing, T.F(1.4f));
            else
                T.StrokeRound(g, r, T.F(5f), Color.FromArgb(38, 0, 0, 0), 1f);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
    }

    /// <summary>Dashed "+" tile that opens the custom colour picker.</summary>
    internal class PlusSwatch : Control
    {
        bool hot;

        public PlusSwatch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            T.FillRound(g, r, T.F(5f), hot ? T.HoverSoft : T.Inset);
            using (GraphicsPath p = T.Round(r, T.F(5f)))
            using (Pen pen = new Pen(hot ? T.IconIdle : T.DashLine, 1.2f))
            {
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 2f, 1.6f };
                g.DrawPath(pen, p);
            }
            float s = Math.Min(Width, Height) * 0.52f;
            T.Icon(g, "plus", new RectangleF((Width - s) / 2f, (Height - s) / 2f, s, s), hot ? T.Text : T.Muted);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
    }

    /// <summary>Window caption button (minimise / maximise / close).</summary>
    internal class WinBtn : Control
    {
        public string Icon = "min";
        bool hot;

        public WinBtn()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.TitleBg;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush b = new SolidBrush(BackColor)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool close = Icon == "close";
            if (hot)
                T.FillRound(g, new RectangleF(T.P(3), T.P(2), Width - T.P(6), Height - T.P(4)),
                    T.F(7f), close ? Color.FromArgb(216, 62, 68) : T.Hover);
            Color fg = hot ? Color.White : T.WinFg;
            float s = T.F(14f);
            T.Icon(g, Icon, new RectangleF((Width - s) / 2f, (Height - s) / 2f, s, s), fg);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
    }

    /// <summary>EN | DE two-way language switch.</summary>
    internal class LangSwitch : Control
    {
        public bool German;
        bool hot;
        public event Action Changed;

        public LangSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0f, 0f, Width, Height);
            T.FillRound(g, r, T.F(7f), hot ? T.HoverSoft : T.Segment);
            int half = Width / 2;
            DrawHalf(g, "EN", new Rectangle(0, 0, half, Height), !German);
            DrawHalf(g, "DE", new Rectangle(half, 0, Width - half, Height), German);
        }

        void DrawHalf(Graphics g, string label, Rectangle rc, bool active)
        {
            if (active)
            {
                RectangleF pill = new RectangleF(rc.X + T.P(2), rc.Y + T.P(2), rc.Width - T.P(4), rc.Height - T.P(4));
                T.FillRound(g, pill, T.F(5f), T.LangPill);
                T.Str(g, label, T.FChip, T.LangPillText, rc, T.FC);
            }
            else
            {
                T.Str(g, label, T.FChip, hot ? T.Muted : T.Dim, rc, T.FC);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            German = e.X >= Width / 2;
            Invalidate();
            if (Changed != null) Changed();
        }
    }

    /// <summary>Header chip with a small leading icon (changelog, settings).</summary>
    internal class LinkChip : Control
    {
        public string Label = "Changelog";
        public string Icon = "sparkle";
        public int Status;   // 0 hidden, 1 waiting / connecting, 2 connected, 3 error
        bool hot;

        public LinkChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.FillRound(g, new RectangleF(0f, 0f, Width, Height), T.F(7f), hot ? T.HoverSoft : T.Segment);

            float ic = T.F(11f);
            float ix = T.P(10);
            T.Icon(g, Icon, new RectangleF(ix, (Height - ic) / 2f, ic, ic), hot ? T.GoldHi : T.Gold);
            int tx = (int)Math.Round(ix + ic) + T.P(6);
            int dotW = Status > 0 ? T.P(15) : 0;
            T.Str(g, Label, T.FChip, hot ? T.Text : T.ChipText,
                new Rectangle(tx, 0, Width - tx - T.P(7) - dotW, Height), T.FL);

            if (Status > 0)
            {
                Color dc = Status == 2 ? T.Teal : Status == 3 ? T.Danger : T.Gold;
                float d = T.F(6f);
                using (SolidBrush b = new SolidBrush(dc))
                    g.FillEllipse(b, Width - T.P(11) - d, (Height - d) / 2f, d, d);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
    }

    /// <summary>Appearance picker tile: a miniature of the window drawn in that palette.</summary>
    internal class ThemeTile : Control
    {
        public ThemeMode Mode = ThemeMode.Dark;
        public bool Selected;
        bool hot;

        public ThemeTile()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF r = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            T.FillRound(g, r, T.F(10f), T.MiniBg(Mode));

            // miniature window: app bar, editor card, details and badge list
            int pad = T.P(9);
            RectangleF win = new RectangleF(pad, pad, Width - pad * 2, Height - pad * 2 - T.P(26));
            T.FillRound(g, win, T.F(6f), T.MiniBg(Mode));
            T.StrokeRound(g, win, T.F(6f), T.MiniStroke(Mode), 1f);

            T.FillRound(g, new RectangleF(win.X + T.P(6), win.Y + T.P(6), T.P(20), T.P(5)), T.F(2f), T.MiniGold(Mode));
            using (SolidBrush d = new SolidBrush(T.MiniStroke(Mode)))
                for (int i = 0; i < 3; i++)
                    g.FillEllipse(d, win.Right - T.P(26) + i * T.P(7), win.Y + T.P(7), T.P(3), T.P(3));

            float bodyY = win.Y + T.P(16);
            float bodyH = win.Bottom - bodyY - T.P(6);
            float lw = (win.Width - T.P(14)) * 0.57f;
            RectangleF editor = new RectangleF(win.X + T.P(5), bodyY, lw, bodyH);
            RectangleF details = new RectangleF(editor.Right + T.P(5), bodyY, win.Right - editor.Right - T.P(10), bodyH * 0.56f);
            RectangleF list = new RectangleF(details.X, details.Bottom + T.P(4), details.Width, bodyH - details.Height - T.P(4));
            RectangleF[] cards = new RectangleF[] { editor, details, list };
            foreach (RectangleF c in cards)
            {
                using (SolidBrush cb = new SolidBrush(T.MiniCard(Mode)))
                using (GraphicsPath p = T.Round(c, T.F(3f)))
                    g.FillPath(cb, p);
            }

            using (SolidBrush b = new SolidBrush(T.MiniGold(Mode)))
            {
                g.FillEllipse(b, editor.X + T.P(5), editor.Y + T.P(5), T.P(4), T.P(4));
                g.FillRectangle(b, editor.X + T.P(5), editor.Y + T.P(13), T.P(12), T.P(3));
            }
            using (SolidBrush b = new SolidBrush(T.MiniStroke(Mode)))
            {
                g.FillRectangle(b, details.X + T.P(5), details.Y + T.P(5), details.Width - T.P(10), T.P(3));
                g.FillRectangle(b, details.X + T.P(5), details.Bottom - T.P(8), details.Width - T.P(10), T.P(3));
                g.FillRectangle(b, list.X + T.P(5), list.Y + T.P(4), list.Width - T.P(10), T.P(3));
            }

            int ly = Height - T.P(24);
            Color lc = Selected ? T.Gold : (hot ? T.Text : T.Muted);
            string label = Mode == ThemeMode.Light ? L.T("Light", "Hell")
                : Mode == ThemeMode.Navy ? L.T("Navy", "Marineblau")
                : Mode == ThemeMode.Custom ? L.T("Custom", "Eigene")
                : L.T("Dark", "Dunkel");
            int lx;
            if (Mode == ThemeMode.Custom)
            {
                // the designer is experimental, so the tile carries its own BETA tag
                int pw = T.Meas("BETA", T.FMicro).Width + T.P(10);
                Rectangle pill = new Rectangle(T.P(10), ly + T.P(2), pw, T.P(14));
                T.FillRound(g, pill, T.F(7f), Color.FromArgb(34, T.Gold));
                T.StrokeRound(g, pill, T.F(7f), T.Gold, 1f);
                T.Str(g, "BETA", T.FMicro, T.Gold, pill, T.FC);
                lx = pill.Right + T.P(7);
            }
            else
            {
                float ic = T.F(12f);
                string icon = Mode == ThemeMode.Light ? "sun" : "moon";
                T.Icon(g, icon, new RectangleF(T.P(11), ly + (T.P(16) - ic) / 2f, ic, ic), lc);
                lx = T.P(11) + (int)ic + T.P(7);
            }
            T.StrEll(g, label, T.FBtn, lc,
                new Rectangle(lx, ly, Math.Max(T.P(34), Width - lx - T.P(26)), T.P(16)), T.FL);
            if (Selected)
            {
                float cs = T.F(12f);
                T.Icon(g, "check", new RectangleF(Width - T.P(11) - cs, ly + (T.P(16) - cs) / 2f, cs, cs), T.Gold);
            }

            T.StrokeRound(g, new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(10f),
                Selected ? T.Gold : (hot ? T.StrokeHot : T.Stroke), Selected ? 2f : 1f);
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
    }

    /// <summary>Rounded text input with placeholder support.</summary>
    internal class FieldBox : Control
    {
        public readonly TextBox Box;
        public string Placeholder = "";
        bool hot, focused, showingPh;

        public FieldBox(bool multiline)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            Box = new TextBox();
            Box.BorderStyle = BorderStyle.None;
            Box.BackColor = T.Inset;
            Box.ForeColor = T.Text;
            Box.Font = T.FInput;
            Box.Multiline = multiline;
            Box.TabStop = true;
            if (multiline) Box.ScrollBars = ScrollBars.None;
            Box.GotFocus += delegate { focused = true; if (showingPh) { showingPh = false; Box.Text = ""; Box.ForeColor = T.Text; } Invalidate(); };
            Box.LostFocus += delegate { focused = false; if (Box.Text.Length == 0) ShowPlaceholder(); Invalidate(); };
            Box.TextChanged += delegate { if (ValueChanged != null) ValueChanged(); };
            Controls.Add(Box);
        }

        public event Action ValueChanged;

        void ShowPlaceholder()
        {
            if (string.IsNullOrEmpty(Placeholder)) return;
            showingPh = true;
            Box.Text = Placeholder;
            Box.ForeColor = T.Dim;
        }

        public void SetPlaceholder(string text)
        {
            Placeholder = text;
            if (Box.Text.Length == 0) ShowPlaceholder();
        }

        /// <summary>Repaints the embedded text box in the active palette.</summary>
        public void ApplyTheme()
        {
            BackColor = T.Card;
            Box.BackColor = T.Inset;
            Box.ForeColor = showingPh ? T.Dim : T.Text;
            Invalidate();
        }

        /// <summary>Re-applies the placeholder text after a language switch.</summary>
        public void ResetPlaceholder(string text)
        {
            Placeholder = text;
            if (showingPh)
            {
                showingPh = false;
                Box.Text = "";
                ShowPlaceholder();
            }
        }

        public string Value
        {
            get { return showingPh ? "" : Box.Text; }
            set
            {
                if (string.IsNullOrEmpty(value)) { showingPh = false; Box.Text = ""; ShowPlaceholder(); }
                else { showingPh = false; Box.Text = value; Box.ForeColor = T.Text; }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutBox();
        }

        void LayoutBox()
        {
            int pad = T.P(13);
            if (Box.Multiline)
            {
                Box.SetBounds(pad, T.P(9), Math.Max(10, Width - pad * 2), Math.Max(10, Height - T.P(16)));
            }
            else
            {
                Box.Width = Math.Max(10, Width - pad * 2);
                Box.Left = pad;
                Box.Top = (Height - Box.Height) / 2;
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { Box.Focus(); base.OnMouseDown(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            Color border = focused ? T.Focus : (hot ? T.StrokeHot : T.Stroke);
            T.FillRound(g, r, T.F(8f), T.Inset);
            T.StrokeRound(g, r, T.F(8f), border, focused ? 1.4f : 1f);
        }
    }

    /// <summary>Small BETA pill in the app bar; hovering it explains the build.</summary>
    internal class BetaTag : Control
    {
        bool hot;

        public BetaTag()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Bg1;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e) { hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hot = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);

            string tag = "BETA";
            int tw = Math.Min(Width, T.Meas(tag, T.FLabel).Width + T.P(12));
            int th = T.P(18);
            int x = (Width - tw) / 2, y = (Height - th) / 2;
            RectangleF r = new RectangleF(x + 0.5f, y + 0.5f, tw - 1f, th - 1f);
            if (hot) T.FillRound(g, r, T.F(9f), T.Inset);
            T.StrokeRound(g, r, T.F(9f), T.Gold, 1f);
            T.Str(g, tag, T.FLabel, T.Gold, new Rectangle(x, y, tw, th), T.FC);
        }
    }
    /// <summary>
    /// Layer stack (beta). Per-layer visibility, lock, inline rename, opacity and blend mode,
    /// plus the stack actions. Painting is custom so it matches the rest of the sheet.
    /// </summary>
    internal class LayersPanel : Control
    {
        public PixelDoc Doc;
        public Action Changed;

        /// <summary>Raised with a button rect (panel coordinates) so the app can show its tooltip.</summary>
        public event Action<Rectangle, string, string> HintShown;
        public event Action HintHidden;

        public const int SlotAdd = 0, SlotCopy = 1, SlotMerge = 2, SlotUp = 3, SlotDown = 4, SlotDel = 5;

        int hoverRow = -1, hoverEye = -1, hoverLock = -1, hoverBtn = -1;
        bool hoverOpacity, hoverBlend;
        bool draggingOpacity, dragPushed;
        TextBox renameBox;
        int renameRow = -1;

        public LayersPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            TabStop = false;
        }

        int Pad { get { return T.P(8); } }
        int HeadH { get { return T.P(28); } }
        int Btn { get { return T.P(24); } }
        int BtnGap { get { return T.P(4); } }
        int FooterH { get { return Btn * 2 + BtnGap; } }
        int PropsH { get { return T.P(80); } }

        // properties sit under the header like Photoshop's blend/opacity strip, the list
        // grows downward from there and the stack actions stay pinned to the bottom
        int RowsTop { get { return HeadH + PropsH; } }
        int PropsTop { get { return HeadH; } }
        int FooterY { get { return Height - T.P(8) - FooterH; } }

        // rows shrink on short windows so the properties and buttons always stay visible
        int RowH
        {
            get
            {
                int n = Doc == null ? 1 : Math.Max(1, Doc.LayerCount);
                int avail = Math.Max(T.P(20), FooterY - T.P(6) - RowsTop);
                return Math.Max(T.P(20), Math.Min(T.P(36), avail / n));
            }
        }

        int LayerAtRow(int row)
        {
            return Doc == null ? -1 : Doc.LayerCount - 1 - row;
        }

        Rectangle RowRect(int row)
        {
            return new Rectangle(0, RowsTop + row * RowH, Width, RowH);
        }

        Rectangle EyeRect(int row)
        {
            Rectangle r = RowRect(row);
            int s = T.P(16);
            return new Rectangle(Pad, r.Y + (r.Height - s) / 2, s, s);
        }

        Rectangle LockRect(int row)
        {
            Rectangle r = RowRect(row);
            int s = T.P(14);
            return new Rectangle(Width - Pad - s, r.Y + (r.Height - s) / 2, s, s);
        }

        Rectangle NameRect(int row)
        {
            Rectangle r = RowRect(row);
            int l = EyeRect(row).Right + T.P(7);
            int right = LockRect(row).Left - T.P(4);
            int pct = Doc != null && Doc.LayerOpacity(LayerAtRow(row)) < 255 ? T.P(30) : T.P(2);
            return new Rectangle(l, r.Y, Math.Max(T.P(24), right - l - pct), r.Height);
        }

        int TrackY { get { return PropsTop + T.P(22); } }

        Rectangle TrackRect { get { return new Rectangle(Pad, TrackY, Math.Max(T.P(40), Width - Pad * 2), T.P(12)); } }

        Rectangle BlendRect { get { return new Rectangle(Pad, PropsTop + T.P(42), Width - Pad * 2, T.P(24)); } }

        /// <summary>Two rows of three action buttons: add, duplicate, merge / up, down, delete.</summary>
        Rectangle ButtonRect(int slot)
        {
            int row = slot / 3, col = slot % 3;
            return new Rectangle(Pad + col * (Btn + BtnGap), FooterY + row * (Btn + BtnGap), Btn, Btn);
        }

        /// <summary>Panel-space rect of a footer button, used by the app to place its tooltip.</summary>
        public Rectangle SlotRect(int slot)
        {
            return ButtonRect(slot);
        }

        int ButtonAt(Point p)
        {
            for (int slot = 0; slot <= SlotDel; slot++)
                if (ButtonRect(slot).Contains(p)) return slot;
            return -1;
        }

        bool IsEnabled(int slot)
        {
            if (Doc == null) return false;
            switch (slot)
            {
                case SlotAdd:
                case SlotCopy:
                    return Doc.LayerCount < PixelDoc.MaxLayers;
                case SlotMerge:
                case SlotDown:
                    return Doc.ActiveLayer > 0 && !Doc.ActiveLocked && !Doc.LayerLocked(Doc.ActiveLayer - 1);
                case SlotUp:
                    return Doc.ActiveLayer < Doc.LayerCount - 1;
                case SlotDel:
                    return Doc.LayerCount > 1;
                default:
                    return false;
            }
        }

        static string ButtonIcon(int slot)
        {
            switch (slot)
            {
                case SlotAdd: return "plus";
                case SlotCopy: return "copy";
                case SlotMerge: return "merge";
                case SlotUp: return "up";
                case SlotDown: return "down";
                default: return "trash";
            }
        }

        static void ButtonText(int slot, out string title, out string key)
        {
            key = "";
            switch (slot)
            {
                case SlotAdd:
                    title = L.T("Add layer", "Ebene hinzufügen");
                    return;
                case SlotCopy:
                    title = L.T("Duplicate layer", "Ebene duplizieren");
                    key = "Ctrl+J";
                    return;
                case SlotMerge:
                    title = L.T("Merge down", "Nach unten zusammenfassen");
                    key = "Ctrl+E";
                    return;
                case SlotUp:
                    title = L.T("Move layer up", "Ebene nach oben");
                    key = "Alt+↑";
                    return;
                case SlotDown:
                    title = L.T("Move layer down", "Ebene nach unten");
                    key = "Alt+↓";
                    return;
                default:
                    title = L.T("Delete layer", "Ebene löschen");
                    return;
            }
        }

        static string ModeName(PixelDoc.LayerBlend mode)
        {
            switch (mode)
            {
                case PixelDoc.LayerBlend.Multiply:
                    return L.T("Multiply", "Multiplizieren");
                case PixelDoc.LayerBlend.Screen:
                    return L.T("Screen", "Negativ multiplizieren");
                default:
                    return L.T("Normal", "Normal");
            }
        }

        static PixelDoc.LayerBlend NextMode(PixelDoc.LayerBlend mode)
        {
            if (mode == PixelDoc.LayerBlend.Normal) return PixelDoc.LayerBlend.Multiply;
            if (mode == PixelDoc.LayerBlend.Multiply) return PixelDoc.LayerBlend.Screen;
            return PixelDoc.LayerBlend.Normal;
        }

        int RowAt(Point p)
        {
            if (Doc == null || p.Y < RowsTop || p.Y >= RowsTop + Doc.LayerCount * RowH) return -1;
            int row = (p.Y - RowsTop) / RowH;
            return row >= 0 && row < Doc.LayerCount ? row : -1;
        }

        void Fire()
        {
            Invalidate();
            if (Changed != null) Changed();
        }

        // ------------------------------------------------------------- rename
        void BeginRename(int row)
        {
            if (Doc == null || row < 0) return;
            EndRename(false);
            renameRow = row;
            Rectangle r = NameRect(row);
            int layer = LayerAtRow(row);
            renameBox = new TextBox();
            renameBox.BorderStyle = BorderStyle.None;
            renameBox.Font = T.FSmall;
            renameBox.BackColor = T.Inset;
            renameBox.ForeColor = T.Text;
            renameBox.Text = Doc.LayerName(layer);
            renameBox.SetBounds(r.X - T.P(3), r.Y + (r.Height - T.P(18)) / 2, r.Width + T.P(6), T.P(18));
            renameBox.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; EndRename(true); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; EndRename(false); }
            };
            renameBox.LostFocus += delegate { EndRename(true); };
            Controls.Add(renameBox);
            renameBox.Focus();
            renameBox.SelectAll();
            Invalidate();
        }

        void EndRename(bool commit)
        {
            if (renameBox == null) return;
            TextBox box = renameBox;
            int row = renameRow;
            renameBox = null;
            renameRow = -1;   // a LostFocus fired by the removal now returns early
            if (commit && Doc != null && row >= 0)
                Doc.SetLayerName(LayerAtRow(row), box.Text);
            Controls.Remove(box);
            box.Dispose();
            Fire();
        }

        // ------------------------------------------------------------- mouse
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Doc == null) return;
            if (draggingOpacity)
            {
                ApplyOpacityAt(e.X);
                return;
            }
            int row = RowAt(e.Location);
            int eye = row >= 0 && EyeRect(row).Contains(e.Location) ? row : -1;
            int lockIcon = row >= 0 && LockRect(row).Contains(e.Location) ? row : -1;
            int btn = ButtonAt(e.Location);
            bool op = TrackRect.Contains(e.Location);
            bool blend = BlendRect.Contains(e.Location);
            if (row != hoverRow || eye != hoverEye || lockIcon != hoverLock || btn != hoverBtn || op != hoverOpacity || blend != hoverBlend)
            {
                hoverRow = row;
                hoverEye = eye;
                hoverLock = lockIcon;
                hoverBtn = btn;
                hoverOpacity = op;
                hoverBlend = blend;
                Cursor = op ? Cursors.Hand : Cursors.Default;
                Invalidate();
                if (btn >= 0 && IsEnabled(btn))
                {
                    string title, key;
                    ButtonText(btn, out title, out key);
                    if (HintShown != null) HintShown(ButtonRect(btn), title, key);
                }
                else if (blend)
                {
                    if (HintShown != null)
                        HintShown(BlendRect, L.T("Blend mode", "Füllmethode"), "");
                }
                else if (op)
                {
                    if (HintShown != null)
                        HintShown(TrackRect, L.T("Layer opacity", "Ebenen-Deckkraft"), "");
                }
                else if (HintHidden != null) HintHidden();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverRow = -1;
            hoverEye = -1;
            hoverLock = -1;
            hoverBtn = -1;
            hoverOpacity = false;
            hoverBlend = false;
            draggingOpacity = false;
            Cursor = Cursors.Default;
            Invalidate();
            if (HintHidden != null) HintHidden();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Doc == null || e.Button != MouseButtons.Left) return;
            if (renameBox != null) EndRename(true);

            int slot = ButtonAt(e.Location);
            if (slot >= 0)
            {
                FireButton(slot);
                return;
            }

            if (BlendRect.Contains(e.Location))
            {
                int layer = Doc.ActiveLayer;
                if (Doc.SetLayerMode(layer, NextMode(Doc.LayerMode(layer)))) Fire();
                return;
            }

            if (TrackRect.Contains(e.Location))
            {
                draggingOpacity = true;
                dragPushed = false;
                Capture = true;
                ApplyOpacityAt(e.X);
                return;
            }

            int row = RowAt(e.Location);
            if (row < 0) return;
            int index = LayerAtRow(row);
            if (EyeRect(row).Contains(e.Location))
            {
                Doc.SetLayerVisible(index, !Doc.LayerVisible(index));
                Fire();
                return;
            }
            if (LockRect(row).Contains(e.Location))
            {
                Doc.SetLayerLocked(index, !Doc.LayerLocked(index));
                Fire();
                return;
            }
            if (Doc.ActiveLayer != index)
            {
                Doc.ActiveLayer = index;
                Fire();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (draggingOpacity)
            {
                draggingOpacity = false;
                dragPushed = false;
                Capture = false;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (Doc == null || e.Button != MouseButtons.Left) return;
            int row = RowAt(e.Location);
            if (row < 0) return;
            if (EyeRect(row).Contains(e.Location) || LockRect(row).Contains(e.Location)) return;
            BeginRename(row);
        }

        void ApplyOpacityAt(int x)
        {
            if (Doc == null) return;
            Rectangle t = TrackRect;
            float f = (x - t.X) / (float)Math.Max(1, t.Width - 1);
            int v = (int)Math.Round(Math.Max(0f, Math.Min(1f, f)) * 255);
            int layer = Doc.ActiveLayer;
            if (v == Doc.LayerOpacity(layer)) return;
            if (!dragPushed) { Doc.PushLayerUndo(); dragPushed = true; }
            Doc.SetLayerOpacityLive(layer, v);
            Fire();
        }

        // ------------------------------------------------------------- paint
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            if (Doc == null) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF box = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            T.FillRound(g, box, T.F(10f), T.Inset);
            T.StrokeRound(g, box, T.F(10f), T.Stroke, 1f);

            // header: title, count, BETA marker
            T.Str(g, L.T("LAYERS", "EBENEN"), T.FLabel, T.Dim,
                new Rectangle(Pad, T.P(6), Width - Pad * 2, T.P(16)), T.FL);
            string count = Doc.LayerCount + " / " + PixelDoc.MaxLayers;
            T.Str(g, "BETA", T.FLabel, T.Gold,
                new Rectangle(Pad, T.P(6), Width - Pad * 2, T.P(16)), T.FR);
            int betaW = T.Meas("BETA", T.FLabel).Width + T.P(10);
            T.Str(g, count, T.FMicro, T.Dim,
                new Rectangle(Pad, T.P(6), Width - Pad * 2 - betaW, T.P(16)), T.FR);

            // rows, newest layer on top
            for (int row = 0; row < Doc.LayerCount; row++)
            {
                Rectangle r = RowRect(row);
                if (r.Y + r.Height > FooterY - T.P(4)) break;
                int layer = LayerAtRow(row);
                bool on = Doc.LayerVisible(layer);
                bool locked = Doc.LayerLocked(layer);
                bool isActive = layer == Doc.ActiveLayer;
                RectangleF rr = new RectangleF(T.P(3), r.Y + T.P(2), Width - T.P(6), r.Height - T.P(4));

                if (isActive)
                {
                    T.FillRound(g, rr, T.F(7f), Color.FromArgb(30, T.Gold));
                    T.FillRound(g, new RectangleF(rr.X + T.P(1), rr.Y + T.P(5), T.P(2.5f), rr.Height - T.P(10)),
                        T.F(1.5f), T.Gold);
                }
                else if (row == hoverRow) T.FillRound(g, rr, T.F(7f), T.Segment);

                Rectangle eye = EyeRect(row);
                bool eyeHot = row == hoverEye;
                T.Icon(g, on ? "eye" : "eyeOff",
                    new RectangleF(eye.X, eye.Y, eye.Width, eye.Height),
                    on ? (eyeHot ? T.Text : T.Muted) : T.Dim);

                string name = Doc.LayerName(layer);
                Color nameColor = !on ? T.Dim : (isActive ? T.Gold : T.Muted);
                T.StrEll(g, name, T.FSmall, nameColor, NameRect(row), T.FL);

                int opacity = Doc.LayerOpacity(layer);
                if (opacity < 255)
                {
                    T.Str(g, Math.Round(opacity * 100f / 255f) + "%", T.FMicro, T.Dim,
                        new Rectangle(LockRect(row).Left - T.P(34), r.Y, T.P(31), r.Height), T.FR);
                }

                Rectangle lockIcon = LockRect(row);
                if (locked || row == hoverLock)
                {
                    T.Icon(g, locked ? "lock" : "unlock",
                        new RectangleF(lockIcon.X, lockIcon.Y, lockIcon.Width, lockIcon.Height),
                        locked ? T.Gold : (row == hoverLock ? T.Text : T.Dim));
                }
            }

            // properties for the active layer
            T.HLine(g, Pad, Width - Pad, RowsTop - T.P(8), T.LineSoft);
            T.Str(g, L.T("OPACITY", "DECKKRAFT"), T.FLabel, T.Dim,
                new Rectangle(Pad, PropsTop, Width - Pad * 2, T.P(14)), T.FL);
            T.Str(g, Math.Round(Doc.LayerOpacity(Doc.ActiveLayer) * 100f / 255f) + "%", T.FMicro, T.Muted,
                new Rectangle(Pad, PropsTop, Width - Pad * 2, T.P(14)), T.FR);

            Rectangle track = TrackRect;
            float ty = track.Y + track.Height / 2f - T.P(2);
            T.FillRound(g, new RectangleF(track.X, ty, track.Width, T.P(4)), T.F(2f), T.Segment);
            float f = Doc.LayerOpacity(Doc.ActiveLayer) / 255f;
            float kx = track.X + T.P(5) + (track.Width - T.P(10)) * f;
            T.FillRound(g, new RectangleF(track.X, ty, Math.Max(T.P(4), kx - track.X), T.P(4)), T.F(2f), T.Gold);
            using (SolidBrush kb = new SolidBrush(T.Gold))
                g.FillEllipse(kb, kx - T.P(5), ty - T.P(3), T.P(10), T.P(10));
            using (Pen kp = new Pen(Color.FromArgb(90, 0, 0, 0), 1f))
                g.DrawEllipse(kp, kx - T.P(5), ty - T.P(3), T.P(10), T.P(10));

            Rectangle blend = BlendRect;
            bool blendHot = hoverBlend;
            T.FillRound(g, new RectangleF(blend.X, blend.Y, blend.Width, blend.Height), T.F(6f),
                blendHot ? T.Segment : T.Card);
            T.StrokeRound(g, new RectangleF(blend.X + 0.5f, blend.Y + 0.5f, blend.Width - 1f, blend.Height - 1f),
                T.F(6f), blendHot ? T.StrokeHot : T.Stroke, 1f);
            T.Str(g, L.T("BLEND", "FÜLLMETHODE"), T.FLabel, T.Dim,
                new Rectangle(blend.X + T.P(8), blend.Y, T.P(52), blend.Height), T.FL);
            T.StrEll(g, ModeName(Doc.LayerMode(Doc.ActiveLayer)), T.FMicro, T.Text,
                new Rectangle(blend.X + T.P(54), blend.Y, blend.Width - T.P(78), blend.Height), T.FR);
            T.Icon(g, "down", new RectangleF(blend.Right - T.P(18), blend.Y + (blend.Height - T.P(10)) / 2f, T.P(10), T.P(10)), T.Dim);

            // stack actions
            for (int slot = 0; slot <= SlotDel; slot++)
            {
                Rectangle br = ButtonRect(slot);
                bool on = IsEnabled(slot);
                if (on && slot == hoverBtn)
                    T.FillRound(g, new RectangleF(br.X, br.Y, br.Width, br.Height), T.F(6f), T.Segment);
                int inset = T.P(5);
                T.Icon(g, ButtonIcon(slot),
                    new RectangleF(br.X + inset, br.Y + inset, br.Width - inset * 2, br.Height - inset * 2),
                    on ? (slot == hoverBtn ? T.Text : T.Muted) : T.Dim);
            }

            // quiet hint under the list so renaming is discoverable
            int listBottom = RowsTop + Doc.LayerCount * RowH;
            if (listBottom + T.P(26) < FooterY)
                T.StrEll(g, L.T("Double-click to rename", "Doppelklick: umbenennen"),
                    T.FMicro, T.Dim,
                    new Rectangle(Pad, listBottom + T.P(8), Width - Pad * 2, T.P(14)), T.FL);

            // rename field sits inside a small ring so it reads as an edit box
            if (renameBox != null)
            {
                Rectangle r = renameBox.Bounds;
                T.FillRound(g, new RectangleF(r.X - T.P(3), r.Y - T.P(2), r.Width + T.P(6), r.Height + T.P(4)),
                    T.F(5f), T.Inset);
                T.StrokeRound(g, new RectangleF(r.X - T.P(3) + 0.5f, r.Y - T.P(2) + 0.5f, r.Width + T.P(5), r.Height + T.P(3)),
                    T.F(5f), T.Gold, 1f);
            }
        }

        // ------------------------------------------------------------- test hooks
        // Used by the self-test to exercise the real hit rectangles without a message loop.
        public Rectangle TrackRectDebug { get { return TrackRect; } }
        public Rectangle BlendRectDebug { get { return BlendRect; } }
        public Rectangle RowRectDebug(int row) { return RowRect(row); }
        public Rectangle EyeRectDebug(int row) { return EyeRect(row); }
        public Rectangle LockRectDebug(int row) { return LockRect(row); }
        public bool Renaming { get { return renameBox != null; } }

        public void ClickAt(int x, int y)
        {
            OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, x, y, 0));
            OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, x, y, 0));
        }

        public void RenameActiveTo(string name)
        {
            if (Doc == null) return;
            BeginRename(Doc.LayerCount - 1 - Doc.ActiveLayer);
            if (renameBox == null) return;
            renameBox.Text = name;
            EndRename(true);
        }

        /// <summary>Runs a footer action; every one of them is a single undo step.</summary>
        public bool FireButton(int slot)
        {
            if (Doc == null || !IsEnabled(slot)) return false;
            bool ok;
            switch (slot)
            {
                case SlotAdd: ok = Doc.AddLayer(); break;
                case SlotCopy: ok = Doc.DuplicateLayer(); break;
                case SlotMerge: ok = Doc.MergeDown(); break;
                case SlotUp: ok = Doc.MoveLayer(1); break;
                case SlotDown: ok = Doc.MoveLayer(-1); break;
                default: ok = Doc.DeleteLayer(); break;
            }
            if (ok) Fire();
            return ok;
        }
    }
    /// <summary>40 x 40 pixel drawing surface.</summary>
    internal class CanvasView : Control
    {
        public PixelDoc Doc;
        public Color DrawColor = Color.Black;
        public int Tool; // 0 pencil, 1 eraser, 2 fill, 3 picker
        public bool Mirror = false;
        public bool ShowGrid = true;
        public int Guides = 0;   // 0 off, 1 centre, 2 thirds, 3 quarters
        public Action<Color> Picked;
        public Action UserZoomed;
        /// <summary>Raised when a paint attempt hits a locked layer, so the app can explain why.</summary>
        public Action LockedAttempt;
        public event Action HoverChanged;

        public int HoverX { get { return hx; } }
        public int HoverY { get { return hy; } }

        int zoom = 10;
        Bitmap cache;
        int cacheVersion = -1, cacheZoom = -1, cacheDoc = -1;

        /// <summary>Performance mode (beta): cache the static board and repaint only dirty pixel cells.</summary>
        public bool PerfMode;

        Bitmap staticLayer;
        int staticKey;
        Bitmap overlay;   // grid and guides, cached separately so they never touch the artwork
        int overlayKey;
        Bitmap surface;    // the finished canvas at display resolution, patched cell by cell
        int surfaceKey;
        int surfaceVersion = -1;
        /// <summary>Test hook: how often the cached canvas had to be rebuilt from scratch.</summary>
        internal int SurfaceBuilds;

        bool drawing;
        Point last;
        int hx = -1, hy = -1;
        Region strokeDirty;   // cells repainted since the stroke began
        readonly List<Rectangle> scratch = new List<Rectangle>();
        PixelDoc watched;     // document whose Changed event this canvas listens to
        bool selfWrite;       // our own pixel writes must not invalidate the caches

        public CanvasView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Opaque, true);
            BackColor = T.Card;
            Cursor = Cursors.Cross;
            TabStop = false;
        }

        public int Zoom
        {
            get { return zoom; }
            set
            {
                int z = Math.Max(3, Math.Min(24, value));
                if (z == zoom) return;
                zoom = z;
                Invalidate();
            }
        }

        void DropCache()
        {
            if (cache != null) { cache.Dispose(); cache = null; }
            cacheVersion = -1;
            cacheDoc = -1;
        }

        void DropStatic()
        {
            if (staticLayer != null) { staticLayer.Dispose(); staticLayer = null; }
            staticKey = 0;
            if (overlay != null) { overlay.Dispose(); overlay = null; }
            overlayKey = 0;
            DropSurface();
        }

        void DropSurface()
        {
            if (surface != null) { surface.Dispose(); surface = null; }
            surfaceKey = 0;
            surfaceVersion = -1;
        }

        /// <summary>Everything the finished surface depends on except the artwork itself.</summary>
        int SurfaceKey()
        {
            unchecked
            {
                int k = StaticKey();
                k = k * 397 ^ OverlayKey();
                k = k * 397 ^ Doc.GetHashCode();
                return k;
            }
        }

        /// <summary>
        /// Performance mode keeps one already-rendered canvas bitmap. Painting is then a copy of
        /// the clip rectangle, and a stroke patches the handful of cells it crossed instead of
        /// scaling the 40 x 40 composite over the whole canvas again.
        /// </summary>
        void EnsureSurface()
        {
            if (Doc == null || Width <= 0 || Height <= 0) { DropSurface(); return; }
            int key = SurfaceKey();
            bool fits = surface != null && surface.Width == Width && surface.Height == Height;
            if (fits && surfaceKey == key && surfaceVersion == Doc.Version) return;
            if (!fits)
            {
                DropSurface();
                surface = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            }
            surfaceKey = key;
            surfaceVersion = Doc.Version;
            SurfaceBuilds++;
            using (Graphics sg = Graphics.FromImage(surface))
            {
                sg.SmoothingMode = SmoothingMode.None;
                Rectangle r = CanvasRect();
                if (staticLayer != null) sg.DrawImageUnscaled(staticLayer, 0, 0);
                if (cache != null)
                {
                    sg.DrawImageUnscaled(cache, r);
                }
                else
                {
                    using (Bitmap src = Doc.ToBitmap())
                    {
                        sg.InterpolationMode = InterpolationMode.NearestNeighbor;
                        sg.PixelOffsetMode = PixelOffsetMode.Half;
                        sg.DrawImage(src, r);
                    }
                }
                if (overlay != null) sg.DrawImageUnscaled(overlay, 0, 0);
                // half-pixel offset belongs to the artwork copy only: the rounded rim must cover
                // the same pixels as the normal render
                sg.PixelOffsetMode = PixelOffsetMode.None;
                using (Pen p = new Pen(T.FrameLine, 1f))
                    sg.DrawRectangle(p, r.X - 1, r.Y - 1, r.Width + 1, r.Height + 1);
                sg.SmoothingMode = SmoothingMode.AntiAlias;
                T.StrokeRound(sg, BoardRect(), T.F(12f), T.Stroke, 1f);
            }
        }

        /// <summary>Copies part of the cached surface; the only work a normal repaint does.</summary>
        void BlitSurface(Graphics g, Rectangle clip)
        {
            if (surface == null) return;
            Rectangle src = Rectangle.Intersect(clip, new Rectangle(0, 0, surface.Width, surface.Height));
            if (src.Width <= 0 || src.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.DrawImage(surface, src, src, GraphicsUnit.Pixel);
        }

        /// <summary>Listens to the document so an outside edit (undo, layer switch) drops the caches.</summary>
        void WatchDoc()
        {
            if (watched == Doc) return;
            if (watched != null) watched.Changed -= DocChanged;
            watched = Doc;
            if (watched != null) watched.Changed += DocChanged;
        }

        void DocChanged()
        {
            if (selfWrite) return;
            DropCache();
            DropSurface();
        }

        void DrawHover(Graphics g)
        {
            if (hx < 0 || drawing || Doc == null) return;
            Rectangle r = CanvasRect();
            using (Pen p = new Pen(T.HoverOutline, 1.5f))
                g.DrawRectangle(p, r.X + hx * zoom, r.Y + hy * zoom, zoom - 1, zoom - 1);
        }

        /// <summary>
        /// Repaints the cells of a stroke segment inside the cached surface, then asks for only
        /// those cells. The clip is the cell region itself, not its bounding box, so a fast flick
        /// across the canvas costs the same as a slow one.
        /// </summary>
        void PatchCells(int x0, int y0, int x1, int y1)
        {
            if (!PerfMode || Doc == null || surface == null || cache == null)
            {
                Invalidate();
                return;
            }

            WatchDoc();
            Rectangle r = CanvasRect();
            PatchCache(x0, y0, x1, y1);

            List<Rectangle> cells = scratch;
            cells.Clear();
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int x = x0, y = y0;
            while (true)
            {
                AddCell(cells, r, x, y);
                if (Mirror) AddCell(cells, r, PixelDoc.N - 1 - x, y);
                if (x == x1 && y == y1) break;
                int e2 = err * 2;
                if (e2 >= dy) { err += dy; x += sx; }
                if (e2 <= dx) { err += dx; y += sy; }
            }
            if (cells.Count == 0) { surfaceVersion = Doc.Version; return; }

            RectangleF[] rects = new RectangleF[cells.Count];
            for (int i = 0; i < cells.Count; i++) rects[i] = cells[i];
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddRectangles(rects);
                using (Region region = new Region(path))
                using (Graphics g = Graphics.FromImage(surface))
                {
                    g.SmoothingMode = SmoothingMode.None;
                    GraphicsState st = g.Save();
                    g.SetClip(region, CombineMode.Replace);
                    // three copies for the whole segment: checkerboard, the patched artwork and
                    // the grid back on top
                    if (staticLayer != null) g.DrawImageUnscaled(staticLayer, 0, 0);
                    g.DrawImageUnscaled(cache, r);
                    if (overlay != null) g.DrawImageUnscaled(overlay, 0, 0);
                    g.Restore(st);
                    // only the cells of this segment are invalid; the accumulated region is
                    // used while painting to avoid copying a diagonal's bounding box
                    Invalidate(region);
                    if (strokeDirty == null) strokeDirty = new Region();
                    strokeDirty.Union(region);
                }
            }
            surfaceVersion = Doc.Version;
        }

        void AddCell(List<Rectangle> cells, Rectangle r, int x, int y)
        {
            Rectangle cell = Rectangle.Intersect(new Rectangle(r.X + x * zoom, r.Y + y * zoom, zoom, zoom), r);
            if (cell.Width > 0 && cell.Height > 0) cells.Add(cell);
        }

        /// <summary>Writes the new colours of the touched cells into the zoomed artwork cache.</summary>
        void PatchCache(int x0, int y0, int x1, int y1)
        {
            if (cache == null) return;
            using (Graphics g = Graphics.FromImage(cache))
            using (SolidBrush px = new SolidBrush(Color.Black))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.SmoothingMode = SmoothingMode.None;
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = dx + dy;
                int x = x0, y = y0;
                while (true)
                {
                    px.Color = Color.FromArgb(Doc.CompositeAt(x, y));
                    g.FillRectangle(px, x * zoom, y * zoom, zoom, zoom);
                    if (Mirror)
                    {
                        px.Color = Color.FromArgb(Doc.CompositeAt(PixelDoc.N - 1 - x, y));
                        g.FillRectangle(px, (PixelDoc.N - 1 - x) * zoom, y * zoom, zoom, zoom);
                    }
                    if (x == x1 && y == y1) break;
                    int e2 = err * 2;
                    if (e2 >= dy) { err += dy; x += sx; }
                    if (e2 <= dx) { err += dx; y += sy; }
                }
            }
            cacheVersion = Doc.Version;
            cacheDoc = Doc.GetHashCode();
        }

        int StaticKey()
        {
            unchecked
            {
                // The palette, not just light/dark: navy and custom themes sit in the same
                // brightness family, so a boolean key left the cached board painted in the
                // colours of the previous theme.
                int k = Width * 397 ^ Height;
                k = k * 397 ^ zoom;
                k = k * 397 ^ (int)T.Mode;
                k = k * 397 ^ BackColor.ToArgb();
                k = k * 397 ^ T.Inset.ToArgb();
                k = k * 397 ^ T.Check1.ToArgb();
                k = k * 397 ^ T.Check2.ToArgb();
                return k;
            }
        }

        /// <summary>Board tone and checkerboard, painted once per size, zoom and theme.</summary>
        void EnsureStatic()
        {
            int key = StaticKey();
            if (staticLayer != null && staticKey == key
                && staticLayer.Width == Width && staticLayer.Height == Height) return;
            DropStatic();
            if (Width <= 0 || Height <= 0) return;
            staticLayer = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(staticLayer))
            {
                using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, 0, 0, Width, Height);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                T.FillRound(g, BoardRect(), T.F(12f), T.Inset);
                g.SmoothingMode = SmoothingMode.None;
                T.Checker(g, CanvasRect());
            }
            staticKey = key;
        }

        Rectangle CanvasRect()
        {
            int side = PixelDoc.N * zoom;
            int x = (Width - side) / 2;
            int y = (Height - side) / 2;
            return new Rectangle(x, y, side, side);
        }

        int OverlayKey()
        {
            unchecked
            {
                int k = Width * 397 ^ Height;
                k = k * 397 ^ zoom;
                k = k * 397 ^ (ShowGrid ? 1 : 0);
                k = k * 397 ^ Guides;
                k = k * 397 ^ (int)T.Mode;
                k = k * 397 ^ T.GridLine.ToArgb();
                k = k * 397 ^ T.Gold.ToArgb();
                return k;
            }
        }

        /// <summary>Grid and guides over a transparent sheet; one blit replaces ~80 line draws.</summary>
        void EnsureOverlay()
        {
            int key = OverlayKey();
            if (overlay != null && overlayKey == key && overlay.Width == Width && overlay.Height == Height) return;
            if (overlay != null) { overlay.Dispose(); overlay = null; }
            if (Width <= 0 || Height <= 0) return;
            overlay = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(overlay))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                Rectangle r = CanvasRect();
                DrawGrid(g, r);
                DrawGuides(g, r);
            }
            overlayKey = key;
        }

        void DrawGrid(Graphics g, Rectangle r)
        {
            if (!ShowGrid || zoom < 11) return;
            using (Pen p = new Pen(T.GridLine, 1f))
            {
                for (int i = 1; i < PixelDoc.N; i++)
                {
                    int x = r.X + i * zoom;
                    int y = r.Y + i * zoom;
                    g.DrawLine(p, x, r.Y, x, r.Bottom);
                    g.DrawLine(p, r.X, y, r.Right, y);
                }
            }
        }

        void DrawGuides(Graphics g, Rectangle r)
        {
            if (Guides <= 0) return;
            using (Pen under = new Pen(Color.FromArgb(90, 0, 0, 0), 1f))
            using (Pen mark = new Pen(T.Gold, 1f))
            {
                mark.DashPattern = new float[] { 3f, 3f };
                foreach (int f in GuideFractions())
                {
                    int gx = r.X + f * zoom;
                    int gy = r.Y + f * zoom;
                    g.DrawLine(under, gx, r.Y, gx, r.Bottom);
                    g.DrawLine(under, r.X, gy, r.Right, gy);
                    g.DrawLine(mark, gx, r.Y, gx, r.Bottom);
                    g.DrawLine(mark, r.X, gy, r.Right, gy);
                }
            }
        }

        RectangleF BoardRect()
        {
            return new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
        }

        void EnsureCache()
        {
            if (Doc == null) { DropCache(); return; }
            if (cache != null && cacheVersion == Doc.Version && cacheZoom == zoom
                && cacheDoc == Doc.GetHashCode()) return;
            int side = PixelDoc.N * zoom;
            // perf mode repaints into the same bitmap instead of allocating one per move
            bool reuse = PerfMode && cache != null && cache.Width == side && cache.Height == side;
            if (!reuse) DropCache();
            using (Bitmap src = Doc.ToBitmap())
            {
                if (!reuse) cache = new Bitmap(side, side, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(cache))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.None;
                    g.DrawImage(src, new Rectangle(0, 0, cache.Width, cache.Height));
                }
            }
            cacheVersion = Doc.Version;
            cacheZoom = zoom;
            cacheDoc = Doc.GetHashCode();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            WatchDoc();
            if (Doc == null)
            {
                using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
                return;
            }

            // Performance mode paints the canvas from a finished bitmap: a repaint copies a
            // rectangle instead of redrawing board, checkerboard, artwork and grid every time.
            if (PerfMode)
            {
                EnsureStatic();
                EnsureOverlay();
                EnsureCache();
                EnsureSurface();
                if (surface == null)
                {
                    using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
                    return;
                }
                Rectangle clip = Rectangle.Intersect(e.ClipRectangle, ClientRectangle);
                if (clip.Width <= 0 || clip.Height <= 0) clip = ClientRectangle;
                // the buffered Graphics is not guaranteed to carry the paint region, and without
                // it a cell-sized repaint still copies the whole canvas
                GraphicsState st = g.Save();
                g.SetClip(clip, CombineMode.Intersect);
                // during a stroke the only change is the patched cells, so never copy the
                // bounding box of a diagonal flick
                if (drawing && strokeDirty != null) g.SetClip(strokeDirty, CombineMode.Intersect);
                BlitSurface(g, clip);
                DrawHover(g);
                g.Restore(st);
                return;
            }

            Rectangle r = CanvasRect();
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.FillRound(g, BoardRect(), T.F(12f), T.Inset);
            g.SmoothingMode = SmoothingMode.None;
            T.Checker(g, r);

            EnsureCache();
            if (cache != null) g.DrawImageUnscaled(cache, r);
            g.SmoothingMode = SmoothingMode.None;

            DrawGrid(g, r);
            DrawGuides(g, r);

            using (Pen p = new Pen(T.FrameLine, 1f))
                g.DrawRectangle(p, r.X - 1, r.Y - 1, r.Width + 1, r.Height + 1);

            DrawHover(g);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.StrokeRound(g, BoardRect(), T.F(12f), T.Stroke, 1f);
        }

        /// <summary>Pixel boundaries the guides sit on.</summary>
        int[] GuideFractions()
        {
            if (Guides == 2) return new int[] { PixelDoc.N / 3, PixelDoc.N * 2 / 3 };
            if (Guides == 3) return new int[] { PixelDoc.N / 4, PixelDoc.N / 2, PixelDoc.N * 3 / 4 };
            return new int[] { PixelDoc.N / 2 };
        }

        bool Cell(Point p, out Point cell)
        {
            Rectangle r = CanvasRect();
            cell = Point.Empty;
            if (p.X < r.X || p.Y < r.Y || p.X >= r.Right || p.Y >= r.Bottom) return false;
            int cx = (p.X - r.X) / zoom;
            int cy = (p.Y - r.Y) / zoom;
            if (cx < 0 || cy < 0 || cx >= PixelDoc.N || cy >= PixelDoc.N) return false;
            cell = new Point(cx, cy);
            return true;
        }

        int PaintColor()
        {
            return Tool == 1 ? 0 : DrawColor.ToArgb();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Doc == null) return;
            Capture = true;
            Point c;
            if (!Cell(e.Location, out c)) return;
            bool erase = e.Button == MouseButtons.Right;
            if (Tool == 3 && !erase)
            {
                int px = Doc.CompositeAt(c.X, c.Y);
                if (Picked != null) Picked(Color.FromArgb(px));
                return;
            }
            if (Doc.ActiveLocked)
            {
                if (LockedAttempt != null) LockedAttempt();
                return;
            }
            if (Tool == 2 && !erase)
            {
                selfWrite = true;
                try
                {
                    Doc.BeginStroke();
                    Doc.FloodFill(c.X, c.Y, PaintColor());
                    Doc.EndStroke();
                }
                finally { selfWrite = false; }
                Invalidate();
                return;
            }
            int argb = erase ? 0 : PaintColor();
            if (PerfMode)
            {
                // the surface has to be current before the first cell is patched into it
                EnsureStatic();
                EnsureOverlay();
                EnsureCache();
                EnsureSurface();
            }
            DropStrokeDirty();
            selfWrite = true;
            try
            {
                Doc.BeginStroke();
                Doc.SetPixels(c.X, c.Y, argb, Mirror);
            }
            finally { selfWrite = false; }
            drawing = true;
            last = c;
            if (PerfMode) PatchCells(c.X, c.Y, c.X, c.Y);
            else Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point c;
            bool inCell = Cell(e.Location, out c);
            if (drawing && Doc != null && inCell)
            {
                int fromX = last.X, fromY = last.Y;
                if (c != last)
                {
                    selfWrite = true;
                    try { Doc.StrokeLine(last.X, last.Y, c.X, c.Y, PaintColor(), Mirror); }
                    finally { selfWrite = false; }
                    last = c;
                    // perf mode patches just the cells on the segment into the cached canvas
                    if (PerfMode) PatchCells(fromX, fromY, c.X, c.Y);
                    else Invalidate();
                }
                return;
            }
            int nx = inCell ? c.X : -1;
            int ny = inCell ? c.Y : -1;
            if (nx != hx || ny != hy)
            {
                if (PerfMode)
                {
                    if (hx >= 0) Invalidate(HoverRect(hx, hy));
                    if (nx >= 0) Invalidate(HoverRect(nx, ny));
                }
                else Invalidate();
                hx = nx; hy = ny;
                if (HoverChanged != null) HoverChanged();
            }
        }

        /// <summary>Test hook: paints as if a stroke were running, covering the fast path.</summary>
        internal void DebugDrawing(bool value)
        {
            drawing = value;
            Invalidate();
        }

        /// <summary>Test hook: patches a cell segment exactly the way a mouse move does.</summary>
        internal void DebugPatch(int x0, int y0, int x1, int y1)
        {
            PatchCells(x0, y0, x1, y1);
        }

        /// <summary>Test hook: draws a segment through the real stroke path.</summary>
        internal void DebugStroke(int x0, int y0, int x1, int y1, int argb)
        {
            if (Doc == null) return;
            selfWrite = true;
            try { Doc.StrokeLine(x0, y0, x1, y1, argb, false); }
            finally { selfWrite = false; }
            PatchCells(x0, y0, x1, y1);
        }

        /// <summary>Test hook: paints one pixel through the real stroke path.</summary>
        internal void DebugPaintPixel(int x, int y, int argb)
        {
            if (Doc == null) return;
            EnsureCache();
            EnsureSurface();
            selfWrite = true;
            try { Doc.SetPixels(x, y, argb, false); }
            finally { selfWrite = false; }
            PatchCells(x, y, x, y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
            EndDraw();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hx != -1 || hy != -1)
            {
                if (PerfMode && hx >= 0) Invalidate(HoverRect(hx, hy));
                else Invalidate();
                hx = -1; hy = -1;
                if (HoverChanged != null) HoverChanged();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            EndDraw();
        }

        void EndDraw()
        {
            if (!drawing) return;
            drawing = false;
            DropStrokeDirty();
            if (Doc != null)
            {
                selfWrite = true;
                try { Doc.EndStroke(); }
                finally { selfWrite = false; }
            }
        }

        void DropStrokeDirty()
        {
            if (strokeDirty == null) return;
            strokeDirty.Dispose();
            strokeDirty = null;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int z = zoom + (e.Delta > 0 ? 1 : -1);
            Zoom = z;
            if (UserZoomed != null) UserZoomed();
            Invalidate();
        }

        Rectangle CellRect(int x, int y, int pad)
        {
            Rectangle r = CanvasRect();
            return new Rectangle(r.X + x * zoom - pad, r.Y + y * zoom - pad, zoom + pad * 2, zoom + pad * 2);
        }

        Rectangle HoverRect(int x, int y)
        {
            return CellRect(x, y, T.P(2));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            DropCache();
            DropStatic();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (watched != null) { watched.Changed -= DocChanged; watched = null; }
                DropCache();
                DropStatic();
                DropStrokeDirty();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Live badge preview at a given pixel scale.</summary>
    internal class PreviewView : Control
    {
        PixelDoc doc;
        Bitmap cache;
        int cacheVersion = -1, cacheCell = -1;

        public PreviewView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
        }

        public PixelDoc Doc
        {
            get { return doc; }
            set { doc = value; cacheVersion = -1; cacheCell = -1; Invalidate(); }
        }

        void EnsureCache(int cell)
        {
            if (doc == null) return;
            if (cache != null && cacheVersion == doc.Version && cacheCell == cell) return;
            if (cache != null) { cache.Dispose(); cache = null; }
            using (Bitmap src = doc.ToBitmap())
            {
                cache = new Bitmap(PixelDoc.N * cell, PixelDoc.N * cell, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(cache))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(src, new Rectangle(0, 0, cache.Width, cache.Height));
                }
            }
            cacheVersion = doc.Version;
            cacheCell = cell;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF board = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            T.FillRound(g, board, T.F(10f), T.Inset);

            g.SmoothingMode = SmoothingMode.None;
            int cell = Math.Max(1, Math.Min(Width, Height) / PixelDoc.N);
            int side = PixelDoc.N * cell;
            int x = (Width - side) / 2, y = (Height - side) / 2;
            Rectangle r = new Rectangle(x, y, side, side);
            using (GraphicsPath clip = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(10f)))
            {
                GraphicsState st = g.Save();
                g.SetClip(clip, CombineMode.Intersect);
                T.Checker(g, r);
                EnsureCache(cell);
                if (cache != null) g.DrawImageUnscaled(cache, r);
                g.Restore(st);
            }
            using (Pen p = new Pen(T.FrameLine, 1f))
                g.DrawRectangle(p, r.X - 1, r.Y - 1, r.Width + 1, r.Height + 1);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.StrokeRound(g, board, T.F(10f), T.Stroke, 1f);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { if (cache != null) cache.Dispose(); }
            base.Dispose(disposing);
        }
    }

    /// <summary>Scrollable list of saved badges.</summary>
    internal class GalleryView : Control
    {
        public List<BadgeEntry> Items = new List<BadgeEntry>();
        public Action<BadgeEntry> LoadRequested;
        public Action<BadgeEntry> DeleteRequested;

        int scroll;
        int hover = -1;
        int hoverDel = -1;

        public GalleryView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
        }

        int RowH { get { return T.P(58); } }

        int ContentH { get { return Items.Count * RowH + T.P(4); } }

        int MaxScroll { get { return Math.Max(0, ContentH - Height); } }

        public void SetItems(List<BadgeEntry> items)
        {
            Items = items;
            scroll = Math.Min(scroll, MaxScroll);
            if (Items.Count == 0) scroll = 0;
            hover = -1; hoverDel = -1;
            Invalidate();
        }

        int RowAt(Point p)
        {
            if (Items.Count == 0) return -1;
            int y = p.Y + scroll - T.P(2);
            if (y < 0) return -1;
            int i = y / RowH;
            if (i < 0 || i >= Items.Count) return -1;
            return i;
        }

        Rectangle RowRect(int i)
        {
            return new Rectangle(0, T.P(2) + i * RowH - scroll, Width, RowH - T.P(4));
        }

        Rectangle DelRect(int i)
        {
            Rectangle r = RowRect(i);
            int s = T.P(26);
            return new Rectangle(r.Right - s - T.P(8), r.Y + (r.Height - s) / 2, s, s);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Items.Count == 0)
            {
                RectangleF box = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
                using (GraphicsPath p = T.Round(box, T.F(12f)))
                using (Pen pen = new Pen(T.DashLine, 1.3f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    pen.DashPattern = new float[] { 3f, 2.4f };
                    g.DrawPath(pen, p);
                }
                float ic = T.F(26f);
                T.Icon(g, "folder", new RectangleF((Width - ic) / 2f, Height / 2f - T.P(52), ic, ic),
                    T.IconIdle);
                Size s1 = T.Meas("No badges yet", T.FBody);
                T.Str(g, L.T("No badges yet", "Noch keine Badges"), T.FBody, T.Text,
                    new Rectangle(0, Height / 2 - T.P(18), Width, s1.Height + T.P(4)), T.FC);
                T.Str(g, L.T("Saved badges appear here with their preview.", "Gespeicherte Badges erscheinen hier mit Vorschau."),
                    T.FSmall, T.Dim, new Rectangle(0, Height / 2, Width, T.P(18)), T.FC);
                return;
            }

            GraphicsState st = g.Save();
            g.SetClip(new Rectangle(0, 0, Width, Height));

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle r = RowRect(i);
                if (r.Bottom < -T.P(10) || r.Y > Height + T.P(10)) continue;
                BadgeEntry it = Items[i];
                bool isHot = (i == hover);

                if (isHot) T.FillRound(g, new RectangleF(r.X, r.Y, r.Width, r.Height), T.F(8f), T.HoverSoft);

                int thumb = T.P(40);
                Rectangle tr = new Rectangle(r.X + T.P(10), r.Y + (r.Height - thumb) / 2, thumb, thumb);
                using (GraphicsPath tp = T.Round(new RectangleF(tr.X, tr.Y, tr.Width, tr.Height), T.F(5f)))
                {
                    GraphicsState ts = g.Save();
                    g.SetClip(tp, CombineMode.Intersect);
                    T.Checker(g, tr);
                    if (it.Thumb != null)
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(it.Thumb, tr);
                    }
                    g.Restore(ts);
                }
                T.StrokeRound(g, new RectangleF(tr.X + 0.5f, tr.Y + 0.5f, tr.Width - 1f, tr.Height - 1f),
                    T.F(5f), T.FrameLine, 1f);

                int tx = tr.Right + T.P(12);
                int tw = r.Width - tx - T.P(48);
                string nm = string.IsNullOrEmpty(it.Name) ? L.T("Untitled badge", "Unbenanntes Badge") : it.Name;
                T.Str(g, nm, T.FBody, T.Text,
                    new Rectangle(tx, r.Y + T.P(12), Math.Max(10, tw), T.P(18)), T.FL);
                string meta = it.Created.ToString("dd MMM yyyy · HH:mm", L.Culture) + "  ·  40 × 40";
                T.Str(g, meta, T.FSmall, T.Dim,
                    new Rectangle(tx, r.Y + T.P(29), Math.Max(10, tw), T.P(16)), T.FL);

                if (isHot)
                {
                    Rectangle dr = DelRect(i);
                    bool dh = (hoverDel == i);
                    T.FillRound(g, new RectangleF(dr.X, dr.Y, dr.Width, dr.Height), T.F(7f),
                        dh ? Color.FromArgb(70, T.Danger.R, T.Danger.G, T.Danger.B) : T.Hover);
                    float s = T.F(13f);
                    T.Icon(g, "trash", new RectangleF(dr.X + (dr.Width - s) / 2f, dr.Y + (dr.Height - s) / 2f, s, s),
                        dh ? T.DangerText : T.Muted);
                }
            }
            g.Restore(st);

            if (ContentH > Height)
            {
                int barH = Math.Max(T.P(28), (int)((float)Height * Height / ContentH));
                int maxScroll = MaxScroll;
                int barY = maxScroll == 0 ? 0 : (int)((float)scroll * (Height - barH) / maxScroll);
                T.FillRound(g, new RectangleF(Width - T.P(4), barY, T.P(3), barH), T.F(2f),
                    T.ScrollBar);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = RowAt(e.Location);
            int d = (i >= 0 && DelRect(i).Contains(e.Location)) ? i : -1;
            if (i != hover || d != hoverDel)
            {
                hover = i; hoverDel = d;
                Cursor = (d >= 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = -1; hoverDel = -1; Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int i = RowAt(e.Location);
            if (i < 0) return;
            if (DelRect(i).Contains(e.Location))
            {
                if (DeleteRequested != null) DeleteRequested(Items[i]);
                return;
            }
            if (LoadRequested != null) LoadRequested(Items[i]);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int ns = scroll - e.Delta / 3;
            ns = Math.Max(0, Math.Min(MaxScroll, ns));
            if (ns != scroll) { scroll = ns; Invalidate(); }
        }
    }
}
