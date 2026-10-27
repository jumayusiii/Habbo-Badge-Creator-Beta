using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>The tone a message sheet takes: warning, error or a neutral note.</summary>
    internal enum MessageKind { Warning, Error, Info }

    /// <summary>
    /// The app's own message sheet. It replaces Windows' stock <see cref="MessageBox"/> for
    /// confirmations and error notes: the same rounded, borderless chrome as Settings and
    /// Changelog, drawn only from the active palette, so it fits Dark, Light, Navy and Custom.
    /// A confirmation can carry the badge being deleted as a thumbnail; Esc cancels, Enter
    /// confirms, and the X only ever cancels.
    /// </summary>
    internal class MessageSheetForm : Form
    {
        readonly MessageKind kind;
        readonly string title, body, detail;
        readonly bool confirm;
        readonly Image preview;
        readonly int bodyTop, bodyWidth, previewSize;

        FlatBtn cancelBtn, confirmBtn;
        WinBtn closeBtn;

        /// <summary>True when the sheet offers a way out besides the X; used by the self-test.</summary>
        internal bool HasCancelAction { get { return cancelBtn != null; } }

        /// <summary>True when the confirming action is painted in the danger tone; used by the self-test.</summary>
        internal bool HasDangerAction { get { return confirmBtn != null && confirmBtn.Danger; } }

        /// <summary>Opens a themed yes/no sheet and returns true only for the confirming button.</summary>
        public static bool Confirm(IWin32Window owner, string title, string body, string detail,
            string confirmLabel, string cancelLabel, Image preview)
        {
            using (MessageSheetForm f = new MessageSheetForm(MessageKind.Warning, title, body, detail,
                       confirmLabel, cancelLabel, preview, true))
                return f.ShowDialog(owner) == DialogResult.OK;
        }

        /// <summary>Opens a themed notice with a single OK button.</summary>
        public static void Error(IWin32Window owner, string title, string body, string detail)
        {
            using (MessageSheetForm f = new MessageSheetForm(MessageKind.Error, title, body, detail,
                       L.T("OK", "OK"), null, null, false))
                f.ShowDialog(owner);
        }

        public MessageSheetForm(MessageKind kind, string title, string body, string detail,
            string confirmLabel, string cancelLabel, Image preview, bool confirm)
        {
            this.kind = kind;
            this.title = title ?? "";
            this.body = body ?? "";
            this.detail = detail ?? "";
            this.preview = preview;
            this.confirm = confirm;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = T.Card;
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            int pad = T.P(22);
            previewSize = preview != null ? T.P(62) : 0;
            int gap = preview != null ? T.P(16) : 0;
            bodyWidth = T.P(480) - pad * 2 - previewSize - gap;
            bodyTop = T.P(86);

            int textHeight = Measure(body, T.FBody, bodyWidth).Height;
            if (detail.Length > 0) textHeight += T.P(7) + Measure(detail, T.FSmall, bodyWidth).Height;
            int contentHeight = Math.Max(previewSize, textHeight);
            int buttonTop = bodyTop + contentHeight + T.P(26);
            int height = buttonTop + T.P(38) + pad;
            ClientSize = new Size(T.P(480), height);

            BuildUi(confirmLabel, cancelLabel, buttonTop);
            SheetFade.Attach(this);
        }

        static Size Measure(string text, Font font, int width)
        {
            if (string.IsNullOrEmpty(text)) return Size.Empty;
            return TextRenderer.MeasureText(text, font, new Size(Math.Max(T.P(40), width), T.P(2000)),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        void BuildUi(string confirmLabel, string cancelLabel, int buttonTop)
        {
            int pad = T.P(22);
            int bh = T.P(38);

            confirmBtn = new FlatBtn();
            confirmBtn.Label = confirmLabel;
            confirmBtn.BackColor = BackColor;
            if (confirm)
            {
                confirmBtn.Danger = true;
                confirmBtn.Icon = "trash";
            }
            else confirmBtn.Primary = true;
            confirmBtn.Click += delegate { Commit(); };
            Controls.Add(confirmBtn);

            if (confirm)
            {
                cancelBtn = new FlatBtn();
                cancelBtn.Label = cancelLabel ?? L.T("Cancel", "Abbrechen");
                cancelBtn.BackColor = BackColor;
                cancelBtn.Click += delegate { Cancel(); };
                Controls.Add(cancelBtn);
            }

            closeBtn = new WinBtn();
            closeBtn.Icon = "close";
            closeBtn.BackColor = BackColor;
            closeBtn.Click += delegate { Cancel(); };
            Controls.Add(closeBtn);

            int cw = Math.Max(T.P(112), T.Meas(confirmBtn.Label, T.FBtn).Width + T.P(34) +
                              (confirmBtn.Icon != null ? T.P(21) : 0));
            confirmBtn.SetBounds(ClientSize.Width - pad - cw, buttonTop, cw, bh);
            if (cancelBtn != null)
            {
                int xw = Math.Max(T.P(104), T.Meas(cancelBtn.Label, T.FBtn).Width + T.P(34));
                cancelBtn.SetBounds(confirmBtn.Left - T.P(10) - xw, buttonTop, xw, bh);
            }

            int closeW = T.P(44);
            closeBtn.SetBounds(ClientSize.Width - closeW, 0, closeW, T.P(38));
        }

        void Commit()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void Cancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Escape)
            {
                Cancel();
                return true;
            }
            if ((keyData & Keys.KeyCode) == Keys.Enter)
            {
                if (confirm) Commit();
                else Cancel();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyWindowShape();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyWindowShape();
        }

        void ApplyWindowShape()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.PaintSheet(g, ClientRectangle, BackColor);

            int pad = T.P(22);
            int tile = T.P(34);
            Color tone = kind == MessageKind.Info ? T.Gold : T.Danger;
            RectangleF iconTile = new RectangleF(pad, T.P(14), tile, tile);
            T.FillRound(g, iconTile, T.F(8f), Color.FromArgb(38, tone.R, tone.G, tone.B));
            T.StrokeRound(g, iconTile, T.F(8f), Color.FromArgb(92, tone.R, tone.G, tone.B), 1f);

            float ic = T.F(19f);
            string icon = kind == MessageKind.Info ? "info" : kind == MessageKind.Error ? "error" : "warning";
            T.Icon(g, icon, new RectangleF(iconTile.X + (tile - ic) / 2f, iconTile.Y + (tile - ic) / 2f, ic, ic), tone);

            int titleX = pad + tile + T.P(12);
            T.Str(g, title, T.FCardTitle, T.Text,
                new Rectangle(titleX, T.P(14), ClientSize.Width - titleX - T.P(50), tile), T.FL);
            T.HLine(g, 0, ClientSize.Width, T.P(62), T.LineSoft);

            int tx = pad;
            if (preview != null)
            {
                Rectangle pr = new Rectangle(pad, bodyTop, previewSize, previewSize);
                using (GraphicsPath pp = T.Round(new RectangleF(pr.X, pr.Y, pr.Width, pr.Height), T.F(7f)))
                {
                    GraphicsState st = g.Save();
                    g.SetClip(pp, CombineMode.Intersect);
                    T.Checker(g, pr);
                    if (preview != null)
                    {
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(preview, pr);
                    }
                    g.Restore(st);
                }
                T.StrokeRound(g, new RectangleF(pr.X + 0.5f, pr.Y + 0.5f, pr.Width - 1f, pr.Height - 1f),
                    T.F(7f), T.FrameLine, 1f);
                tx = pr.Right + T.P(16);
            }

            Size bodySize = Measure(body, T.FBody, bodyWidth);
            T.Wrap(g, body, T.FBody, T.Text,
                new Rectangle(tx, bodyTop, bodyWidth, bodySize.Height + T.P(2)));
            if (detail.Length > 0)
            {
                T.Wrap(g, detail, T.FSmall, T.Muted,
                    new Rectangle(tx, bodyTop + bodySize.Height + T.P(7), bodyWidth,
                        Measure(detail, T.FSmall, bodyWidth).Height + T.P(2)));
            }

            T.HLine(g, 0, ClientSize.Width, ClientSize.Height - T.P(66), T.LineSoft);

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Stroke, 1f))
                g.DrawPath(pen, rim);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < T.P(58))
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
