using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    internal static class Program
    {
        public static bool Demo;
        public static string ForceLang;
        public static ThemeMode? ForceMode;
        public static int GuideMode = 1;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { SetProcessDPIAware(); }
            catch { }

            float dpi = 96f;
            try
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) dpi = g.DpiX;
            }
            catch { }
            if (dpi < 96f) dpi = 96f;
            T.S = dpi / 96f;
            string forced = Value(args, "--scale");
            bool forcedScale = false;
            if (forced != null)
            {
                float f;
                if (float.TryParse(forced, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out f) && f >= 1f && f <= 3f)
                {
                    T.S = f;
                    forcedScale = true;
                }
            }
            // a real DPI already grows the fonts; a faked scale has to do it explicitly
            T.Init(forcedScale ? T.S : 1f);
            Store.Ensure();

            // the boot screen already uses the palette the app is set to
            AppSettings prefs = Store.LoadSettings();
            string themeArg = Value(args, "--theme");
            if (themeArg != null)
            {
                if (string.Equals(themeArg, "light", StringComparison.OrdinalIgnoreCase)) ForceMode = ThemeMode.Light;
                else if (string.Equals(themeArg, "dark", StringComparison.OrdinalIgnoreCase)) ForceMode = ThemeMode.Dark;
                else if (string.Equals(themeArg, "navy", StringComparison.OrdinalIgnoreCase)) ForceMode = ThemeMode.Navy;
                else if (string.Equals(themeArg, "custom", StringComparison.OrdinalIgnoreCase)) ForceMode = ThemeMode.Custom;
            }
            ApplyForced(prefs);
            T.Apply(prefs);

            if (Has(args, "--selftest"))
            {
                string log = Value(args, "--selftest");
                Environment.ExitCode = SelfTest.Run(log) ? 0 : 1;
                return;
            }

            Demo = Has(args, "--demo");
            ForceLang = Value(args, "--lang");
            string guideArg = Value(args, "--guide-mode");
            if (guideArg != null) int.TryParse(guideArg, out GuideMode);

            bool noSplash = Has(args, "--nosplash") || Has(args, "--shot") || Has(args, "--demo") ||
                            Has(args, "--layers-demo") || Has(args, "--shot-designer") ||
                            Has(args, "--tip") || Has(args, "--changelog") || Has(args, "--settings") ||
                            Has(args, "--layer-tip") || Has(args, "--shot-settings") || Has(args, "--shot-changelog") ||
                            Has(args, "--shot-splash") || Has(args, "--shot-picker") || Has(args, "--picker");
            if (!noSplash) Application.Run(new SplashForm());

            string splashShot = Value(args, "--shot-splash");
            if (splashShot != null)
            {
                ShotSplash(splashShot);
                return;
            }

            string designerShot = Value(args, "--shot-designer");
            if (designerShot != null)
            {
                ShotDesigner(designerShot);
                return;
            }

            string sheetShot = Value(args, "--shot-settings");
            if (sheetShot != null)
            {
                ShotSettings(sheetShot);
                return;
            }
            string notesShot = Value(args, "--shot-changelog");
            if (notesShot != null)
            {
                ShotChangelog(notesShot);
                return;
            }
            string pickerShot = Value(args, "--shot-picker");
            if (pickerShot != null)
            {
                ShotPicker(pickerShot);
                return;
            }

            MainForm form = new MainForm();
            string shot = Value(args, "--shot");
            bool chipTip = string.Equals(Value(args, "--tip"), "chip", StringComparison.OrdinalIgnoreCase);
            if (shot != null)
            {
                Screenshot(form, shot, Has(args, "--stroke"), Has(args, "--tip"), chipTip, Has(args, "--guides"), Has(args, "--layers"), Has(args, "--layer-tip"), Has(args, "--layers-demo"), Has(args, "--no-layers"));
                return;
            }
            if (Has(args, "--tip"))
            {
                System.Windows.Forms.Timer tip = new System.Windows.Forms.Timer();
                tip.Interval = 900;
                tip.Tick += delegate
                {
                    tip.Stop();
                    tip.Dispose();
                    if (chipTip) form.DebugSettingsTip(); else form.DebugTip();
                };
                tip.Start();
            }
            if (Has(args, "--settings"))
            {
                System.Windows.Forms.Timer sheet = new System.Windows.Forms.Timer();
                sheet.Interval = 800;
                sheet.Tick += delegate
                {
                    sheet.Stop();
                    sheet.Dispose();
                    form.DebugSettings();
                };
                sheet.Start();
            }
            if (Has(args, "--layer-tip"))
            {
                System.Windows.Forms.Timer lt = new System.Windows.Forms.Timer();
                lt.Interval = 900;
                lt.Tick += delegate
                {
                    lt.Stop();
                    lt.Dispose();
                    form.DebugLayerTip();
                };
                lt.Start();
            }
            if (Has(args, "--changelog"))
            {
                System.Windows.Forms.Timer changelog = new System.Windows.Forms.Timer();
                changelog.Interval = 800;
                changelog.Tick += delegate
                {
                    changelog.Stop();
                    changelog.Dispose();
                    form.DebugChangelog();
                };
                changelog.Start();
            }
            if (Has(args, "--picker"))
            {
                System.Windows.Forms.Timer picker = new System.Windows.Forms.Timer();
                picker.Interval = 800;
                picker.Tick += delegate
                {
                    picker.Stop();
                    picker.Dispose();
                    form.DebugPicker();
                };
                picker.Start();
            }
            Application.Run(form);
        }

        /// <summary>Applies --theme to a settings copy without touching the stored file.</summary>
        static void ApplyForced(AppSettings prefs)
        {
            if (!ForceMode.HasValue) return;
            prefs.LightTheme = ForceMode.Value == ThemeMode.Light;
            prefs.NavyTheme = ForceMode.Value == ThemeMode.Navy;
            prefs.CustomTheme = ForceMode.Value == ThemeMode.Custom;
        }

        static bool Has(string[] args, string flag)
        {
            foreach (string a in args) if (string.Equals(a, flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        static string Value(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        /// <summary>Renders the window to a PNG for visual checks.</summary>
        static void Screenshot(MainForm form, string path, bool stroke, bool tip, bool chipTip, bool guides, bool layers, bool layerTip, bool layerDemo, bool noLayers)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(40, 40);
            form.Show();
            Application.DoEvents();
            if (Demo) form.ApplyDemo();
            form.ClearToast();
            if (stroke) form.DebugStroke();
            if (guides) form.DebugGuides(GuideMode);
            if (layers) form.DebugLayers();
            if (layerDemo) form.DebugLayerDemo();
            if (noLayers) form.DebugHideLayers();
            if (layerTip) form.DebugLayerTip();
            if (tip) { if (chipTip) form.DebugSettingsTip(); else form.DebugTip(); }
            Application.DoEvents();
            System.Threading.Thread.Sleep(450);
            Application.DoEvents();
            using (Bitmap b = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(b, new Rectangle(0, 0, form.Width, form.Height));
                b.Save(path, ImageFormat.Png);
            }
            form.Close();
        }

        /// <summary>Renders the boot splash to a PNG for visual checks.</summary>
        static void ShotSplash(string path)
        {
            AppSettings prefs = Store.LoadSettings();
            ApplyForced(prefs);
            T.Apply(prefs);
            L.De = ForceLang == "de" || (ForceLang != "en" && prefs.Lang == "de");
            using (SplashForm f = new SplashForm())
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.Show();
                f.PrepareShot();
                Application.DoEvents();
                System.Threading.Thread.Sleep(200);
                Application.DoEvents();
                using (Bitmap b = new Bitmap(f.Width, f.Height))
                {
                    f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                    b.Save(path, ImageFormat.Png);
                }
                f.Close();
            }
        }

        /// <summary>Renders the beta custom-theme designer to a PNG for visual checks.</summary>
        static void ShotDesigner(string path)
        {
            AppSettings prefs = Store.LoadSettings();
            ApplyForced(prefs);
            T.Apply(prefs);
            L.De = ForceLang == "de" || (ForceLang != "en" && prefs.Lang == "de");
            T.ApplyCustom(prefs.SurfaceColor, prefs.AccentColor, prefs.TextColor);
            using (ThemeDesignerForm f = new ThemeDesignerForm(prefs.SurfaceColor, prefs.AccentColor, prefs.TextColor, prefs.Presets))
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.Show();
                Application.DoEvents();
                System.Threading.Thread.Sleep(300);
                Application.DoEvents();
                using (Bitmap b = new Bitmap(f.Width, f.Height))
                {
                    f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                    b.Save(path, ImageFormat.Png);
                }
                f.Close();
            }
        }

        /// <summary>Renders the settings sheet to a PNG for visual checks.</summary>
        static void ShotSettings(string path)
        {
            AppSettings prefs = Store.LoadSettings();
            ApplyForced(prefs);
            T.Apply(prefs);
            L.De = ForceLang == "de" || (ForceLang != "en" && prefs.Lang == "de");
            using (SettingsForm f = new SettingsForm(prefs))
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.Show();
                Application.DoEvents();
                System.Threading.Thread.Sleep(300);
                Application.DoEvents();
                using (Bitmap b = new Bitmap(f.Width, f.Height))
                {
                    f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                    b.Save(path, ImageFormat.Png);
                }
                f.Close();
            }
        }

        /// <summary>Renders the changelog sheet to a PNG for visual checks.</summary>
        static void ShotChangelog(string path)
        {
            AppSettings prefs = Store.LoadSettings();
            ApplyForced(prefs);
            T.Apply(prefs);
            L.De = ForceLang == "de" || (ForceLang != "en" && prefs.Lang == "de");
            using (ChangelogForm f = new ChangelogForm())
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.Show();
                Application.DoEvents();
                System.Threading.Thread.Sleep(300);
                Application.DoEvents();
                using (Bitmap b = new Bitmap(f.Width, f.Height))
                {
                    f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                    b.Save(path, ImageFormat.Png);
                }
                f.Close();
            }
        }

        /// <summary>Renders the colour sheet to a PNG for visual checks; follows --theme.</summary>
        static void ShotPicker(string path)
        {
            AppSettings prefs = Store.LoadSettings();
            ApplyForced(prefs);
            T.Apply(prefs);
            L.De = ForceLang == "de" || (ForceLang != "en" && prefs.Lang == "de");
            using (ColorSheetForm f = new ColorSheetForm(Color.FromArgb(255, 245, 166, 35)))
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(40, 40);
                f.Show();
                Application.DoEvents();
                System.Threading.Thread.Sleep(300);
                Application.DoEvents();
                using (Bitmap b = new Bitmap(f.Width, f.Height))
                {
                    f.DrawToBitmap(b, new Rectangle(0, 0, f.Width, f.Height));
                    b.Save(path, ImageFormat.Png);
                }
                f.Close();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();
    }

    /// <summary>Headless checks for the pixel document and badge storage.</summary>
    internal static class SelfTest
    {
        static int pass, fail;
        static System.Text.StringBuilder log;

        static void W(string s)
        {
            if (log != null) log.AppendLine(s);
            Console.WriteLine(s);
        }

        public static bool Run(string logPath)
        {
            log = new System.Text.StringBuilder();
            W("Habbo Badge Creator self-test");
            W(string.Format("scale={0:0.##}  storage={1}", T.S, Store.Root));

            PixelDoc doc = new PixelDoc();
            Check("fresh canvas is 40x40 and blank", doc.IsBlank() && PixelDoc.N == 40);

            doc.BeginStroke();
            doc.SetPixels(0, 0, unchecked((int)0xFFFF0000), false);
            doc.EndStroke();
            Check("set pixel", doc[0, 0] == unchecked((int)0xFFFF0000));
            Check("neighbour untouched", doc[1, 0] == 0);

            doc.BeginStroke();
            doc.StrokeLine(0, 10, 39, 10, unchecked((int)0xFF00FF00), false);
            doc.EndStroke();
            bool lineOk = true;
            for (int x = 0; x < 40; x++) if (doc[x, 10] != unchecked((int)0xFF00FF00)) lineOk = false;
            Check("bresenham line fills 40 pixels", lineOk);

            int versionBefore = doc.Version;
            doc.BeginStroke();
            doc.SetPixels(5, 5, unchecked((int)0xFF00FFFF), true);
            Check("pixel write bumps version (live canvas repaint)", doc.Version != versionBefore);
            doc.EndStroke();
            Check("mirror writes both halves", doc[5, 5] == unchecked((int)0xFF00FFFF) && doc[34, 5] == unchecked((int)0xFF00FFFF));

            int lineVersion = doc.Version;
            doc.BeginStroke();
            doc.StrokeLine(1, 2, 30, 12, unchecked((int)0xFF123456), false);
            Check("stroke line bumps version once", doc.Version == lineVersion + 1);
            doc.EndStroke();
            doc.Undo();

            doc.BeginStroke();
            doc.FloodFill(20, 20, unchecked((int)0xFF0000FF));
            doc.EndStroke();
            Check("flood fill covers transparent area", doc[39, 39] == unchecked((int)0xFF0000FF) && doc[0, 10] == unchecked((int)0xFF00FF00));

            doc.Undo();
            Check("undo restores previous state", doc[39, 39] == 0);
            doc.Redo();
            Check("redo reapplies change", doc[39, 39] == unchecked((int)0xFF0000FF));

            // flipping the artwork — the beta's canvas transform
            PixelDoc flip = new PixelDoc();
            flip.BeginStroke();
            flip.SetPixels(0, 3, unchecked((int)0xFFFF0000), false);
            flip.EndStroke();
            flip.Flip(true);
            Check("flip horizontal moves the pixel to the far edge",
                flip[39, 3] == unchecked((int)0xFFFF0000) && flip[0, 3] == 0);
            flip.Flip(true);
            Check("flip horizontal twice restores the artwork", flip[0, 3] == unchecked((int)0xFFFF0000));
            flip.Flip(false);
            Check("flip vertical moves the pixel to the far edge",
                flip[0, 36] == unchecked((int)0xFFFF0000) && flip[0, 3] == 0);
            flip.Undo();
            Check("flip is undoable", flip[0, 3] == unchecked((int)0xFFFF0000));

            // layers (beta): composite, visibility, add/delete, nudge, flatten
            PixelDoc lay = new PixelDoc();
            Check("document starts with a single layer", lay.LayerCount == 1);
            lay.SetPixels(1, 1, unchecked((int)0xFFFF0000), false);
            Check("add layer above the active one", lay.AddLayer() && lay.LayerCount == 2 && lay.ActiveLayer == 1);
            lay.SetPixels(1, 1, unchecked((int)0xFF00FF00), false);
            Check("upper layer covers the one below", lay.CompositeAt(1, 1) == unchecked((int)0xFF00FF00));
            lay.SetLayerVisible(1, false);
            Check("hidden layer drops out of the composite", lay.CompositeAt(1, 1) == unchecked((int)0xFFFF0000));
            lay.SetLayerVisible(1, true);
            lay.ActiveLayer = 0;
            lay.Nudge(1, 0);
            Check("nudge moves only the active layer", lay[2, 1] == unchecked((int)0xFFFF0000) && lay[1, 1] == 0);
            lay.Undo();
            Check("nudge is undoable", lay[1, 1] == unchecked((int)0xFFFF0000));
            lay.Flatten();
            Check("flatten merges to one layer", lay.LayerCount == 1 && lay.CompositeAt(1, 1) == unchecked((int)0xFF00FF00));
            Check("delete keeps the last layer", !lay.DeleteLayer() && lay.LayerCount == 1);

            // layer stack operations (beta): duplicate, merge down, move, and the stroke flag
            PixelDoc ops = new PixelDoc();
            ops.Set(0, 0, unchecked((int)0xFFFF0000));
            ops.AddLayer();
            ops.Set(5, 5, unchecked((int)0xFF00FF00));
            Check("duplicate layer copies the active layer",
                ops.DuplicateLayer() && ops.LayerCount == 3 && ops[5, 5] == unchecked((int)0xFF00FF00));
            ops.Undo();
            Check("duplicate undo drops the copy", ops.LayerCount == 2 && ops.ActiveLayer == 1);
            Check("merge down folds into the layer below",
                ops.MergeDown() && ops.LayerCount == 1 &&
                ops[0, 0] == unchecked((int)0xFFFF0000) && ops[5, 5] == unchecked((int)0xFF00FF00));

            PixelDoc mov = new PixelDoc();
            mov.Set(1, 1, unchecked((int)0xFFFF0000));
            mov.AddLayer();
            mov.Set(1, 1, unchecked((int)0xFF0000FF));
            Check("top layer cannot move further up", !mov.MoveLayer(1));
            Check("move layer down puts it below the other",
                mov.MoveLayer(-1) && mov.ActiveLayer == 0 && mov.CompositeAt(1, 1) == unchecked((int)0xFFFF0000));
            mov.Undo();
            Check("undo restores the layer order",
                mov.ActiveLayer == 1 && mov.CompositeAt(1, 1) == unchecked((int)0xFF0000FF));

            // layer properties (beta): opacity, blend modes, lock, rename and rotation
            PixelDoc prop = new PixelDoc();
            prop.Set(2, 2, unchecked((int)0xFFFF0000));
            prop.SetLayerOpacity(0, 128);
            int faded = prop.CompositeAt(2, 2);
            Check("layer opacity scales the composite alpha", ((faded >> 24) & 0xFF) == 128);
            prop.Undo();
            Check("opacity is undoable", prop.LayerOpacity(0) == 255);

            PixelDoc bl = new PixelDoc();
            bl.Set(1, 1, unchecked((int)0xFF808080));
            bl.AddLayer();
            bl.Set(1, 1, unchecked((int)0xFF808080));
            bl.SetLayerMode(1, PixelDoc.LayerBlend.Multiply);
            Check("multiply blend darkens the stack", ((bl.CompositeAt(1, 1) >> 16) & 0xFF) == 0x40);
            bl.SetLayerMode(1, PixelDoc.LayerBlend.Screen);
            Check("screen blend lightens the stack", ((bl.CompositeAt(1, 1) >> 16) & 0xFF) == 0xC0);
            bl.SetLayerMode(1, PixelDoc.LayerBlend.Normal);

            bl.SetLayerLocked(1, true);
            bl.Set(1, 1, unchecked((int)0xFFFFFFFF));
            Check("locked layer refuses paint", bl[1, 1] == unchecked((int)0xFF808080));
            bl.SetLayerLocked(1, false);
            bl.Set(1, 1, unchecked((int)0xFFFFFFFF));
            Check("unlocked layer accepts paint again", bl[1, 1] == unchecked((int)0xFFFFFFFF));
            Check("layer rename sticks", bl.SetLayerName(1, "Shading") && bl.LayerName(1) == "Shading");

            PixelDoc rot = new PixelDoc();
            rot.Set(0, 0, unchecked((int)0xFFFF0000));
            rot.Rotate(true);
            Check("rotate 90° clockwise maps top-left to top-right",
                rot[PixelDoc.N - 1, 0] == unchecked((int)0xFFFF0000) && rot[0, 0] == 0);
            rot.Undo();
            Check("rotate is undoable", rot[0, 0] == unchecked((int)0xFFFF0000));

            // the layer panel's real hit rectangles, driven through its mouse handlers
            LayersPanel lp = new LayersPanel();
            PixelDoc lpd = new PixelDoc();
            lpd.Set(3, 3, unchecked((int)0xFFFF0000));
            lpd.AddLayer();
            lpd.Set(3, 3, unchecked((int)0xFF0000FF));
            lp.Doc = lpd;
            lp.SetBounds(0, 0, T.P(168), T.P(460));
            Rectangle track = lp.TrackRectDebug;
            lp.ClickAt(track.X + track.Width / 4, track.Y + track.Height / 2);
            int op = lpd.LayerOpacity(1);
            Check("opacity track click sets the layer opacity", op > 40 && op < 100);
            Rectangle blendChip = lp.BlendRectDebug;
            lp.ClickAt(blendChip.X + T.P(6), blendChip.Y + T.P(6));
            Check("blend chip cycles to multiply", lpd.LayerMode(1) == PixelDoc.LayerBlend.Multiply);
            Rectangle rowLock = lp.LockRectDebug(0);
            lp.ClickAt(rowLock.X + 2, rowLock.Y + 2);
            Check("lock icon locks the active layer", lpd.ActiveLocked);
            lp.RenameActiveTo("Shading");
            Check("inline rename commits the typed name", lpd.LayerName(1) == "Shading");

            PixelDoc st = new PixelDoc();
            Check("document is idle outside a stroke", !st.InStroke);
            st.BeginStroke();
            bool duringStroke = st.InStroke;
            st.EndStroke();
            Check("perf mode can detect a live stroke", duringStroke && !st.InStroke);

            // performance mode paints from a cached board; it must look identical to the normal path
            try
            {
                using (Form host = new Form())
                {
                    CanvasView cv = new CanvasView();
                    cv.PerfMode = true;
                    cv.Zoom = 10;
                    cv.Bounds = new Rectangle(0, 0, 440, 440);
                    host.Controls.Add(cv);
                    PixelDoc pc = new PixelDoc();
                    pc.Set(5, 5, unchecked((int)0xFF00A0FF));
                    pc.Set(0, 0, unchecked((int)0xFFFF0000));
                    cv.Doc = pc;
                    using (Bitmap shot = new Bitmap(440, 440))
                    {
                        cv.DrawToBitmap(shot, new Rectangle(0, 0, 440, 440));
                        Rectangle cr = new Rectangle(20, 20, 400, 400);
                        int mid = shot.GetPixel(cr.X + 5 * 10 + 5, cr.Y + 5 * 10 + 5).ToArgb();
                        int corner = shot.GetPixel(cr.X + 5, cr.Y + 5).ToArgb();
                        Check("perf mode draws the artwork over the cached board",
                            mid == unchecked((int)0xFF00A0FF) && corner == unchecked((int)0xFFFF0000));
                    }
                    // the live-stroke fast path must land on exactly the same pixels
                    cv.DebugDrawing(true);
                    using (Bitmap live = new Bitmap(440, 440))
                    {
                        cv.DrawToBitmap(live, new Rectangle(0, 0, 440, 440));
                        Rectangle cr = new Rectangle(20, 20, 400, 400);
                        int lmid = live.GetPixel(cr.X + 5 * 10 + 5, cr.Y + 5 * 10 + 5).ToArgb();
                        int lcorner = live.GetPixel(cr.X + 5, cr.Y + 5).ToArgb();
                        Check("perf mode draws a live stroke from the composite",
                            lmid == unchecked((int)0xFF00A0FF) && lcorner == unchecked((int)0xFFFF0000));
                    }
                    cv.DebugDrawing(false);
                    // the cached board and overlay must land on exactly the same pixels
                    cv.Zoom = 12;
                    cv.ShowGrid = true;
                    cv.Guides = 1;
                    using (Bitmap plain = new Bitmap(440, 440))
                    using (Bitmap fast = new Bitmap(440, 440))
                    {
                        cv.PerfMode = false;
                        cv.DrawToBitmap(plain, new Rectangle(0, 0, 440, 440));
                        cv.PerfMode = true;
                        cv.DrawToBitmap(fast, new Rectangle(0, 0, 440, 440));
                        int diffs = 0;
                        for (int y = 0; y < 440; y++)
                            for (int x = 0; x < 440; x++)
                            {
                                Color a = plain.GetPixel(x, y), b = fast.GetPixel(x, y);
                                // compositing the cached grid can round a channel by one shade
                                if (a.A != b.A || Math.Abs(a.R - b.R) > 1 ||
                                    Math.Abs(a.G - b.G) > 1 || Math.Abs(a.B - b.B) > 1) diffs++;
                            }
                        Check("perf mode matches the normal render" + (diffs == 0 ? "" : " (" + diffs + " pixels differ)"), diffs == 0);
                    }

                    // a patched stroke must land on the same pixels and must not rebuild the
                    // cached canvas: that rebuild was the cost of every mouse move before
                    cv.PerfMode = true;
                    cv.Zoom = 10;
                    cv.ShowGrid = true;
                    cv.Guides = 0;
                    PixelDoc patchDoc = new PixelDoc();
                    patchDoc.Set(5, 5, unchecked((int)0xFF00A0FF));
                    cv.Doc = patchDoc;
                    using (Bitmap warm = new Bitmap(440, 440))
                    {
                        cv.DrawToBitmap(warm, new Rectangle(0, 0, 440, 440));
                        int builds = cv.SurfaceBuilds;
                        for (int i = 0; i < 6; i++)
                        {
                            cv.DebugPaintPixel(6 + i, 5, unchecked((int)0xFFFF8800));
                            cv.DrawToBitmap(warm, new Rectangle(0, 0, 440, 440));
                        }
                        Check("perf patch does not rebuild the canvas", cv.SurfaceBuilds == builds);

                        using (Bitmap fast = new Bitmap(440, 440))
                        {
                            cv.DrawToBitmap(fast, new Rectangle(0, 0, 440, 440));
                            cv.PerfMode = false;
                            using (Bitmap plain = new Bitmap(440, 440))
                            {
                                cv.DrawToBitmap(plain, new Rectangle(0, 0, 440, 440));
                                int off = 0;
                                for (int y = 0; y < 440; y++)
                                    for (int x = 0; x < 440; x++)
                                    {
                                        Color a = plain.GetPixel(x, y), b = fast.GetPixel(x, y);
                                        if (a.A != b.A || Math.Abs(a.R - b.R) > 1 ||
                                            Math.Abs(a.G - b.G) > 1 || Math.Abs(a.B - b.B) > 1) off++;
                                    }
                                Check("perf patch matches the normal render" + (off == 0 ? "" : " (" + off + " pixels differ)"), off == 0);
                            }
                            cv.PerfMode = true;
                        }
                    }
                    cv.PerfMode = false;
                }
            }
            catch (Exception ex)
            {
                Check("perf mode canvas renders (" + ex.GetType().Name + ")", false);
            }

            PixelDoc blend = new PixelDoc();
            blend.Set(0, 0, unchecked((int)0xFFFF0000));
            blend.AddLayer();
            blend.Set(0, 0, unchecked((int)0x80FFFFFF));
            int mixed = blend.CompositeAt(0, 0);
            int mg = (mixed >> 8) & 0xFF, mb = mixed & 0xFF;
            Check("half-transparent layer blends over the lower one",
                ((mixed >> 24) & 0xFF) == 255 && ((mixed >> 16) & 0xFF) > 240 && mg > 100 && mg < 160 && mb > 100 && mb < 160);

            using (Bitmap bmp = doc.ToBitmap())
            {
                Check("bitmap is 40x40", bmp.Width == 40 && bmp.Height == 40);
                Check("bitmap keeps transparency flags", bmp.PixelFormat == PixelFormat.Format32bppArgb);
            }

            // scaling an imported image
            using (Bitmap src = new Bitmap(120, 60))
            {
                using (Graphics g = Graphics.FromImage(src)) g.Clear(Color.Magenta);
                using (Bitmap scaled = PixelDoc.ScaleToCanvas(src))
                    Check("import scales to 40x40", scaled.Width == 40 && scaled.Height == 40);
            }

            // gallery round trip
            int before = Store.List().Count;
            BadgeEntry e = null;
            using (Bitmap bmp = doc.ToBitmap())
                e = Store.Save("Selftest ümlaut \"/\\", "round trip test", bmp);
            Check("badge png written", File.Exists(e.PngPath));
            var list = Store.List();
            Check("gallery lists the badge", list.Count == before + 1);
            BadgeEntry found = null;
            foreach (BadgeEntry x in list) if (x.Id == e.Id) found = x;
            Check("metadata round trips", found != null && found.Name == "Selftest ümlaut \"/\\" && found.Desc == "round trip test");
            Check("thumbnail is 40x40", found != null && found.Thumb != null && found.Thumb.Width == 40);
            Store.Delete(e);
            Check("badge deleted", Store.List().Count == before);

            // draft — keep whatever the user currently has, this only checks the round trip
            string draftPng = null, draftMeta = null;
            try { if (File.Exists(Store.DraftPng)) draftPng = Convert.ToBase64String(File.ReadAllBytes(Store.DraftPng)); } catch { }
            try { if (File.Exists(Store.DraftMeta)) draftMeta = File.ReadAllText(Store.DraftMeta); } catch { }
            try
            {
                using (Bitmap bmp = doc.ToBitmap()) Store.SaveDraft(bmp, "draft name", "draft desc");
                Bitmap d; string dn, dd;
                Check("draft loads", Store.LoadDraft(out d, out dn, out dd) && d != null && d.Width == 40 && dn == "draft name");
                if (d != null) d.Dispose();
            }
            finally
            {
                try
                {
                    if (draftPng != null) File.WriteAllBytes(Store.DraftPng, Convert.FromBase64String(draftPng));
                    else if (File.Exists(Store.DraftPng)) File.Delete(Store.DraftPng);
                    if (draftMeta != null) File.WriteAllText(Store.DraftMeta, draftMeta);
                    else if (File.Exists(Store.DraftMeta)) File.Delete(Store.DraftMeta);
                }
                catch { }
            }

            CheckSettings();
            CheckThemePalette();
            CheckCustomTheme();
            CheckPickers();
            CheckDiscord();
            CheckPortal();

            W("");
            W(string.Format("{0} passed, {1} failed", pass, fail));
            if (logPath != null)
            {
                try { File.WriteAllText(logPath, log.ToString()); }
                catch { }
            }
            return fail == 0;
        }

        static void CheckThemePalette()
        {
            ThemeMode before = T.Mode;
            T.Apply(ThemeMode.Navy);
            Check("navy palette applies a blue dark surface set",
                T.Mode == ThemeMode.Navy && !T.Light && T.Card != T.Bg1 && T.Card.B > T.Card.R && T.Bg1.B > T.Bg1.R);
            T.Apply(before);
            Check("previous palette is restored after the check", T.Mode == before);
            T.Apply(ThemeMode.Light);
            Check("light palette still sets Light without navy", T.Light && T.Mode == ThemeMode.Light);
            T.Apply(before);
        }

        static void CheckCustomTheme()
        {
            ThemeMode before = T.Mode;
            Color surface = Color.FromArgb(26, 24, 52);
            Color accent = Color.FromArgb(255, 140, 90);
            Color ink = Color.FromArgb(240, 236, 255);
            T.ApplyCustom(surface, accent, ink);
            Check("custom palette uses the picked surface, accent and text colours",
                T.Mode == ThemeMode.Custom && T.Card.ToArgb() == surface.ToArgb() &&
                T.Gold.ToArgb() == accent.ToArgb() && T.Text.ToArgb() == ink.ToArgb() && !T.Light);
            Check("custom palette derives a darker page and inset from the surface",
                T.Lum(T.Bg1) < T.Lum(T.Card) && T.Lum(T.Inset) < T.Lum(T.Card));

            T.ApplyCustom(Color.FromArgb(244, 245, 248), accent, Color.FromArgb(24, 24, 30));
            Check("a bright custom surface switches to light-family derivations",
                T.Light && T.Card.R == 244 && T.Mode == ThemeMode.Custom);

            Check("hex colours round trip through AppSettings",
                AppSettings.ToHex(Color.FromArgb(0x12, 0x34, 0x56)) == "#123456" &&
                AppSettings.ParseHex("#123456", Color.Black).ToArgb() == Color.FromArgb(0x12, 0x34, 0x56).ToArgb());
            Check("a malformed hex value falls back safely",
                AppSettings.ParseHex("nope", accent).ToArgb() == accent.ToArgb());

            // Regression: the canvas caches its board bitmap. Dark and navy share the same
            // brightness family, so a light/dark key alone left the old palette on screen.
            T.Apply(ThemeMode.Dark);
            CanvasView cv = new CanvasView();
            cv.Doc = new PixelDoc();
            cv.PerfMode = true;
            cv.Size = new Size(T.P(240), T.P(240));
            cv.CreateControl();
            using (Bitmap a = new Bitmap(cv.Width, cv.Height))
            using (Bitmap b = new Bitmap(cv.Width, cv.Height))
            {
                cv.DrawToBitmap(a, new Rectangle(0, 0, a.Width, a.Height));
                T.Apply(ThemeMode.Navy);
                cv.BackColor = T.Card;
                cv.DrawToBitmap(b, new Rectangle(0, 0, b.Width, b.Height));
                Check("canvas repaints when the palette changes between two dark themes",
                    a.GetPixel(T.P(20), T.P(20)).ToArgb() != b.GetPixel(T.P(20), T.P(20)).ToArgb());
            }
            cv.Dispose();
            T.Apply(before);
        }

        static void CheckSettings()
        {
            string path = Store.SettingsFile;
            string backup = null;
            try { if (File.Exists(path)) backup = File.ReadAllText(path); } catch { }
            try
            {
                AppSettings s = new AppSettings();
                s.Lang = "de";
                s.LightTheme = true;
                s.DiscordEnabled = true;
                s.DiscordAppId = "123456789012345678";
                s.DiscordImageKey = "art";
                s.LayersBeta = true;
                s.PerfMode = true;
                Store.SaveSettings(s);
                AppSettings r = Store.LoadSettings();
                Check("settings round trip", r.Lang == "de" && r.LightTheme && r.DiscordEnabled &&
                    r.DiscordAppId == "123456789012345678" && r.DiscordImageKey == "art" && r.LayersBeta &&
                    r.PerfMode);

                AppSettings g = new AppSettings();
                g.NavyTheme = true;
                Store.SaveSettings(g);
                AppSettings gr = Store.LoadSettings();
                Check("navy theme round trip", gr.NavyTheme && !gr.LightTheme && gr.Mode == ThemeMode.Navy);

                AppSettings cs = new AppSettings();
                cs.CustomTheme = true;
                cs.CustomSurface = "#102030";
                cs.CustomAccent = "#AA5500";
                cs.CustomText = "#EEEEEE";
                Store.SaveSettings(cs);
                AppSettings cr = Store.LoadSettings();
                Check("custom theme round trip keeps the three colours",
                    cr.CustomTheme && cr.Mode == ThemeMode.Custom &&
                    cr.SurfaceColor.ToArgb() == Color.FromArgb(0x10, 0x20, 0x30).ToArgb() &&
                    cr.AccentColor.ToArgb() == Color.FromArgb(0xAA, 0x55, 0x00).ToArgb() &&
                    cr.TextColor.ToArgb() == Color.FromArgb(0xEE, 0xEE, 0xEE).ToArgb());

                AppSettings ps = new AppSettings();
                ps.CustomTheme = true;
                ps.Presets.Add(new ThemePreset("Deep \"Ocean\"", Color.FromArgb(9, 18, 34), Color.FromArgb(34, 211, 238), Color.FromArgb(230, 239, 252)));
                ps.Presets.Add(new ThemePreset("Sand", Color.FromArgb(244, 240, 232), Color.FromArgb(196, 120, 40), Color.FromArgb(38, 34, 30)));
                Store.SaveSettings(ps);
                AppSettings pr = Store.LoadSettings();
                Check("custom presets round trip with names and colours",
                    pr.Presets.Count == 2 && pr.Presets[0].Name == "Deep \"Ocean\"" &&
                    pr.Presets[0].SurfaceColor.ToArgb() == Color.FromArgb(9, 18, 34).ToArgb() &&
                    pr.Presets[0].AccentColor.ToArgb() == Color.FromArgb(34, 211, 238).ToArgb() &&
                    pr.Presets[1].Name == "Sand" &&
                    pr.Presets[1].TextColor.ToArgb() == Color.FromArgb(38, 34, 30).ToArgb());
                Check("presets clone with the settings instead of sharing the list",
                    ps.Clone().Presets[1].Name == "Sand" && !object.ReferenceEquals(ps.Presets, ps.Clone().Presets));
                File.WriteAllText(path, "{\n  \"presets\": [ { \"name\": \"\", \"surface\": \"#000000\" }, { \"name\": \"Broken\" } ]\n}\n");
                Check("malformed preset entries are skipped", Store.LoadSettings().Presets.Count == 0);

                List<ThemePreset> order = new List<ThemePreset>();
                order.Add(new ThemePreset("Alpha", Color.Black, Color.White, Color.White));
                order.Add(new ThemePreset("Beta", Color.Black, Color.White, Color.White));
                order.Add(new ThemePreset("Gamma", Color.Black, Color.White, Color.White));
                PresetOps.Move(order, 2, 0);
                Check("preset reorder moves a chip to the front",
                    order[0].Name == "Gamma" && order[1].Name == "Alpha" && order[2].Name == "Beta");
                PresetOps.Move(order, 0, 2);
                Check("preset reorder moves a chip back to the end",
                    order[0].Name == "Alpha" && order[1].Name == "Beta" && order[2].Name == "Gamma");
                PresetOps.Move(order, 1, 1);
                PresetOps.Move(order, 9, 0);
                Check("preset reorder ignores same-slot and out-of-range moves",
                    order.Count == 3 && order[0].Name == "Alpha" && order[1].Name == "Beta" && order[2].Name == "Gamma");
                Check("preset rename adds a suffix for another preset's name, but keeps its own",
                    PresetOps.UniqueName(order, "beta", 0) == "beta 2" &&
                    PresetOps.UniqueName(order, "Gamma", 2) == "Gamma");
                Check("preset names cap at 24 characters",
                    PresetOps.UniqueName(order, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 2) ==
                    "ABCDEFGHIJKLMNOPQRSTUVWX");
                Check("a blank preset rename falls back to a placeholder",
                    PresetOps.UniqueName(order, "   ", 2) == "Preset");

                File.WriteAllText(path, "{\n  \"theme\": \"glass\"\n}\n");
                AppSettings legacy = Store.LoadSettings();
                Check("retired glass setting loads as navy", legacy.NavyTheme && legacy.Mode == ThemeMode.Navy);

                AppSettings modes = new AppSettings();
                bool darkDefault = modes.Mode == ThemeMode.Dark;
                modes.LightTheme = true;
                bool lightMode = modes.Mode == ThemeMode.Light;
                modes.NavyTheme = true;
                Check("theme mode resolves dark, light and navy", darkDefault && lightMode && modes.Mode == ThemeMode.Navy);
            }
            finally
            {
                try
                {
                    if (backup != null) File.WriteAllText(path, backup);
                    else if (File.Exists(path)) File.Delete(path);
                }
                catch { }
            }
        }

        /// <summary>Covers the colour maths and the two painters the pickers share.</summary>
        static void CheckPickers()
        {
            Color[] probes = new Color[]
            {
                Color.FromArgb(255, 17, 226, 226), Color.FromArgb(255, 245, 166, 35),
                Color.FromArgb(255, 90, 30, 120), Color.FromArgb(255, 255, 255, 255),
                Color.FromArgb(255, 12, 15, 19)
            };
            bool round = true;
            foreach (Color c in probes)
            {
                float ph, ps, pv;
                T.RgbToHsv(c, out ph, out ps, out pv);
                Color back = T.HsvToRgb(ph, ps, pv);
                if (Math.Abs(back.R - c.R) > 1 || Math.Abs(back.G - c.G) > 1 || Math.Abs(back.B - c.B) > 1)
                    round = false;
            }
            Check("hsv round-trip keeps every probe colour", round);
            Check("hsv corner hues land exactly on green and blue",
                T.HsvToRgb(120f, 1f, 1f).ToArgb() == Color.FromArgb(255, 0, 255, 0).ToArgb() &&
                T.HsvToRgb(240f, 1f, 1f).ToArgb() == Color.FromArgb(255, 0, 0, 255).ToArgb());
            Check("a grey keeps no hue of its own",
                T.HsvToRgb(210f, 0f, 1f).ToArgb() == Color.White.ToArgb());

            // the square and the rail are the shared painters behind the colour sheet and designer
            using (Bitmap bmp = new Bitmap(120, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(255, 5, 5, 5));
                    // the knob only marks sat/val, so the two are parked in the middle to keep the
                    // corners of the gradient clean for sampling
                    T.PaintSvSquare(g, new RectangleF(0, 0, 100, 100), 0f, 0.5f, 0.5f, Color.Red);
                    T.PaintHueRail(g, new RectangleF(104, 0, 14, 100), 90f);
                }
                // left edge is saturation 0, the right edge the pure hue, and value 0 flattens
                // the bottom row to black
                Color svWhite = bmp.GetPixel(6, 6);
                Color svHue = bmp.GetPixel(96, 6);
                Color svDark = bmp.GetPixel(6, 96);
                Color railTop = bmp.GetPixel(111, 10);
                Color railMid = bmp.GetPixel(111, 50);
                Check("sv square paints white at saturation 0 and the hue at saturation 1",
                    svWhite.R > 220 && svWhite.G > 220 && svWhite.B > 220 &&
                    svHue.R > 200 && svHue.G < 60 && svHue.B < 60);
                Check("sv square reaches black at the bottom",
                    svDark.R < 60 && svDark.G < 60 && svDark.B < 60);
                Check("hue rail runs warm at the top and cool in the middle",
                    railTop.R > railTop.G && railTop.R > railTop.B &&
                    railMid.B > railMid.R && railMid.G > railMid.R);
            }
        }

        static void CheckDiscord()
        {
            Check("discord id validation",
                DiscordRpc.IsValidAppId("123456789012345678") &&
                !DiscordRpc.IsValidAppId("123") &&
                !DiscordRpc.IsValidAppId("12345678901234567x") &&
                !DiscordRpc.IsValidAppId(null));
            Check("custom discord application wins over the built-in one",
                DiscordRpc.ResolveAppId("123456789012345678") == "123456789012345678");
            Check("a build without an application id resolves to none",
                DiscordRpc.HasBuiltInAppId || DiscordRpc.ResolveAppId(" ") == "");
            Check("the built-in application id is valid when set",
                !DiscordRpc.HasBuiltInAppId || DiscordRpc.IsValidAppId(DiscordRpc.BuiltInAppId));
            Check("rich presence defaults on only with a built-in application",
                new AppSettings().DiscordEnabled == DiscordRpc.HasBuiltInAppId);

            string json = DiscordRpc.BuildActivityJson(4242, 1700000000, "Designing \"badges\"", "Golden Crown", "logo");
            Check("discord json: set_activity", json.IndexOf("\"cmd\":\"SET_ACTIVITY\"", StringComparison.Ordinal) >= 0);
            Check("discord json: pid", json.IndexOf("\"pid\":4242", StringComparison.Ordinal) >= 0);
            Check("discord json: escapes quotes", json.IndexOf("Designing \\\"badges\\\"", StringComparison.Ordinal) >= 0);
            Check("discord json: large image", json.IndexOf("\"large_image\":\"logo\"", StringComparison.Ordinal) >= 0);
            Check("discord json: timestamp", json.IndexOf("\"timestamps\":{\"start\":1700000000}", StringComparison.Ordinal) >= 0);

            try
            {
                using (NamedPipeServerStream srv = new NamedPipeServerStream("discord-ipc-9", PipeDirection.InOut, 1,
                           PipeTransmissionMode.Byte, PipeOptions.None, 65536, 65536))
                {
                    string handshake = null, activity = null;
                    Thread server = new Thread(delegate()
                    {
                        try
                        {
                            srv.WaitForConnection();
                            BinaryReader r = new BinaryReader(srv, new UTF8Encoding(false));
                            BinaryWriter w = new BinaryWriter(srv, new UTF8Encoding(false));
                            handshake = ReadJsonFrame(r);
                            byte[] ready = Encoding.UTF8.GetBytes("{\"cmd\":\"DISPATCH\",\"evt\":\"READY\",\"data\":{\"v\":1}}");
                            w.Write(1);
                            w.Write(ready.Length);
                            w.Write(ready);
                            w.Flush();
                            activity = ReadJsonFrame(r);
                        }
                        catch { }
                    });
                    server.IsBackground = true;
                    server.Start();

                    DiscordRpc.TestPipeIndex = 9;
                    DiscordRpc.Start("123456789012345678", "logo");
                    bool joined = server.Join(5000);
                    Check("discord ipc: client connected", joined && handshake != null);
                    Check("discord ipc: handshake payload", handshake != null &&
                        handshake.IndexOf("\"client_id\":\"123456789012345678\"", StringComparison.Ordinal) >= 0 &&
                        handshake.IndexOf("\"v\":1", StringComparison.Ordinal) >= 0);
                    Check("discord ipc: activity payload", activity != null &&
                        activity.IndexOf("\"cmd\":\"SET_ACTIVITY\"", StringComparison.Ordinal) >= 0 &&
                        activity.IndexOf("\"large_image\":\"logo\"", StringComparison.Ordinal) >= 0);
                    DiscordRpc.Stop();
                    DiscordRpc.TestPipeIndex = -1;
                }
            }
            catch (Exception ex)
            {
                W("  [skip] discord ipc test: " + ex.Message);
            }
        }

        static void CheckPortal()
        {
            const string list = "https://discord.com/developers/applications";
            Check("portal url targets the app's art assets",
                SettingsForm.PortalUrl(" 123456789012345678 ") == list + "/123456789012345678/rich-presence/assets");
            Check("portal url falls back to the application list",
                SettingsForm.PortalUrl("") == list && SettingsForm.PortalUrl(null) == list);
            Check("portal url rejects malformed ids",
                SettingsForm.PortalUrl("12345") == list && SettingsForm.PortalUrl("12345678901234567x") == list);
        }

        static string ReadJsonFrame(BinaryReader r)
        {
            r.ReadInt32();
            int len = r.ReadInt32();
            byte[] payload = r.ReadBytes(len);
            return Encoding.UTF8.GetString(payload);
        }

        static void Check(string what, bool ok)
        {
            if (ok) { pass++; W("  [ok]   " + what); }
            else { fail++; W("  [FAIL] " + what); }
        }
    }
}
