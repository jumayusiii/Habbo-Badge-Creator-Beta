using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>Modal settings sheet: appearance (dark / light / navy) and Discord Rich Presence.</summary>
    internal class SettingsForm : Form
    {
        public AppSettings Result;
        public bool Saved;
        /// <summary>Re-applies the palette and repaints the main window while the sheet is open.</summary>
        public Action<AppSettings> PreviewSettings;

        ThemeTile darkTile, lightTile, navyTile, customTile;
        bool perfHot, perfDown;
        FieldBox idField, keyField;
        FlatBtn enableBtn, portalBtn, cancelBtn, saveBtn;
        WinBtn closeBtn;
        string error = "";
        readonly Timer statusTimer = new Timer();

        public SettingsForm(AppSettings current)
        {
            Result = current.Clone();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = T.Card;
            ClientSize = new Size(T.P(580), T.P(660));
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            BuildUi();
            ApplyLanguage();

            statusTimer.Interval = 700;
            statusTimer.Tick += delegate { Invalidate(new Rectangle(0, T.P(540), Width, T.P(50))); };
            statusTimer.Start();
        }

        void BuildUi()
        {
            darkTile = new ThemeTile();
            darkTile.Mode = ThemeMode.Dark;
            darkTile.Click += delegate { ChooseTheme(ThemeMode.Dark); };
            Controls.Add(darkTile);

            lightTile = new ThemeTile();
            lightTile.Mode = ThemeMode.Light;
            lightTile.Click += delegate { ChooseTheme(ThemeMode.Light); };
            Controls.Add(lightTile);

            navyTile = new ThemeTile();
            navyTile.Mode = ThemeMode.Navy;
            navyTile.Click += delegate { ChooseTheme(ThemeMode.Navy); };
            Controls.Add(navyTile);

            customTile = new ThemeTile();
            customTile.Mode = ThemeMode.Custom;
            customTile.Click += delegate { ChooseTheme(ThemeMode.Custom); };
            Controls.Add(customTile);

            idField = new FieldBox(false);
            idField.BackColor = BackColor;
            Controls.Add(idField);

            keyField = new FieldBox(false);
            keyField.BackColor = BackColor;
            Controls.Add(keyField);

            enableBtn = new FlatBtn();
            enableBtn.BackColor = BackColor;
            enableBtn.Click += delegate
            {
                Result.DiscordEnabled = !Result.DiscordEnabled;
                ApplyLanguage();
            };
            Controls.Add(enableBtn);

            portalBtn = new FlatBtn();
            portalBtn.BackColor = BackColor;
            portalBtn.Icon = "discord";
            portalBtn.Click += delegate { OpenPortal(); };
            Controls.Add(portalBtn);

            cancelBtn = new FlatBtn();
            cancelBtn.BackColor = BackColor;
            cancelBtn.Click += delegate { Close(); };
            Controls.Add(cancelBtn);

            saveBtn = new FlatBtn();
            saveBtn.BackColor = BackColor;
            saveBtn.Primary = true;
            saveBtn.Icon = "check";
            saveBtn.Click += delegate { Save(); };
            Controls.Add(saveBtn);

            closeBtn = new WinBtn();
            closeBtn.Icon = "close";
            closeBtn.BackColor = BackColor;
            closeBtn.Click += delegate { Close(); };
            Controls.Add(closeBtn);

            idField.Value = Result.DiscordAppId;
            keyField.Value = Result.DiscordImageKey;
        }

        void ChooseTheme(ThemeMode mode)
        {
            if (Result.Mode == mode)
            {
                // clicking the selected custom tile is the way back into the designer
                if (mode == ThemeMode.Custom) OpenDesigner();
                return;
            }
            Result.LightTheme = mode == ThemeMode.Light;
            Result.NavyTheme = mode == ThemeMode.Navy;
            Result.CustomTheme = mode == ThemeMode.Custom;

            ApplyOwnPalette();
            Preview();
            if (mode == ThemeMode.Custom) OpenDesigner();
        }

        void SyncTiles()
        {
            darkTile.Selected = Result.Mode == ThemeMode.Dark;
            lightTile.Selected = Result.Mode == ThemeMode.Light;
            navyTile.Selected = Result.Mode == ThemeMode.Navy;
            customTile.Selected = Result.Mode == ThemeMode.Custom;
            darkTile.Invalidate();
            lightTile.Invalidate();
            navyTile.Invalidate();
            customTile.Invalidate();
        }

        /// <summary>Applies Result's palette to this sheet and repaints its own controls.</summary>
        void ApplyOwnPalette()
        {
            T.Apply(Result);
            BackColor = T.Card;
            foreach (Control c in Controls) c.BackColor = T.Card;
            idField.ApplyTheme();
            keyField.ApplyTheme();
            SyncTiles();
            Invalidate(true);
            foreach (Control c in Controls) c.Invalidate();
        }

        void Preview()
        {
            if (PreviewSettings != null) PreviewSettings(Result);
        }

        /// <summary>Opens the beta colour designer; the app previews every change live.</summary>
        void OpenDesigner()
        {
            AppSettings before = Result.Clone();
            using (ThemeDesignerForm d = new ThemeDesignerForm(Result.SurfaceColor, Result.AccentColor, Result.TextColor, Result.Presets))
            {
                d.Preview = delegate(Color surface, Color accent, Color ink)
                {
                    Result.CustomSurface = AppSettings.ToHex(surface);
                    Result.CustomAccent = AppSettings.ToHex(accent);
                    Result.CustomText = AppSettings.ToHex(ink);
                    Result.CustomTheme = true;
                    Result.LightTheme = false;
                    Result.NavyTheme = false;
                    ApplyOwnPalette();
                    Preview();
                };
                d.ShowDialog(this);
                if (!d.Saved) Result = before;
                else
                {
                    Result.Presets = new List<ThemePreset>();
                    foreach (ThemePreset p in d.Presets) Result.Presets.Add(p.Clone());
                }
            }
            ApplyOwnPalette();
            Preview();
        }

        void ApplyLanguage()
        {
            idField.SetPlaceholder(DiscordRpc.HasBuiltInAppId
                ? L.T("Built-in app — leave empty", "Eingebaute App – leer lassen")
                : L.T("Paste your application ID", "Application-ID einfügen"));
            keyField.SetPlaceholder("logo");
            enableBtn.Label = Result.DiscordEnabled
                ? L.T("Rich Presence: On", "Rich Presence: An")
                : L.T("Rich Presence: Off", "Rich Presence: Aus");
            enableBtn.Primary = Result.DiscordEnabled;
            enableBtn.Invalidate();
            portalBtn.Label = L.T("Developer Portal", "Developer Portal");
            cancelBtn.Label = L.T("Cancel", "Abbrechen");
            saveBtn.Label = L.T("Save", "Speichern");
            SyncTiles();
            LayoutAll();
            Invalidate(true);
        }

        void LayoutAll()
        {
            if (idField == null) return;   // OnResize fires while the ctor is still setting sizes
            int pad = T.P(22);
            int w = ClientSize.Width - pad * 2;

            int th = T.P(104), tg = T.P(6);
            int tw = (w - tg * 3) / 4;
            darkTile.SetBounds(pad, T.P(92), tw, th);
            lightTile.SetBounds(pad + tw + tg, T.P(92), tw, th);
            navyTile.SetBounds(pad + (tw + tg) * 2, T.P(92), tw, th);
            customTile.SetBounds(pad + (tw + tg) * 3, T.P(92), tw, th);

            idField.SetBounds(pad, T.P(420), w, T.P(40));
            int fw = (int)(w * 0.44f);
            keyField.SetBounds(pad, T.P(492), fw, T.P(40));
            enableBtn.SetBounds(pad + fw + T.P(14), T.P(492), w - fw - T.P(14), T.P(40));

            int bh = T.P(38);
            int by = ClientSize.Height - pad - bh;
            saveBtn.SetBounds(ClientSize.Width - pad - T.P(124), by, T.P(124), bh);
            cancelBtn.SetBounds(saveBtn.Left - T.P(10) - T.P(112), by, T.P(112), bh);
            portalBtn.SetBounds(pad, by, T.P(160), bh);

            int cw = T.P(44);
            closeBtn.SetBounds(ClientSize.Width - cw, 0, cw, T.P(38));
        }

        void Save()
        {
            string id = idField.Value.Trim();
            string key = keyField.Value.Trim();
            if (key.Length == 0) key = "logo";
            if (Result.DiscordEnabled && DiscordRpc.ResolveAppId(id).Length == 0)
            {
                error = L.T("Enter a valid Discord Application ID — numbers only, 17–20 digits.",
                            "Gib eine gültige Discord-Application-ID ein – nur Zahlen, 17–20 Stellen.");
                Invalidate();
                return;
            }
            Result.DiscordAppId = id;
            Result.DiscordImageKey = key;
            Saved = true;
            Close();
        }

        /// <summary>Art Assets page for the entered application ID, or the application list without one.</summary>
        internal static string PortalUrl(string id)
        {
            string clean = DiscordRpc.ResolveAppId(id);
            return DiscordRpc.IsValidAppId(clean)
                ? "https://discord.com/developers/applications/" + clean + "/rich-presence/assets"
                : "https://discord.com/developers/applications";
        }

        void OpenPortal()
        {
            try { Process.Start(PortalUrl(idField == null ? null : idField.Value)); }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.PaintSheet(g, ClientRectangle, BackColor);

            int pad = T.P(22);
            int w = ClientSize.Width - pad * 2;
            int tile = T.P(30);
            T.LogoTile(g, new RectangleF(pad, T.P(15), tile, tile), T.F(7f));
            T.Str(g, L.T("Settings", "Einstellungen"), T.FPageTitle, T.Text,
                new Rectangle(pad + tile + T.P(12), T.P(11), T.P(360), T.P(28)), T.FL);
            T.Str(g, L.T("Appearance, performance and Discord Rich Presence", "Darstellung, Leistung und Discord Rich Presence"), T.FSmall, T.Muted,
                new Rectangle(pad + tile + T.P(12), T.P(37), T.P(400), T.P(18)), T.FL);
            // measured once and right-aligned against the close button, so the leading "v"
            // is never clipped by a narrow box (it used to look like a stray glyph)
            T.Str(g, "v" + AppInfo.Version, T.FMicro, T.Dim,
                new Rectangle(ClientSize.Width - T.P(56) - T.P(160), T.P(18), T.P(156), T.P(18)), T.FR);
            T.HLine(g, 0, ClientSize.Width, T.P(62), T.LineSoft);

            T.Str(g, L.T("APPEARANCE", "DARSTELLUNG"), T.FLabel, T.Dim,
                new Rectangle(pad, T.P(72), T.P(300), T.P(16)), T.FL);
            T.Str(g,
                L.T("Remembered for the next start — the boot screen follows it.",
                    "Wird gespeichert – auch der Startbildschirm folgt ihr."),
                T.FMicro, T.Dim, new Rectangle(pad, T.P(72), w, T.P(16)), T.FR);
            T.HLine(g, 0, ClientSize.Width, T.P(206), T.LineSoft);

            // performance mode, marked beta like every other experimental switch
            string perf = L.T("PERFORMANCE", "LEISTUNG");
            T.Str(g, perf, T.FLabel, T.Dim, new Rectangle(pad, T.P(216), T.P(300), T.P(16)), T.FL);
            int tagW = T.Meas("BETA", T.FLabel).Width + T.P(12);
            RectangleF tag = new RectangleF(pad + T.Meas(perf, T.FLabel).Width + T.P(10), T.P(213), tagW, T.P(18));
            T.StrokeRound(g, tag, T.F(9f), T.Gold, 1f);
            T.Str(g, "BETA", T.FLabel, T.Gold, Rectangle.Round(tag), T.FC);

            Rectangle row = PerfRow();
            RectangleF perfRow = new RectangleF(row.X, row.Y, row.Width, row.Height);
            Color rowFill = perfHot ? T.HoverSoft : T.Inset;
            T.FillRound(g, perfRow, T.F(10f), rowFill);
            T.StrokeRound(g,
                new RectangleF(perfRow.X + 0.5f, perfRow.Y + 0.5f, perfRow.Width - 1f, perfRow.Height - 1f),
                T.F(10f), Result.PerfMode ? T.Gold28 : (perfHot ? T.StrokeHot : T.Stroke), 1f);
            T.Str(g, L.T("Performance mode", "Leistungsmodus"), T.FBtn, T.Text,
                new Rectangle(pad + T.P(14), T.P(242), w - T.P(84), T.P(20)), T.FL);
            T.Wrap(g,
                L.T("Caches the finished canvas and repaints only the cells a stroke crosses: roughly ten times less painting per mouse move.",
                    "Hält die fertige Leinwand im Cache und zeichnet nur die Zellen eines Strichs neu: rund zehnmal weniger Zeichenarbeit pro Bewegung."),
                T.FSmall, T.Muted, new Rectangle(pad + T.P(14), T.P(262), w - T.P(84), T.P(30)));

            // the switch is painted with the sheet, not as a child control, so the row keeps its
            // rounded end and the theme backdrop stays continuous behind it
            Rectangle sw = PerfSwitchRect();
            float swRad = sw.Height / 2f;
            RectangleF track = new RectangleF(sw.X + 0.5f, sw.Y + 0.5f, sw.Width - 1f, sw.Height - 1f);
            Color trackFill = Result.PerfMode ? T.Gold : (perfHot ? T.Hover : T.Stroke);
            T.FillRound(g, track, swRad, trackFill);
            T.StrokeRound(g, track, swRad, Result.PerfMode ? T.GoldHi : T.StrokeHot, 1f);
            float knob = sw.Height - T.P(8);
            float kx = Result.PerfMode ? sw.Right - T.P(4) - knob : sw.X + T.P(4);
            using (SolidBrush kb = new SolidBrush(Result.PerfMode ? T.Bg2 : T.Muted))
                g.FillEllipse(kb, kx, sw.Y + T.P(4), knob, knob);
            T.HLine(g, 0, ClientSize.Width, T.P(308), T.LineSoft);

            T.Str(g, L.T("DISCORD RICH PRESENCE", "DISCORD RICH PRESENCE"), T.FLabel, T.Dim,
                new Rectangle(pad, T.P(318), T.P(320), T.P(16)), T.FL);

            string help = DiscordRpc.HasBuiltInAppId
                ? L.T(
                    "Rich Presence uses the app's own Discord application, so it works out of the box: switch it on and keep the Discord desktop client running. The field below is optional — paste your own application ID only if you want your own app name and artwork.",
                    "Rich Presence nutzt die eigene Discord-Anwendung der App und läuft damit ohne Einrichtung: einschalten und den Discord-Desktop-Client geöffnet lassen. Das Feld unten ist optional – füge nur dann eine eigene Application-ID ein, wenn du eigenen Namen und eigene Grafik möchtest.")
                : L.T(
                    "This build has no Discord application of its own yet. Until the publisher adds one, create an application in the Developer Portal, upload assets/logo.png under Rich Presence → Art Assets with the image key below, then paste its 17–20 digit ID here.",
                    "Dieser Build bringt noch keine eigene Discord-Anwendung mit. Bis der Herausgeber eine hinzufügt: erstelle eine im Developer Portal, lade assets/logo.png unter Rich Presence → Art Assets mit dem Bild-Key unten hoch und füge hier die 17–20-stellige ID ein.");
            T.Wrap(g, help, T.FSmall, T.Muted, new Rectangle(pad, T.P(336), ClientSize.Width - pad * 2, T.P(56)));

            string idLabel = DiscordRpc.HasBuiltInAppId
                ? L.T("CUSTOM APPLICATION ID (OPTIONAL)", "EIGENE APPLICATION-ID (OPTIONAL)")
                : L.T("APPLICATION ID", "APPLICATION-ID");
            T.Str(g, idLabel, T.FLabel, T.Dim,
                new Rectangle(pad, T.P(400), T.P(360), T.P(16)), T.FL);
            T.Str(g, L.T("LARGE IMAGE KEY", "BILD-KEY (GROSS)"), T.FLabel, T.Dim,
                new Rectangle(pad, T.P(472), T.P(300), T.P(16)), T.FL);

            int dotX = pad;
            int dotY = T.P(548);
            Color dot = DiscordRpc.Status == "connected" ? T.Teal
                       : DiscordRpc.Status == "connecting" ? T.Gold
                       : DiscordRpc.Status == "error" ? T.Danger
                       : Result.DiscordEnabled ? T.Gold
                       : T.DotOff;
            using (SolidBrush b = new SolidBrush(dot)) g.FillEllipse(b, dotX, dotY, T.F(7), T.F(7));
            T.Str(g, StatusText(), T.FSmall, T.Muted,
                new Rectangle(dotX + T.P(14), T.P(540), ClientSize.Width - pad * 2 - T.P(14), T.P(20)), T.FL);

            if (error.Length > 0)
            {
                T.Str(g, error, T.FSmall, T.DangerText,
                    new Rectangle(pad, T.P(568), ClientSize.Width - pad * 2, T.P(18)), T.FL);
            }

            T.HLine(g, 0, ClientSize.Width, ClientSize.Height - T.P(62), T.Line);

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Stroke, 1f))
                g.DrawPath(pen, rim);
        }

        string StatusText()
        {
            if (Result.DiscordEnabled && !DiscordRpc.HasBuiltInAppId &&
                DiscordRpc.ResolveAppId(idField == null ? null : idField.Value).Length == 0)
                return L.T("No application ID in this build — paste your own, or ask the publisher to add one.",
                           "Keine Application-ID in diesem Build – eigene einfügen oder den Herausgeber fragen.");
            if (Result.DiscordEnabled && DiscordRpc.Status == "off")
                return L.T("Saved — Save closes this sheet and connects to Discord.",
                           "Bereit – Speichern schließt das Fenster und verbindet mit Discord.");
            switch (DiscordRpc.Status)
            {
                case "connected":
                    return L.T("Connected — your profile shows the activity now.", "Verbunden – dein Profil zeigt die Aktivität jetzt.");
                case "connecting":
                    return L.T("Connecting to Discord…", "Verbinde mit Discord…");
                case "no-discord":
                    return L.T("Discord is not running — presence starts automatically once it is.",
                               "Discord läuft nicht – die Präsenz startet automatisch, sobald es offen ist.");
                case "error":
                    return L.T("Discord refused the connection: ", "Discord hat die Verbindung abgelehnt: ") + DiscordRpc.StatusDetail;
                default:
                    return L.T("Rich Presence is off.", "Rich Presence ist aus.");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Escape) { Close(); return true; }
            if ((keyData & Keys.KeyCode) == Keys.Enter) { Save(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyWindowShape();
            LayoutAll();
        }

        void ApplyWindowShape()
        {
            if (WindowState == FormWindowState.Maximized) { Region = null; return; }
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyWindowShape();
            idField.Box.Focus();
            idField.Box.SelectionStart = idField.Box.TextLength;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            statusTimer.Stop();
            statusTimer.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>The performance row, which switches mode on a click anywhere inside it.</summary>
        Rectangle PerfRow()
        {
            int pad = T.P(22);
            return new Rectangle(pad, T.P(234), ClientSize.Width - pad * 2, T.P(62));
        }

        Rectangle PerfSwitchRect()
        {
            Rectangle row = PerfRow();
            int w = T.P(40), h = T.P(22);
            return new Rectangle(row.Right - T.P(14) - w, row.Y + (row.Height - h) / 2, w, h);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hot = PerfRow().Contains(e.Location);
            if (hot != perfHot)
            {
                perfHot = hot;
                Cursor = hot ? Cursors.Hand : Cursors.Default;
                Invalidate(PerfRow());
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!perfHot && !perfDown) return;
            perfHot = false;
            perfDown = false;
            Cursor = Cursors.Default;
            Invalidate(PerfRow());
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!perfDown) return;
            perfDown = false;
            if (PerfRow().Contains(e.Location))
                Result.PerfMode = !Result.PerfMode;
            Invalidate(PerfRow());
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && PerfRow().Contains(e.Location))
            {
                perfDown = true;
                perfHot = true;
                Invalidate(PerfRow());
                return;
            }

            base.OnMouseDown(e);
            if (e.Y < T.P(44))
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
