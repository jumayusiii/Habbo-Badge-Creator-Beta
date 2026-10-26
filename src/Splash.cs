using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>Short startup splash: the HABBO wordmark fades in, holds and fades out.</summary>
    internal class SplashForm : Form
    {
        readonly Timer anim = new Timer();
        const int FadeIn = 320, Hold = 820, FadeOut = 340;
        int elapsed;

        public SplashForm()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            Text = "Habbo Badge Creator";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            BackColor = T.Bg1;
            DoubleBuffered = true;
            ClientSize = new Size(T.P(520), T.P(300));
            Opacity = 0;
            Cursor = Cursors.Hand;
            KeyPreview = true;

            anim.Interval = 16;
            anim.Tick += delegate { Step(); };
        }

        /// <summary>Freezes the splash at full opacity for a documentation render.</summary>
        public void PrepareShot()
        {
            anim.Stop();
            Opacity = 1;
        }

        void Step()
        {
            elapsed += anim.Interval;
            float o;
            if (elapsed < FadeIn) o = (float)elapsed / FadeIn;
            else if (elapsed < FadeIn + Hold) o = 1f;
            else o = 1f - (float)(elapsed - FadeIn - Hold) / FadeOut;

            if (o <= 0f)
            {
                anim.Stop();
                Close();
                return;
            }
            Opacity = Math.Min(1f, o);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(16f)))
                Region = new Region(p);
            anim.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush b = new SolidBrush(T.Bg1)) g.FillRectangle(b, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (T.Wordmark != null)
            {
                int lw = T.P(T.Wordmark.Width * 3f);
                int lh = T.P(T.Wordmark.Height * 3f);
                int lx = (Width - lw) / 2;
                int ly = T.P(56);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(T.Wordmark, new Rectangle(lx, ly, lw, lh));
            }

            T.FillRound(g, new RectangleF((Width - T.P(40)) / 2f, T.P(216), T.P(40), T.P(2)), T.F(1f), T.Gold);
            T.Str(g, L.T("BADGE CREATOR · BETA", "BADGE CREATOR · BETA"), T.FLabel, T.Muted,
                new Rectangle(0, T.P(228), Width, T.P(18)), T.FC);
            T.Str(g, "v" + AppInfo.Version, T.FMicro, T.Dim,
                new Rectangle(0, T.P(250), Width, T.P(16)), T.FC);

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(16f)))
            using (Pen pen = new Pen(T.Rim, 1f))
                g.DrawPath(pen, rim);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            anim.Stop();
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            anim.Stop();
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            anim.Stop();
            anim.Dispose();
            base.OnFormClosed(e);
        }
    }
}
