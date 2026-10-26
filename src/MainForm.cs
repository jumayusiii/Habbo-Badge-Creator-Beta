using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    internal class MainForm : Form
    {
        // ------------------------------------------------------------- state
        readonly PixelDoc doc = new PixelDoc();

        AppSettings settings = new AppSettings();
        LangSwitch langSwitch;
        LinkChip changelogChip, settingsChip;

        CanvasView canvas;
        FieldBox nameField, descField;
        GalleryView gallery;
        PreviewView previewBig;
        FlatBtn btnSave, btnExport, btnClear, btnImport, btnOpenFolder;
        IconBtn tPencil, tEraser, tFill, tPicker, tMirror, tFlipH, tFlipV, tRotate, tGrid, tCentre, tLayers, tUndo, tRedo, tZoomIn, tZoomOut;
        IconBtn btnCopyBadge;
        LayersPanel layersPanel;
        BetaTag betaTag;
        SwatchBtn colorChip;
        PlusSwatch plusSwatch;
        readonly List<SwatchBtn> swatches = new List<SwatchBtn>();
        Rectangle rGroupTools, rGroupMirror, rGroupHistory, rGroupZoom, rGroupGrid;
        WinBtn wbMin, wbMax, wbClose;
        TipView tipView;
        IconBtn tipAnchor;

        Color drawColor = Color.FromArgb(0xF5, 0xA6, 0x23);
        int tool = 0;
        bool autoZoom = true;
        int lastDraftVersion = -1;
        readonly Timer draftTimer = new Timer();
        readonly Timer toastTimer = new Timer();
        readonly Timer strokeTimer = new Timer();   // perf mode: batches the big preview while painting
        bool previewStale;
        string toastText = "";

        // ------------------------------------------------------------- layout
        Rectangle rTb, rCardL, rCardR1, rCardR2, rFooter;
        Rectangle rCanvasArea;
        Rectangle lastCardL, lastCardR1, lastCardR2;
        bool compact, galleryCollapsed, lastCollapsed;

        static readonly int[] Palette = new int[]
        {
            0xFFFFFF, 0xDCE4EE, 0x9AA9BA, 0x56677A, 0x2C3746, 0x0A0E14,
            0xFFEFB8, 0xFFD65C, 0xF5A623, 0xD9772A, 0x9E3B1E, 0x5E2312,
            0xFFB8C6, 0xF2566B, 0xC31E45, 0x7A1230,
            0xD9C7FF, 0x9C7CF4, 0x5B3FBF, 0x3A2275,
            0xA9F0E4, 0x4FD1C5, 0x1E7F79, 0x14615D,
            0xC8F08A, 0x6FBF46, 0x3C8A34, 0x1E5B2C
        };

        public MainForm()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            Text = "Habbo Badge Creator Beta " + AppInfo.Version + " — build " + AppInfo.Build;
            BackColor = T.Bg2;
            DoubleBuffered = true;
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            MinimumSize = new Size(
                Math.Min(T.P(1040), (int)(wa.Width * 0.95f)),
                Math.Min(T.P(640), (int)(wa.Height * 0.95f)));

            int w = Math.Min(T.P(1300), (int)(wa.Width * 0.92f));
            int h = Math.Min(T.P(830), (int)(wa.Height * 0.90f));
            ClientSize = new Size(w, h);
            MaximizedBounds = wa;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            Store.Ensure();

            draftTimer.Interval = 4000;
            draftTimer.Tick += delegate { AutoSaveDraft(); };
            draftTimer.Start();

            toastTimer.Interval = 2600;
            toastTimer.Tick += delegate { toastTimer.Stop(); toastText = ""; Invalidate(); };

            strokeTimer.Interval = 70;
            strokeTimer.Tick += delegate
            {
                if (!previewStale || previewBig == null) { strokeTimer.Stop(); return; }
                previewStale = false;
                previewBig.Invalidate();
                if (!doc.InStroke) strokeTimer.Stop();
            };
        }

        void BuildUi()
        {
            canvas = new CanvasView();
            canvas.Doc = doc;
            canvas.UserZoomed += delegate { autoZoom = false; UpdateToolUi(); };
            canvas.LockedAttempt += delegate
            {
                ShowToast(L.T("That layer is locked — unlock it in the layers panel",
                    "Diese Ebene ist gesperrt – im Ebenen-Bereich entsperren"));
            };
            canvas.Picked += delegate (Color c)
            {
                if (c.A == 0) { ShowToast(L.T("That pixel is transparent", "Dieses Pixel ist transparent")); return; }
                SetColor(Color.FromArgb(255, c.R, c.G, c.B));
                SetTool(0);
            };
            Controls.Add(canvas);

            previewBig = new PreviewView();
            previewBig.Doc = doc;
            Controls.Add(previewBig);

            layersPanel = new LayersPanel();
            layersPanel.BackColor = T.Card;
            layersPanel.Doc = doc;
            layersPanel.Changed += delegate
            {
                RefreshDoc();
                Invalidate(new Rectangle(rCardL.X, rCardL.Y, rCardL.Width, rCardL.Height));
            };
            layersPanel.HintShown += delegate (Rectangle anchor, string title, string key)
            {
                if (tipView == null) return;
                tipView.ShowAt(new Rectangle(layersPanel.Left + anchor.X, layersPanel.Top + anchor.Y,
                    anchor.Width, anchor.Height), title, key);
            };
            layersPanel.HintHidden += delegate { HideTip(); };
            layersPanel.Visible = false;
            Controls.Add(layersPanel);


            nameField = new FieldBox(false);
            nameField.SetPlaceholder(L.T("Badge name", "Name des Badges"));
            nameField.ValueChanged += delegate { UpdateDiscordActivity(); };
            Controls.Add(nameField);

            descField = new FieldBox(true);
            descField.SetPlaceholder(L.T("What is this badge about?", "Wofür steht das Badge?"));
            Controls.Add(descField);

            gallery = new GalleryView();
            gallery.LoadRequested += LoadBadge;
            gallery.DeleteRequested += DeleteBadge;
            Controls.Add(gallery);

            colorChip = new SwatchBtn();
            colorChip.Round = true;
            colorChip.Value = drawColor;
            colorChip.Click += delegate { PickCustomColor(); };
            Controls.Add(colorChip);

            plusSwatch = new PlusSwatch();
            plusSwatch.Click += delegate { PickCustomColor(); };
            Controls.Add(plusSwatch);

            foreach (int rgb in Palette)
            {
                int v = rgb;
                SwatchBtn s = new SwatchBtn();
                s.Value = Color.FromArgb(255, (v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
                s.Click += delegate { SetColor(s.Value); };
                swatches.Add(s);
                Controls.Add(s);
            }

            tPencil = MakeTool("pencil", "Pencil");
            tEraser = MakeTool("eraser", "Eraser");
            tFill = MakeTool("bucket", "Fill");
            tPicker = MakeTool("picker", "Colour picker");
            tMirror = MakeToggle("mirror", "Mirror");
            tFlipH = MakeTool("flipH", "Flip horizontally");
            tFlipV = MakeTool("flipV", "Flip vertically");
            tRotate = MakeTool("rotate", "Rotate 90°");
            tGrid = MakeToggle("grid", "Pixel grid");
            tCentre = MakeToggle("crosshair", "Guides");
            tLayers = MakeToggle("layers", "Layers");
            tUndo = MakeTool("undo", "Undo");
            tRedo = MakeTool("redo", "Redo");
            tZoomOut = MakeTool("minus", "Zoom out");
            tZoomIn = MakeTool("plus", "Zoom in");

            ApplyTooltips();

            tPencil.Click += delegate { SetTool(0); };
            tEraser.Click += delegate { SetTool(1); };
            tFill.Click += delegate { SetTool(2); };
            tPicker.Click += delegate { SetTool(3); };
            tMirror.Click += delegate { ToggleMirror(); };
            tFlipH.Click += delegate { FlipCanvas(true); };
            tFlipV.Click += delegate { FlipCanvas(false); };
            tRotate.Click += delegate { RotateCanvas(true); };
            tGrid.Click += delegate
            {
                tGrid.Active = !tGrid.Active;
                canvas.ShowGrid = tGrid.Active;
                tGrid.Invalidate();
                canvas.Invalidate();
            };
            tCentre.Click += delegate { CycleGuides(); };
            tLayers.Click += delegate { ToggleLayers(); };
            tUndo.Click += delegate { doc.Undo(); RefreshDoc(); };
            tRedo.Click += delegate { doc.Redo(); RefreshDoc(); };
            tZoomIn.Click += delegate { autoZoom = false; canvas.Zoom = canvas.Zoom + 1; UpdateToolUi(); };
            tZoomOut.Click += delegate { autoZoom = false; canvas.Zoom = canvas.Zoom - 1; UpdateToolUi(); };            btnImport = MakeBtn(L.T("Import image", "Bild importieren"), "image", false);
            btnImport.Click += delegate { OpenImportDialog(); };
            Controls.Add(btnImport);

            btnClear = MakeBtn(L.T("Clear", "Leeren"), null, false);
            btnClear.Click += delegate
            {
                if (doc.ActiveLocked)
                {
                    ShowToast(L.T("That layer is locked — unlock it in the layers panel",
                        "Diese Ebene ist gesperrt – im Ebenen-Bereich entsperren"));
                    return;
                }
                doc.Clear();
                RefreshDoc();
                ShowToast(L.T("Canvas cleared — Ctrl+Z to undo", "Zeichenfläche geleert – Strg+Z macht es rückgängig"));
            };
            Controls.Add(btnClear);

            btnSave = MakeBtn(L.T("Save badge", "Badge speichern"), "check", true);
            btnSave.Click += delegate { SaveBadge(); };
            Controls.Add(btnSave);

            btnExport = MakeBtn(L.T("Export PNG", "PNG exportieren"), "download", false);
            btnExport.Click += delegate { ExportPng(); };
            Controls.Add(btnExport);

            // beta: one click to put the 40 x 40 badge on the clipboard
            btnCopyBadge = MakeTool("copy", "Copy badge");
            btnCopyBadge.Click += delegate { CopyBadge(); };
            SetTip(btnCopyBadge, L.T("Copy badge", "Badge kopieren"), "Ctrl+Shift+C");

            btnOpenFolder = MakeBtn(L.T("Open folder", "Ordner öffnen"), "folder", false);
            btnOpenFolder.Click += delegate { Store.OpenFolder(Store.GalleryDir); };
            Controls.Add(btnOpenFolder);

            wbMin = new WinBtn();
            wbMin.Icon = "min";
            wbMin.Tag = "bar";
            wbMin.Click += delegate { WindowState = FormWindowState.Minimized; };
            Controls.Add(wbMin);

            wbMax = new WinBtn();
            wbMax.Icon = "max";
            wbMax.Tag = "bar";
            wbMax.Click += delegate { ToggleMaximize(); };
            Controls.Add(wbMax);

            IconBtn[] segmented = new IconBtn[] { tPencil, tEraser, tFill, tPicker, tMirror, tFlipH, tFlipV, tRotate, tUndo, tRedo, tZoomIn, tZoomOut, tGrid, tCentre, tLayers };
            foreach (IconBtn sb in segmented)
            {
                sb.Segmented = true;
                sb.Tag = "segment";
                sb.BackColor = T.Segment;
            }

            canvas.HoverChanged += delegate
            {
                Invalidate(new Rectangle(rCanvasArea.X, rCanvasArea.Bottom, rCardL.Width - T.P(36), T.P(30)));
            };

            tipView = new TipView();
            Controls.Add(tipView);
            tipView.CreateControl();

            // controls painted straight onto the app bar keep the bar tone as their backdrop
            betaTag = new BetaTag();
            betaTag.Tag = "bar";
            betaTag.MouseEnter += delegate { ShowBetaTip(); };
            betaTag.MouseLeave += delegate { HideTip(); };
            betaTag.Click += delegate { OpenChangelog(); };
            Controls.Add(betaTag);

            langSwitch = new LangSwitch();
            langSwitch.Tag = "bar";
            langSwitch.Changed += delegate { SetLanguage(langSwitch.German); };
            Controls.Add(langSwitch);

            changelogChip = new LinkChip();
            changelogChip.Tag = "bar";
            changelogChip.Label = L.T("Changelog", "Änderungen");
            changelogChip.Click += delegate { OpenChangelog(); };
            Controls.Add(changelogChip);

            settingsChip = new LinkChip();
            settingsChip.Tag = "bar";
            settingsChip.Icon = "gear";
            settingsChip.Label = L.T("Settings", "Einstellungen");
            settingsChip.Click += delegate { OpenSettings(); };
            settingsChip.MouseEnter += delegate { ShowSettingsTip(); };
            settingsChip.MouseLeave += delegate { HideTip(); };
            Controls.Add(settingsChip);

            wbClose = new WinBtn();
            wbClose.Icon = "close";
            wbClose.Tag = "bar";
            wbClose.Click += delegate { Close(); };
            AllowDrop = true;
            Controls.Add(wbClose);

            tipView.BringToFront();

            doc.Changed += RefreshDoc;
            SetTool(0);
            SetColor(drawColor);
            tMirror.Active = false;
            tGrid.Active = true;
            tCentre.Active = false;
            tLayers.Active = settings.LayersBeta;

            ReloadGallery();
            LayoutAll();
        }

        FlatBtn MakeBtn(string label, string icon, bool primary)
        {
            FlatBtn b = new FlatBtn();
            b.Label = label;
            b.Icon = icon;
            b.Primary = primary;
            return b;
        }

        IconBtn MakeTool(string icon, string tip)
        {
            IconBtn b = new IconBtn();
            b.Icon = icon;
            b.Click += delegate { };
            b.MouseEnter += delegate { QueueTip(b); };
            b.MouseLeave += delegate { if (tipAnchor == b) HideTip(); };
            Controls.Add(b);
            return b;
        }

        IconBtn MakeToggle(string icon, string tip)
        {
            IconBtn b = new IconBtn();
            b.Icon = icon;
            b.Toggle = true;
            b.Click += delegate { };
            b.MouseEnter += delegate { QueueTip(b); };
            b.MouseLeave += delegate { if (tipAnchor == b) HideTip(); };
            Controls.Add(b);
            return b;
        }

        void QueueTip(IconBtn b)
        {
            tipAnchor = b;
            if (tipView != null) tipView.ShowFor(b);
        }

        /// <summary>Explains the connection dot on the Settings chip in words.</summary>
        void ShowSettingsTip()
        {
            if (tipView == null || settingsChip == null) return;
            tipView.ShowFor(settingsChip,
                L.T("Discord Rich Presence", "Discord Rich Presence"), DiscordStateText());
        }

        string DiscordStateText()
        {
            if (!settings.DiscordEnabled) return L.T("Off", "Aus");
            if (DiscordRpc.ResolveAppId(settings.DiscordAppId).Length == 0)
                return L.T("No application ID in this build", "Keine Application-ID in diesem Build");
            switch (DiscordRpc.Status)
            {
                case "connected": return L.T("Connected", "Verbunden");
                case "connecting": return L.T("Connecting…", "Verbinde…");
                case "no-discord": return L.T("Discord is not running", "Discord läuft nicht");
                case "error": return L.T("Connection refused", "Verbindung abgelehnt");
                default: return L.T("Waiting for Discord", "Warte auf Discord");
            }
        }

        void HideTip()
        {
            tipAnchor = null;
            if (tipView != null) tipView.Dismiss();
        }

        // ------------------------------------------------------------- layout
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (tipView != null) HideTip();
            LayoutAll();
            Invalidate();
        }

        void LayoutAll()
        {
            if (canvas == null) return;
            int w = ClientSize.Width, h = ClientSize.Height;
            int abH = T.P(64), pad = T.P(16), gap = T.P(12);
            compact = h < T.P(700);
            galleryCollapsed = gallery.Items.Count == 0;
            int top = abH + (compact ? T.P(10) : T.P(14));
            int footerH = T.P(36);
            int rowH = Math.Max(T.P(320), h - top - footerH - T.P(10));
            int leftW = (int)Math.Round((w - pad * 2 - gap) * 0.575f);
            int rightW = w - pad * 2 - gap - leftW;
            int dInner = T.P(14), dHead = T.P(44);
            int basePrev = compact ? T.P(88) : T.P(120);
            int baseBody = Math.Max(basePrev + T.P(18), T.P(130));
            int detailsH = galleryCollapsed
                ? Math.Max(T.P(252), rowH - gap - dHead)
                : dHead + dInner + baseBody + T.P(12) + T.P(38) + dInner;

            rTb = new Rectangle(0, 0, w, abH);
            rCardL = new Rectangle(pad, top, leftW, rowH);
            rCardR1 = new Rectangle(pad + leftW + gap, top, rightW, detailsH);
            rCardR2 = new Rectangle(rCardR1.X, rCardR1.Bottom + gap, rightW,
                galleryCollapsed ? dHead : Math.Max(T.P(110), rowH - detailsH - gap));
            rFooter = new Rectangle(0, h - footerH, w, footerH);

            // app bar: caption buttons, then language switch and Discord chip
            int capW = T.P(42), capH = T.P(30), capPad = T.P(10);
            int capY = (abH - capH) / 2;
            wbClose.SetBounds(w - capPad - capW, capY, capW, capH);
            wbMax.SetBounds(wbClose.Left - capW, capY, capW, capH);
            wbMin.SetBounds(wbMax.Left - capW, capY, capW, capH);

            int chipH = T.P(28), chipY = (abH - chipH) / 2, langW = T.P(74);
            langSwitch.SetBounds(wbMin.Left - T.P(14) - langW, chipY, langW, chipH);
            int setW = T.Meas(settingsChip.Label, T.FChip).Width + T.P(40) +
                       (settingsChip.Status > 0 ? T.P(15) : 0);
            settingsChip.SetBounds(langSwitch.Left - T.P(8) - setW, chipY, setW, chipH);
            int chW = T.Meas(changelogChip.Label, T.FChip).Width + T.P(40);
            changelogChip.SetBounds(settingsChip.Left - T.P(8) - chW, chipY, chW, chipH);

            int wmW = T.Wordmark != null ? (int)Math.Round(T.Wordmark.Width * T.S) : 0;
            int betaW = T.Meas("BETA", T.FLabel).Width + T.P(16);
            betaTag.SetBounds(T.P(24) + wmW + T.P(10), (abH - T.P(24)) / 2, betaW, T.P(24));

            // ---- left card: editor
            int cx = rCardL.X + T.P(16), innerW = rCardL.Width - T.P(32);
            int headH = T.P(44), btnH = T.P(28);
            int cw2 = BtnWidth(btnClear, T.P(70));
            btnClear.SetBounds(rCardL.Right - T.P(16) - cw2, rCardL.Y + (headH - btnH) / 2, cw2, btnH);
            int iw2 = BtnWidth(btnImport, T.P(104));
            btnImport.SetBounds(btnClear.Left - T.P(8) - iw2, btnClear.Top, iw2, btnH);

            int bodyY = rCardL.Y + headH + T.P(10);
            int gh = T.P(36), b = T.P(28), inset = T.P(4), bgap = T.P(2), ggap = T.P(6);
            int x = cx;
            rGroupTools = new Rectangle(x, bodyY, inset * 2 + b * 4 + bgap * 3, gh);
            int bx = x + inset;
            tPencil.SetBounds(bx, bodyY + inset, b, b); bx += b + bgap;
            tEraser.SetBounds(bx, bodyY + inset, b, b); bx += b + bgap;
            tFill.SetBounds(bx, bodyY + inset, b, b); bx += b + bgap;
            tPicker.SetBounds(bx, bodyY + inset, b, b);
            x = rGroupTools.Right + ggap;

            rGroupMirror = new Rectangle(x, bodyY, inset * 2 + b * 4 + bgap * 3, gh);
            tMirror.SetBounds(x + inset, bodyY + inset, b, b);
            tFlipH.SetBounds(tMirror.Right + bgap, bodyY + inset, b, b);
            tFlipV.SetBounds(tFlipH.Right + bgap, bodyY + inset, b, b);
            tRotate.SetBounds(tFlipV.Right + bgap, bodyY + inset, b, b);
            x = rGroupMirror.Right + ggap;

            rGroupHistory = new Rectangle(x, bodyY, inset * 2 + b * 2 + bgap, gh);
            bx = x + inset;
            tUndo.SetBounds(bx, bodyY + inset, b, b); bx += b + bgap;
            tRedo.SetBounds(bx, bodyY + inset, b, b);

            int rx = cx + innerW;
            rGroupGrid = new Rectangle(rx - (inset * 2 + b * 3 + bgap * 2), bodyY, inset * 2 + b * 3 + bgap * 2, gh);
            tGrid.SetBounds(rGroupGrid.X + inset, bodyY + inset, b, b);
            tCentre.SetBounds(tGrid.Right + bgap, bodyY + inset, b, b);
            tLayers.SetBounds(tCentre.Right + bgap, bodyY + inset, b, b);
            int zw = T.P(38);
            rGroupZoom = new Rectangle(rGroupGrid.Left - ggap - (inset * 2 + b * 2 + zw), bodyY, inset * 2 + b * 2 + zw, gh);
            tZoomOut.SetBounds(rGroupZoom.X + inset, bodyY + inset, b, b);
            tZoomIn.SetBounds(rGroupZoom.Right - inset - b, bodyY + inset, b, b);

            // ---- palette
            int sw = T.P(22), sg = T.P(6);
            int chipW = T.P(30);
            int paletteY = bodyY + gh + T.P(10);
            int avail = innerW - chipW - T.P(12);
            int items = swatches.Count + 1;
            int cols = Math.Max(6, (avail + sg) / (sw + sg));
            if (cols > items) cols = items;
            int rows = (items + cols - 1) / cols;
            if (rows > 1) cols = (items + rows - 1) / rows;
            int paletteH = rows * sw + (rows - 1) * sg;
            for (int i = 0; i < swatches.Count; i++)
            {
                int col = i % cols, row = i / cols;
                swatches[i].SetBounds(cx + chipW + T.P(12) + col * (sw + sg),
                    paletteY + row * (sw + sg), sw, sw);
            }
            int plusCol = swatches.Count % cols, plusRow = swatches.Count / cols;
            plusSwatch.SetBounds(cx + chipW + T.P(12) + plusCol * (sw + sg),
                paletteY + plusRow * (sw + sg), sw, sw);
            int chipTop = paletteY + (paletteH - chipW) / 2;
            colorChip.SetBounds(cx, chipTop, chipW, chipW);

            int canvasY = paletteY + paletteH + T.P(12);
            int canvasH = Math.Max(T.P(110), rCardL.Bottom - T.P(26) - canvasY);
            int railW = T.P(168);
            bool rail = settings.LayersBeta;
            int canvasAvail = rail ? Math.Max(T.P(120), innerW - railW - T.P(10)) : innerW;
            int side = Math.Min(canvasAvail, canvasH);
            rCanvasArea = new Rectangle(cx + (canvasAvail - side) / 2, canvasY + (canvasH - side) / 2, side, side);
            canvas.Bounds = rCanvasArea;
            layersPanel.Visible = rail;
            if (rail) layersPanel.SetBounds(rCardL.Right - T.P(16) - railW, canvasY, railW, canvasH);
            if (autoZoom)
            {
                int fit = Math.Min(rCanvasArea.Width, rCanvasArea.Height) - T.P(20);
                int z = fit / PixelDoc.N;
                canvas.Zoom = Math.Max(3, Math.Min(16, z));
            }

            // ---- right card 1: details (grows when there are no saved badges)
            int r1x = rCardR1.X + dInner, r1w = rCardR1.Width - dInner * 2;
            int cy = rCardR1.Y + dHead + dInner;
            int bodyH = rCardR1.Height - dHead - dInner * 2 - T.P(12) - T.P(38);
            int prev, descH;
            if (galleryCollapsed)
            {
                int maxPrev = Math.Max(T.P(96), r1w - T.P(220));
                prev = Math.Max(T.P(96), Math.Min(T.P(280), Math.Min(maxPrev, bodyH - T.P(18))));
                descH = Math.Min(T.P(100), Math.Max(T.P(46), bodyH - T.P(80)));
            }
            else
            {
                prev = basePrev;
                descH = T.P(50);
            }
            int blockH = Math.Max(prev + T.P(18), T.P(16) + T.P(38) + T.P(8) + T.P(16) + descH);
            int blockTop = cy + (galleryCollapsed ? Math.Max(0, (bodyH - blockH) / 2) : 0);
            previewBig.SetBounds(r1x, blockTop + T.P(18), prev, prev);

            int fx = r1x + prev + T.P(16);
            int fw = Math.Max(T.P(120), r1x + r1w - fx);
            nameField.SetBounds(fx, blockTop + T.P(18), fw, T.P(38));
            descField.SetBounds(fx, blockTop + T.P(80), fw, descH);

            int by = rCardR1.Bottom - dInner - T.P(38);
            int saveW = (int)(r1w * 0.50f);
            int copyW = T.P(42);
            btnSave.SetBounds(r1x, by, saveW, T.P(38));
            btnCopyBadge.SetBounds(r1x + r1w - copyW, by, copyW, T.P(38));
            btnExport.SetBounds(r1x + saveW + T.P(10), by,
                Math.Max(T.P(96), r1w - saveW - T.P(10) - copyW - T.P(10)), T.P(38));

            // ---- right card 2: gallery, or a one-line bar when it is empty
            int gx = rCardR2.X + T.P(16), gw = rCardR2.Width - T.P(32);
            int ow2 = BtnWidth(btnOpenFolder, T.P(100));
            btnOpenFolder.SetBounds(rCardR2.Right - T.P(16) - ow2, rCardR2.Y + (dHead - btnH) / 2, ow2, btnH);
            gallery.Visible = !galleryCollapsed;
            gallery.SetBounds(gx, rCardR2.Y + dHead, gw,
                galleryCollapsed ? T.P(4) : Math.Max(T.P(60), rCardR2.Height - dHead - T.P(8)));

            ApplyWindowShape();

            // Collapsing or growing the My badges card moves every control in the right
            // column. Repainting the whole surface keeps stale card borders, labels and
            // field outlines from being left behind where the old layout used to be.
            if (rCardL != lastCardL || rCardR1 != lastCardR1 || rCardR2 != lastCardR2 ||
                galleryCollapsed != lastCollapsed)
            {
                lastCardL = rCardL;
                lastCardR1 = rCardR1;
                lastCardR2 = rCardR2;
                lastCollapsed = galleryCollapsed;
                Invalidate(true);
            }

            UpdateToolUi();
        }

        void UpdateToolUi()
        {
            if (tPencil == null) return;
            tPencil.Active = (tool == 0);
            tEraser.Active = (tool == 1);
            tFill.Active = (tool == 2);
            tPicker.Active = (tool == 3);
            tPencil.Invalidate(); tEraser.Invalidate(); tFill.Invalidate(); tPicker.Invalidate();
            tUndo.Invalidate();
            tRedo.Invalidate();
            canvas.Invalidate();
            Invalidate(new Rectangle(rCardL.X, rCardL.Y, rCardL.Width, T.P(200)));
        }

        // ------------------------------------------------------------- painting
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            T.PaintBackdrop(g, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // flat app bar, separated from the canvas by a single hairline
            T.HLine(g, 0, Width, rTb.Bottom - 1, T.LineSoft);
            if (T.Wordmark != null)
            {
                int lw = (int)Math.Round(T.Wordmark.Width * T.S);
                int lh = (int)Math.Round(T.Wordmark.Height * T.S);
                int lx = T.P(24), ly = rTb.Y + (rTb.Height - lh) / 2;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(T.Wordmark, new Rectangle(lx, ly, lw, lh));
            }

            DrawCardShell(g, rCardL, "brush", L.T("Design your badge", "Gestalte dein Badge"));
            DrawCardShell(g, rCardR1, "sparkle", L.T("Badge details", "Badge-Details"));
            DrawCardShell(g, rCardR2, "folder", L.T("My badges", "Meine Badges"),
                galleryCollapsed ? L.T("No badges yet", "Noch keine Badges") : null);

            DrawEditorChrome(g);
            DrawDetailsChrome(g);
            DrawFooter(g);
            DrawToast(g);

            // hairline rim so the rounded window reads as a surface on any wallpaper
            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Rim, 1f))
                g.DrawPath(pen, rim);
        }

        void DrawCard(Graphics g, Rectangle r)
        {
            RectangleF rf = new RectangleF(r.X, r.Y, r.Width, r.Height);
            T.FillRound(g, rf, T.F(12f), T.Card);
            T.StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f),
                T.F(12f), T.Stroke, 1f);
        }

        void DrawCardShell(Graphics g, Rectangle r, string icon, string title, string hint = null)
        {
            DrawCard(g, r);
            int headH = T.P(44);
            float ic = T.F(15f);
            float ix = r.X + T.P(16);
            T.Icon(g, icon, new RectangleF(ix, r.Y + (headH - ic) / 2f, ic, ic), T.Gold);
            int tx = (int)Math.Round(ix + ic) + T.P(10);
            T.Str(g, title, T.FCardTitle, T.Text,
                new Rectangle(tx, r.Y, Math.Max(T.P(40), r.Width - (tx - r.X) - T.P(40)), headH), T.FL);
            if (!string.IsNullOrEmpty(hint))
            {
                int hx = tx + T.Meas(title, T.FCardTitle).Width + T.P(10);
                int hw = btnOpenFolder.Left - T.P(10) - hx;
                if (hw > T.P(24))
                    T.StrEll(g, hint, T.FSmall, T.Dim, new Rectangle(hx, r.Y, hw, headH), T.FL);
            }
        }

        void DrawSegment(Graphics g, Rectangle r)
        {
            T.FillRound(g, new RectangleF(r.X, r.Y, r.Width, r.Height), T.F(9f), T.Segment);
        }

        void DrawEditorChrome(Graphics g)
        {
            DrawSegment(g, rGroupTools);
            DrawSegment(g, rGroupMirror);
            DrawSegment(g, rGroupHistory);
            DrawSegment(g, rGroupZoom);
            DrawSegment(g, rGroupGrid);

            Rectangle zr = new Rectangle(tZoomOut.Right, rGroupZoom.Y, tZoomIn.Left - tZoomOut.Right, rGroupZoom.Height);
            T.Str(g, canvas.Zoom + "×", T.FBtn, T.Muted, zr, T.FC);

            int hx0 = rCardL.X + T.P(16);
            int hy = rCardL.Bottom - T.P(22);
            string cap = L.T("CANVAS", "ZEICHENFLÄCHE");
            int capW = T.Meas(cap, T.FLabel).Width + T.P(2);
            T.Str(g, cap, T.FLabel, T.Dim, new Rectangle(hx0, hy, capW + T.P(4), T.P(16)), T.FL);
            string co = canvas.HoverX >= 0
                ? "X " + canvas.HoverX + "   Y " + canvas.HoverY
                : "40 × 40 px";
            T.Str(g, co, T.FMicro, canvas.HoverX >= 0 ? T.Muted : T.Dim,
                new Rectangle(hx0 + capW + T.P(12), hy, T.P(120), T.P(16)), T.FL);
        }

        void DrawDetailsChrome(Graphics g)
        {
            T.Str(g, L.T("PREVIEW", "VORSCHAU"), T.FLabel, T.Dim,
                new Rectangle(previewBig.Left, previewBig.Top - T.P(18), T.P(200), T.P(16)), T.FL);

            T.Str(g, L.T("NAME", "NAME"), T.FLabel, T.Dim,
                new Rectangle(nameField.Left, nameField.Top - T.P(18), T.P(200), T.P(16)), T.FL);
            T.Str(g, L.T("DESCRIPTION", "BESCHREIBUNG"), T.FLabel, T.Dim,
                new Rectangle(descField.Left, descField.Top - T.P(18), T.P(260), T.P(16)), T.FL);
        }

        void DrawFooter(Graphics g)
        {
            T.HLine(g, 0, Width, rFooter.Y, T.LineSoft);
            int tile = T.P(20);
            int ty = rFooter.Y + (rFooter.Height - tile) / 2;
            int pad = T.P(18);
            T.LogoTile(g, new RectangleF(pad, ty, tile, tile), T.F(6f));
            int creditX = pad + tile + T.P(10);
            int creditW = Math.Max(T.P(200), Width - creditX - pad - T.P(430));
            T.StrEll(g, AppInfo.Credit, T.FSmall, T.Muted,
                new Rectangle(creditX, rFooter.Y, creditW, rFooter.Height), T.FL);

            if (toastText.Length > 0) return;

            string right = L.T("Saves to  %APPDATA%\\HabboBadgeCreatorBeta", "Speichert unter  %APPDATA%\\HabboBadgeCreatorBeta");
            T.Str(g, right, T.FMicro, T.Dim,
                new Rectangle(Width - pad - T.P(420), rFooter.Y, T.P(420), rFooter.Height), T.FR);
        }

        void DrawToast(Graphics g)
        {
            if (toastText.Length == 0) return;
            Size ts = T.Meas(toastText, T.FSmall);
            int w = Math.Max(T.P(200), ts.Width + T.P(52));
            int h = T.P(32);
            int pad = T.P(18);
            int x = Width - pad - w;
            int y = rFooter.Y + (rFooter.Height - h) / 2;
            RectangleF r = new RectangleF(x, y, w, h);
            T.FillRound(g, r, T.F(8f), T.ToastBg);
            T.StrokeRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1f, r.Height - 1f), T.F(8f), T.Line, 1f);
            float ic = T.F(13f);
            T.Icon(g, "check", new RectangleF(r.X + T.P(14), r.Y + (h - ic) / 2f, ic, ic), T.Gold);
            T.Str(g, toastText, T.FSmall, T.ToastText,
                new Rectangle((int)r.X + T.P(34), (int)r.Y, w - T.P(44), h), T.FL);
        }

        // ------------------------------------------------------------- behaviour
        void RefreshDoc()
        {
            // Performance mode: while a stroke runs only the touched cells repaint. The
            // toolbar, layer panel and preview wait for EndStroke instead of being redrawn
            // for every mouse move, which is what made the mode feel barely different.
            if (settings.PerfMode && doc.InStroke)
            {
                previewStale = true;
                if (!strokeTimer.Enabled) strokeTimer.Start();
                return;
            }

            tUndo.Invalidate();
            tRedo.Invalidate();
            if (layersPanel != null) layersPanel.Invalidate();

            previewStale = false;
            strokeTimer.Stop();
            canvas.Invalidate();
            previewBig.Invalidate();
        }

        /// <summary>Layer actions from the keyboard; the panel has the same buttons.</summary>
        void LayerAction(int slot)
        {
            if (!settings.LayersBeta)
            {
                ShowToast(L.T("Turn on layers first (Ctrl+L)", "Ebenen zuerst einschalten (Strg+L)"));
                return;
            }
            if (!layersPanel.FireButton(slot)) return;
            switch (slot)
            {
                case LayersPanel.SlotAdd:
                    ShowToast(L.T("Layer added", "Ebene hinzugefügt"));
                    break;
                case LayersPanel.SlotCopy:
                    ShowToast(L.T("Layer duplicated", "Ebene dupliziert"));
                    break;
                case LayersPanel.SlotMerge:
                    ShowToast(L.T("Merged into the layer below", "Mit der Ebene darunter zusammengefasst"));
                    break;
                case LayersPanel.SlotUp:
                    ShowToast(L.T("Layer moved up", "Ebene nach oben verschoben"));
                    break;
                case LayersPanel.SlotDown:
                    ShowToast(L.T("Layer moved down", "Ebene nach unten verschoben"));
                    break;
                default:
                    ShowToast(L.T("Layer deleted", "Ebene gelöscht"));
                    break;
            }
        }

        void ToggleMirror()
        {
            tMirror.Active = !tMirror.Active;
            canvas.Mirror = tMirror.Active;
            tMirror.Invalidate();
            ShowToast(tMirror.Active ? L.T("Mirror drawing on", "Spiegelzeichnen an") : L.T("Mirror drawing off", "Spiegelzeichnen aus"));
        }

        /// <summary>Centre guides, then thirds, then quarters, then off.</summary>
        void CycleGuides()
        {
            canvas.Guides = (canvas.Guides + 1) % 4;
            tCentre.Active = canvas.Guides > 0;
            tCentre.Invalidate();
            canvas.Invalidate();
            ShowToast(canvas.Guides == 1 ? L.T("Centre guides", "Mittellinien")
                : canvas.Guides == 2 ? L.T("Rule of thirds", "Drittelraster")
                : canvas.Guides == 3 ? L.T("Quarter guides", "Viertelraster")
                : L.T("Guides off", "Hilfslinien aus"));
        }

        /// <summary>Turns the layered canvas (beta) on or off.</summary>
        void ToggleLayers()
        {
            settings.LayersBeta = !settings.LayersBeta;
            tLayers.Active = settings.LayersBeta;
            tLayers.Invalidate();
            if (settings.LayersBeta)
            {
                layersPanel.Doc = doc;
                ShowToast(L.T("Layers (beta) on — up to 5 layers", "Ebenen (Beta) an – bis zu 5 Ebenen"));
            }
            else
            {
                doc.Flatten();
                ShowToast(L.T("Layers (beta) off — flattened to one layer", "Ebenen (Beta) aus – zu einer Ebene zusammengefasst"));
            }
            Store.SaveSettings(settings);
            LayoutAll();
            RefreshDoc();
        }

        /// <summary>Mirrors the whole artwork, so a badge can be flipped into its variant.</summary>
        void FlipCanvas(bool horizontal)
        {
            doc.Flip(horizontal);
            RefreshDoc();
            ShowToast(horizontal
                ? L.T("Flipped horizontally — Ctrl+Z to undo", "Horizontal gespiegelt – Strg+Z macht es rückgängig")
                : L.T("Flipped vertically — Ctrl+Z to undo", "Vertikal gespiegelt – Strg+Z macht es rückgängig"));
        }

        /// <summary>Beta: turns the artwork a quarter turn. Undoable.</summary>
        void RotateCanvas(bool clockwise)
        {
            doc.Rotate(clockwise);
            RefreshDoc();
            ShowToast(clockwise
                ? L.T("Rotated 90° clockwise — Ctrl+Z to undo", "Um 90° im Uhrzeigersinn gedreht – Strg+Z macht es rückgängig")
                : L.T("Rotated 90° anticlockwise — Ctrl+Z to undo", "Um 90° gegen den Uhrzeigersinn gedreht – Strg+Z macht es rückgängig"));
        }

        /// <summary>Beta: puts the flattened 40 x 40 badge on the clipboard as a PNG-quality image.</summary>
        void CopyBadge()
        {
            try
            {
                using (Bitmap bmp = doc.ToBitmap())
                    Clipboard.SetImage(bmp);
                ShowToast(L.T("Badge copied to clipboard", "Badge in die Zwischenablage kopiert"));
            }
            catch
            {
                ShowToast(L.T("Could not copy the badge", "Badge konnte nicht kopiert werden"));
            }
        }

        void SetTool(int t)
        {
            tool = t;
            canvas.Tool = t;
            UpdateToolUi();
        }

        void SetColor(Color c)
        {
            drawColor = Color.FromArgb(255, c.R, c.G, c.B);
            canvas.DrawColor = drawColor;
            colorChip.Value = drawColor;
            colorChip.Invalidate();
            if (tool == 3) SetTool(0);
        }

        void PickCustomColor()
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = drawColor;
                dlg.FullOpen = true;
                dlg.AnyColor = true;
                if (dlg.ShowDialog(this) == DialogResult.OK) SetColor(dlg.Color);
            }
        }

        // ------------------------------------------------------------- settings
        void SetLanguage(bool german)
        {
            L.De = german;
            settings.Lang = german ? "de" : "en";
            Store.SaveSettings(settings);
            if (langSwitch != null)
            {
                langSwitch.German = german;
                langSwitch.Invalidate();
            }
            ApplyLanguage();
        }

        void ApplyLanguage()
        {
            if (nameField == null) return;
            btnImport.Label = L.T("Import image", "Bild importieren");
            btnClear.Label = L.T("Clear", "Leeren");
            btnSave.Label = L.T("Save badge", "Badge speichern");
            btnExport.Label = L.T("Export PNG", "PNG exportieren");
            btnOpenFolder.Label = L.T("Open folder", "Ordner öffnen");
            changelogChip.Label = L.T("Changelog", "Änderungen");
            changelogChip.Invalidate();
            settingsChip.Label = L.T("Settings", "Einstellungen");
            settingsChip.Invalidate();
            nameField.ResetPlaceholder(L.T("Badge name", "Name des Badges"));
            descField.ResetPlaceholder(L.T("What is this badge about?", "Wofür steht das Badge?"));
            foreach (Control c in Controls) c.Invalidate();
            ApplyTooltips();
            UpdateDiscordActivity();
            LayoutAll();
            Invalidate();
        }

        void ApplyTooltips()
        {
            if (tPencil == null) return;
            SetTip(tPencil, L.T("Pencil", "Stift"), "B");
            SetTip(tEraser, L.T("Eraser", "Radierer"), "E");
            SetTip(tFill, L.T("Fill", "Füllen"), "G");
            SetTip(tPicker, L.T("Colour picker", "Pipette"), "I");
            SetTip(tMirror, L.T("Mirror drawing", "Spiegelzeichnen"), "M");
            SetTip(tFlipH, L.T("Flip horizontally", "Horizontal spiegeln"), "Shift+H");
            SetTip(tFlipV, L.T("Flip vertically", "Vertikal spiegeln"), "Shift+V");
            SetTip(tRotate, L.T("Rotate 90°", "Um 90° drehen"), "R · Shift+R");
            SetTip(btnCopyBadge, L.T("Copy badge", "Badge kopieren"), "Ctrl+Shift+C");
            SetTip(tGrid, L.T("Pixel grid", "Pixelraster"), "");
            SetTip(tCentre, L.T("Guides", "Hilfslinien"), "C");
            SetTip(tLayers, L.T("Layers (beta)", "Ebenen (Beta)"), "Ctrl+L");
            SetTip(tUndo, L.T("Undo", "Rückgängig"), "Ctrl+Z");
            SetTip(tRedo, L.T("Redo", "Wiederholen"), "Ctrl+Y");
            SetTip(tZoomOut, L.T("Zoom out", "Verkleinern"), "Ctrl−");
            SetTip(tZoomIn, L.T("Zoom in", "Vergrößern"), "Ctrl+");
            HideTip();
        }

        static void SetTip(IconBtn b, string title, string key)
        {
            if (b == null) return;   // tips are applied while the UI is still being built
            b.TipTitle = title;
            b.TipKey = key;
        }

        void OpenSettings()
        {
            HideTip();
            AppSettings was = settings.Clone();
            using (SettingsForm f = new SettingsForm(settings))
            {
                f.PreviewSettings = ApplyTheme;
                f.ShowDialog(this);
                if (f.Saved)
                {
                    settings = f.Result;
                    Store.SaveSettings(settings);
                    ApplyDiscordSettings();
                    ApplyTheme(settings);
                    canvas.PerfMode = settings.PerfMode;
                    RefreshDoc();
                }
                else ApplyTheme(was);   // a cancelled theme preview reverts
            }
            UpdateSettingsStatus();
        }

        /// <summary>Repaints the whole window with the palette the settings describe.</summary>
        void ApplyTheme(AppSettings s)
        {
            T.Apply(s);
            BackColor = T.Bg2;
            foreach (Control c in Controls)
            {
                string role = c.Tag as string;
                c.BackColor = role == "bar" ? T.Bg2 : role == "segment" ? T.Segment : T.Card;
            }
            if (nameField != null)
            {
                nameField.ApplyTheme();
                descField.ApplyTheme();
            }
            Invalidate(true);
            foreach (Control c in Controls) c.Invalidate();
        }

        void ApplyDiscordSettings()
        {
            string id = DiscordRpc.ResolveAppId(settings.DiscordAppId);
            if (settings.DiscordEnabled && id.Length > 0)
                DiscordRpc.Start(id, settings.DiscordImageKey);
            else
                DiscordRpc.Stop();
            UpdateDiscordActivity();
            UpdateSettingsStatus();
        }

        /// <summary>Shows the Discord connection state as a dot on the Settings chip.</summary>
        void UpdateSettingsStatus()
        {
            if (settingsChip == null) return;
            int s = 0;
            if (settings.DiscordEnabled && DiscordRpc.ResolveAppId(settings.DiscordAppId).Length > 0)
            {
                switch (DiscordRpc.Status)
                {
                    case "connected": s = 2; break;
                    case "error": s = 3; break;
                    default: s = 1; break;   // starting, waiting for Discord, reconnecting
                }
            }
            bool hadDot = settingsChip.Status > 0;
            settingsChip.Status = s;
            settingsChip.Invalidate();
            if (hadDot != (s > 0)) LayoutAll();   // the dot takes its own room in the chip
        }

        void UpdateDiscordActivity()
        {
            if (nameField == null) return;
            string nm = nameField.Value.Trim();
            DiscordRpc.SetActivity(
                L.T("Creating Habbo badges", "Erstellt Habbo-Badges"),
                nm.Length >= 2 ? nm : L.T("40 × 40 px canvas", "40 × 40 px Leinwand"));
        }

        void OpenChangelog()
        {
            HideTip();
            using (ChangelogForm f = new ChangelogForm()) f.ShowDialog(this);
        }

        /// <summary>Opens the settings sheet without a mouse click.</summary>
        public void DebugSettings()
        {
            OpenSettings();
        }

        /// <summary>Opens the changelog for screenshots without a mouse click.</summary>
        public void DebugChangelog()
        {
            using (ChangelogForm f = new ChangelogForm()) f.ShowDialog(this);
        }

        int BtnWidth(FlatBtn b, int min)
        {
            int w = T.Meas(b.Label, T.FBtn).Width + T.P(34);
            if (b.Icon != null) w += T.P(21);
            return Math.Max(min, w);
        }

        void ReloadGallery()
        {
            List<BadgeEntry> old = gallery.Items;
            gallery.SetItems(Store.List());
            foreach (BadgeEntry e in old)
                if (e.Thumb != null) e.Thumb.Dispose();
            LayoutAll();
        }

        void LoadBadge(BadgeEntry e)
        {
            if (e == null || e.Thumb == null) return;
            doc.LoadBitmap(e.Thumb);
            nameField.Value = e.Name;
            descField.Value = e.Desc;
            RefreshDoc();
            ShowToast(L.F("Loaded \"{0}\"", "\"{0}\" geladen", string.IsNullOrEmpty(e.Name) ? L.T("Untitled badge", "Unbenanntes Badge") : e.Name));
        }

        void DeleteBadge(BadgeEntry e)
        {
            if (e == null) return;
            string nm = string.IsNullOrEmpty(e.Name) ? L.T("Untitled badge", "Unbenanntes Badge") : e.Name;
            DialogResult r = MessageBox.Show(this,
                L.F("Delete \"{0}\" from your badges?\nThis cannot be undone.",
                    "\"{0}\" aus deinen Badges löschen?\nDas kann nicht rückgängig gemacht werden.", nm),
                L.T("Delete badge", "Badge löschen"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
            Store.Delete(e);
            ReloadGallery();
            ShowToast(L.T("Badge deleted", "Badge gelöscht"));
        }

        void SaveBadge()
        {
            doc.EndStroke();
            string name = nameField.Value.Trim();
            if (name.Length == 0)
            {
                ShowToast(L.T("Give your badge a name first", "Gib deinem Badge zuerst einen Namen"));
                nameField.Box.Focus();
                return;
            }
            if (doc.IsBlank())
            {
                ShowToast(L.T("Draw something first", "Zeichne zuerst etwas"));
                return;
            }
            using (Bitmap bmp = doc.ToBitmap())
            {
                Store.Save(name, descField.Value.Trim(), bmp);
            }
            ReloadGallery();
            lastDraftVersion = doc.Version;
            ShowToast(L.F("Saved \"{0}\" to My badges", "\"{0}\" in Meine Badges gespeichert", name));
        }

        void ExportPng()
        {
            if (doc.IsBlank()) { ShowToast(L.T("Draw something first", "Zeichne zuerst etwas")); return; }
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = L.T("Export badge as PNG", "Badge als PNG exportieren");
                dlg.Filter = L.T("PNG image (*.png)|*.png", "PNG-Bild (*.png)|*.png");
                dlg.DefaultExt = "png";
                dlg.FileName = Store.SafeName(nameField.Value.Length > 0 ? nameField.Value : "habbo-badge") + ".png";
                try
                {
                    string desk = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (Directory.Exists(desk)) dlg.InitialDirectory = desk;
                }
                catch { }
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (Bitmap bmp = doc.ToBitmap())
                        Store.ExportPng(bmp, dlg.FileName);
                    ShowToast(L.F("Exported {0}", "{0} exportiert", Path.GetFileName(dlg.FileName)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, L.T("Could not save the image:\n", "Bild konnte nicht gespeichert werden:\n") + ex.Message,
                        L.T("Export failed", "Export fehlgeschlagen"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ImportImage(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image img = Image.FromStream(fs))
                using (Bitmap bmp = new Bitmap(img))
                {
                    doc.LoadBitmap(bmp);
                }
                RefreshDoc();
                if (nameField.Value.Trim().Length == 0)
                {
                    string n = Path.GetFileNameWithoutExtension(path);
                    if (n.Length > 40) n = n.Substring(0, 40);
                    nameField.Value = n;
                }
                ShowToast(L.T("Image imported and scaled to 40 × 40", "Bild importiert und auf 40 × 40 skaliert"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.T("Could not read this image:\n", "Bild konnte nicht gelesen werden:\n") + ex.Message,
                    L.T("Import failed", "Import fehlgeschlagen"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenImportDialog()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = L.T("Choose an image for your badge", "Bild für dein Badge wählen");
                dlg.Filter = L.T("Images (*.png;*.gif;*.jpg;*.jpeg;*.bmp)|*.png;*.gif;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                    "Bilder (*.png;*.gif;*.jpg;*.jpeg;*.bmp)|*.png;*.gif;*.jpg;*.jpeg;*.bmp|Alle Dateien (*.*)|*.*");
                if (dlg.ShowDialog(this) == DialogResult.OK) ImportImage(dlg.FileName);
            }
        }

        /// <summary>Hides any toast, so rendered screenshots stay clean.</summary>
        public void ClearToast()
        {
            toastTimer.Stop();
            toastText = "";
            Invalidate();
        }

        void ShowToast(string msg)
        {
            toastText = msg;
            toastTimer.Stop();
            toastTimer.Start();
            Invalidate(new Rectangle(0, rFooter.Y, Width, rFooter.Height));
        }

        void AutoSaveDraft()
        {
            if (Program.Demo) return;
            if (doc.Version == lastDraftVersion) return;
            try
            {
                using (Bitmap bmp = doc.ToBitmap())
                    Store.SaveDraft(bmp, nameField.Value.Trim(), descField.Value.Trim());
                lastDraftVersion = doc.Version;
            }
            catch { }
        }

        void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized) WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Maximized;
            wbMax.Icon = WindowState == FormWindowState.Maximized ? "restore" : "max";
            wbMax.Invalidate();
            ApplyWindowShape();
        }

        /// <summary>Clips the window to rounded corners (Windows 10 has no native rounding).</summary>
        void ApplyWindowShape()
        {
            if (rTb.Width <= 0) return;
            if (WindowState == FormWindowState.Maximized)
            {
                Region = null;
                return;
            }
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            settings = Store.LoadSettings();
            if (Program.ForceLang == "de") settings.Lang = "de";
            if (Program.ForceLang == "en") settings.Lang = "en";
            if (Program.ForceMode.HasValue)
            {
                settings.LightTheme = Program.ForceMode.Value == ThemeMode.Light;
                settings.NavyTheme = Program.ForceMode.Value == ThemeMode.Navy;
                settings.CustomTheme = Program.ForceMode.Value == ThemeMode.Custom;
            }
            ApplyTheme(settings);
            canvas.PerfMode = settings.PerfMode;
            L.De = settings.Lang == "de";
            if (langSwitch != null)
            {
                langSwitch.German = L.De;
                langSwitch.Invalidate();
            }
            ApplyLanguage();

            DiscordRpc.Changed += delegate
            {
                if (IsDisposed || !IsHandleCreated) return;
                try { BeginInvoke((Action)delegate { UpdateSettingsStatus(); }); }
                catch { }
            };
            ApplyDiscordSettings();

            LayoutAll();

            Bitmap draft;
            string dn, dd;
            if (Store.LoadDraft(out draft, out dn, out dd))
            {
                using (draft)
                {
                    if (!IsBlank(draft))
                    {
                        doc.CopyFromBitmap(draft);
                        if (dn.Length > 0) nameField.Value = dn;
                        if (dd.Length > 0) descField.Value = dd;
                        if (!Program.Demo) ShowToast(L.T("Restored your last draft", "Letzter Entwurf wiederhergestellt"));
                    }
                }
            }
            lastDraftVersion = -1;
            AutoSaveDraft();
            ReloadGallery();
            RefreshDoc();
            canvas.Focus();
        }

        static bool IsBlank(Bitmap b)
        {
            for (int y = 0; y < b.Height; y++)
                for (int x = 0; x < b.Width; x++)
                    if (b.GetPixel(x, y).A != 0) return false;
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                AutoSaveDraft();
                draftTimer.Stop();
                strokeTimer.Stop();
                DiscordRpc.Stop();
            }
            catch { }
            base.OnFormClosing(e);
        }

        // ------------------------------------------------------------- shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool shift = (keyData & Keys.Shift) == Keys.Shift;
            bool typing = nameField.ContainsFocus || descField.ContainsFocus;

            if (ctrl)
            {
                switch (k)
                {
                    case Keys.S: SaveBadge(); return true;
                    case Keys.E: ExportPng(); return true;
                    case Keys.O: OpenImportDialog(); return true;
                    case Keys.J: LayerAction(LayersPanel.SlotCopy); return true;
                    case Keys.M: LayerAction(LayersPanel.SlotMerge); return true;
                    case Keys.Z:
                        if (shift) doc.Redo(); else doc.Undo();
                        RefreshDoc(); return true;
                    case Keys.Y: doc.Redo(); RefreshDoc(); return true;
                    case Keys.L: ToggleLayers(); return true;
                    case Keys.C: if (shift) { CopyBadge(); return true; } break;
                    case Keys.Add:
                    case Keys.Oemplus: autoZoom = false; canvas.Zoom = canvas.Zoom + 1; UpdateToolUi(); return true;
                    case Keys.Subtract:
                    case Keys.OemMinus: autoZoom = false; canvas.Zoom = canvas.Zoom - 1; UpdateToolUi(); return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            bool alt = (keyData & Keys.Alt) == Keys.Alt;
            if (alt)
            {
                if (k == Keys.Up) { LayerAction(LayersPanel.SlotUp); return true; }
                if (k == Keys.Down) { LayerAction(LayersPanel.SlotDown); return true; }
            }

            if (typing) return base.ProcessCmdKey(ref msg, keyData);

            if (shift)
            {
                if (k == Keys.H) { FlipCanvas(true); return true; }
                if (k == Keys.V) { FlipCanvas(false); return true; }
                if (k == Keys.R) { RotateCanvas(false); return true; }
            }

            // arrow keys nudge the active layer, one pixel or five with Shift
            int step = shift ? 5 : 1;
            if (k == Keys.Left) { doc.Nudge(-step, 0); RefreshDoc(); return true; }
            if (k == Keys.Right) { doc.Nudge(step, 0); RefreshDoc(); return true; }
            if (k == Keys.Up) { doc.Nudge(0, -step); RefreshDoc(); return true; }
            if (k == Keys.Down) { doc.Nudge(0, step); RefreshDoc(); return true; }

            switch (k)
            {
                case Keys.B: SetTool(0); return true;
                case Keys.E: SetTool(1); return true;
                case Keys.G: SetTool(2); return true;
                case Keys.I: SetTool(3); return true;
                case Keys.M: ToggleMirror(); return true;
                case Keys.C: CycleGuides(); return true;
                case Keys.R: RotateCanvas(true); return true;
                case Keys.D0: autoZoom = true; LayoutAll(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Shows the pencil tooltip for layout screenshots.</summary>
        public void DebugTip()
        {
            if (tipView != null && tPencil != null) tipView.ShowFor(tPencil);
        }

        /// <summary>Shows the duplicate-layer tooltip for layout screenshots.</summary>
        public void DebugLayerTip()
        {
            settings.LayersBeta = true;
            if (tLayers != null) { tLayers.Active = true; tLayers.Invalidate(); }
            layersPanel.Doc = doc;
            doc.AddLayer();
            LayoutAll();
            if (layersPanel == null || tipView == null || !layersPanel.Visible) return;
            Rectangle r = layersPanel.SlotRect(LayersPanel.SlotCopy);
            tipView.ShowAt(new Rectangle(layersPanel.Left + r.X, layersPanel.Top + r.Y, r.Width, r.Height),
                L.T("Duplicate layer", "Ebene duplizieren"), "Ctrl+J");
        }

        /// <summary>Turns the centre guides on for layout screenshots.</summary>
        public void DebugGuides()
        {
            DebugGuides(1);
        }

        /// <summary>Shows a specific guide mode: 1 centre, 2 thirds, 3 quarters.</summary>
        public void DebugGuides(int mode)
        {
            if (tCentre == null) return;
            canvas.Guides = Math.Max(1, Math.Min(3, mode));
            tCentre.Active = true;
            tCentre.Invalidate();
            canvas.Invalidate();
        }

        /// <summary>Builds a two-layer demo document for layout screenshots.</summary>
        public void DebugLayers()
        {
            settings.LayersBeta = true;
            if (tLayers != null) { tLayers.Active = true; tLayers.Invalidate(); }
            layersPanel.Doc = doc;
            doc.AddLayer();
            doc.BeginStroke();
            doc.StrokeLine(4, 34, 34, 4, unchecked((int)0xFFFFFFFF), false);
            doc.EndStroke();
            LayoutAll();
            RefreshDoc();
        }

        /// <summary>Turns the beta layer rail off for layout screenshots of the plain editor.</summary>
        public void DebugHideLayers()
        {
            settings.LayersBeta = false;
            if (tLayers != null) { tLayers.Active = false; tLayers.Invalidate(); }
            LayoutAll();
            RefreshDoc();
        }

        /// <summary>Arranges named layers with opacity, blend and a lock for layout screenshots.</summary>
        public void DebugLayerDemo()
        {
            if (!settings.LayersBeta) DebugLayers();
            doc.Flatten();            // start from one artwork layer so the demo shows exactly three
            doc.SetLayerName(0, "Base");
            doc.SetLayerOpacity(0, 255);
            doc.AddLayer();
            doc.SetLayerName(1, "Shading");
            doc.SetLayerOpacity(1, 160);
            doc.SetLayerMode(1, PixelDoc.LayerBlend.Multiply);
            doc.AddLayer();
            doc.SetLayerName(2, "Highlight");
            doc.SetLayerLocked(0, true);
            LayoutAll();
            RefreshDoc();
        }

        /// <summary>Shows the beta tooltip for screenshots.</summary>
        public void DebugBetaTip()
        {
            ShowBetaTip();
        }

        void ShowBetaTip()
        {
            if (tipView == null || betaTag == null) return;
            tipView.ShowFor(betaTag,
                L.T("Beta build", "Beta-Build"),
                "v" + AppInfo.Version + "  ·  " + L.T("build ", "Build ") + AppInfo.Build);
        }

        /// <summary>Shows the Settings chip tooltip for layout screenshots.</summary>
        public void DebugSettingsTip()
        {
            ShowSettingsTip();
        }

        /// <summary>Draws mid-stroke without finishing it, to prove the canvas repaints live.</summary>
        public void DebugStroke()
        {
            doc.BeginStroke();
            doc.StrokeLine(6, 33, 34, 6, Color.White.ToArgb(), false);
            doc.StrokeLine(7, 35, 24, 35, Color.FromArgb(255, 90, 200, 255).ToArgb(), false);
            // deliberately not EndStroke(): the canvas must already show this
        }

        /// <summary>Fills the window with sample content for layout screenshots.</summary>
        public void ApplyDemo()
        {
            using (Bitmap b = new Bitmap(PixelDoc.N, PixelDoc.N, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (GraphicsPath p = T.Round(new RectangleF(0, 0, 40, 40), 8f))
                using (LinearGradientBrush lg = new LinearGradientBrush(
                    new Rectangle(0, 0, 40, 40), Color.FromArgb(58, 96, 168), Color.FromArgb(15, 28, 52), 90f))
                    g.FillPath(lg, p);
                using (SolidBrush gold = new SolidBrush(Color.FromArgb(247, 197, 74)))
                {
                    g.FillPolygon(gold, new PointF[] {
                        new PointF(8f, 27f), new PointF(11f, 14f), new PointF(16f, 21f),
                        new PointF(20f, 10f), new PointF(24f, 21f), new PointF(29f, 14f), new PointF(32f, 27f) });
                    g.FillRectangle(gold, 9, 28, 22, 3);
                }
                using (SolidBrush w = new SolidBrush(Color.FromArgb(255, 255, 255)))
                {
                    g.FillRectangle(w, 30, 7, 2, 2);
                    g.FillRectangle(w, 6, 8, 1, 1);
                    g.FillRectangle(w, 34, 33, 1, 1);
                }
                doc.CopyFromBitmap(b);
            }
            nameField.Value = "Golden Crown";
            descField.Value = "A shiny crown badge made with the creator.";

            string[] names = { "Golden Crown", "Neon Heart", "Ocean Badge" };
            int[] colors = { 0xF5A623, 0xF2566B, 0x4FD1C5 };
            List<BadgeEntry> demo = new List<BadgeEntry>();
            for (int i = 0; i < names.Length; i++)
            {
                BadgeEntry e = new BadgeEntry();
                e.Id = "demo-" + i;
                e.Name = names[i];
                e.Desc = "Demo entry";
                e.Created = DateTime.Now.AddDays(-i).AddMinutes(-7 * i);
                Bitmap t = new Bitmap(40, 40, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(t))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    int c = colors[i];
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(255, (c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF)))
                    using (GraphicsPath p = T.Round(new RectangleF(2, 2, 36, 36), 8f))
                        g.FillPath(b, p);
                    using (SolidBrush w = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                        g.FillEllipse(w, 12, 12, 16, 16);
                }
                e.Thumb = t;
                demo.Add(e);
            }
            gallery.SetItems(demo);
            SetTool(0);
            LayoutAll();
            RefreshDoc();
        }

        // ------------------------------------------------------------- chrome
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            if (e.Y < rTb.Bottom)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
                return;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Y < rTb.Bottom) ToggleMaximize();
        }

        const int WM_NCHITTEST = 0x84;
        const int HTCLIENT = 1, HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13, HTTOPRIGHT = 14,
                  HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT && WindowState == FormWindowState.Normal)
                {
                    int lp = m.LParam.ToInt32();
                    int sx = (short)(lp & 0xFFFF);
                    int sy = (short)((lp >> 16) & 0xFFFF);
                    Point p = PointToClient(new Point(sx, sy));
                    int g = T.P(7);
                    bool left = p.X <= g, right = p.X >= ClientSize.Width - g;
                    bool top = p.Y <= g, bottom = p.Y >= ClientSize.Height - g;
                    if (top && left) m.Result = (IntPtr)HTTOPLEFT;
                    else if (top && right) m.Result = (IntPtr)HTTOPRIGHT;
                    else if (bottom && left) m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (bottom && right) m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (left) m.Result = (IntPtr)HTLEFT;
                    else if (right) m.Result = (IntPtr)HTRIGHT;
                    else if (top) m.Result = (IntPtr)HTTOP;
                    else if (bottom) m.Result = (IntPtr)HTBOTTOM;
                }
                return;
            }
            base.WndProc(ref m);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0) ImportImage(files[0]);
            }
            catch { }
        }
    }
}
