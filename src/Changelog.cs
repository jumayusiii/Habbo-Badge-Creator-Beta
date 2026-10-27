using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>Version, credit and the release history shown in the changelog.</summary>
    internal static class AppInfo
    {
        public const string Version = "1.4.5-beta.21";
        public const string Build = "2026-10-27.21";
        public const string Date = "27 October 2026";
        public const string Credit = "Habbo Badge Creator © 2026 Jumayusi — Not affiliated with Habbo or Sulake";

        public static readonly ReleaseNote[] Releases = new ReleaseNote[]
        {
            new ReleaseNote("1.4.5-beta.21", "27 Oct 2026",
                "Themed colour picker",
                "Farbwähler im App-Design",
                new string[]
                {
                    "Windows' colour dialog is gone — it was the one window that ignored the app, coming up in pale system grey with a stock colour wheel no matter which palette was active. Colours are now picked in the app's own sheet (beta).",
                    "The sheet is built like the custom theme designer: one saturation/value square with the hue rail beside it, a hex field, a live R/G/B readout, a before/after pair and the editor's palette. Every tone comes from the active palette, so it fits Dark, Light, Navy and Custom.",
                    "Picking previews on the badge behind the sheet, so a colour can be judged in place: the colour chip, the paint colour and the palette selection follow the square while it is dragged. Use colour commits it; Cancel, Esc, the close button or a click on BEFORE puts the previous colour back.",
                    "The square and the hue rail now come from one shared painter that the colour sheet and the theme designer both call, so the two pickers can no longer drift apart. A render of the theme designer before and after the move is pixel-identical.",
                    "Greys keep the hue you picked instead of snapping the rail back to red, the hex field parses on Enter or when it loses focus, and the sheet fades in and out like Settings and Changelog.",
                    "Six new self-test checks cover the colour maths (HSV round trip, corner hues, greys) and both painters (white at saturation 0, the pure hue at saturation 1, black along the bottom, a warm-to-cool rail)."
                },
                new string[]
                {
                    "Der Windows-Farbdialog ist weg – er war das einzige Fenster, das die App ignorierte: helles Systemgrau mit Standard-Farbrad, egal welches Design aktiv war. Farben werden jetzt im eigenen Fenster der App gewählt (Beta).",
                    "Das Fenster ist wie der Design-Designer aufgebaut: ein Sättigungs-/Helligkeitsquadrat mit Farbtonleiste daneben, Hex-Feld, Live-R/G/B-Anzeige, Vorher/Nachher-Felder und die Palette des Editors. Alle Töne stammen aus der aktiven Palette und passen damit zu Dunkel, Hell, Marineblau und Eigenes.",
                    "Die Auswahl erscheint live auf dem Badge dahinter, eine Farbe lässt sich also direkt beurteilen: Farbfeld, Malfarbe und Palettenauswahl folgen dem Quadrat schon beim Ziehen. „Farbe übernehmen“ setzt sie ein; Abbrechen, Esc, das Schließen-Kreuz oder ein Klick auf VORHER stellen die vorherige Farbe wieder her.",
                    "Quadrat und Farbtonleiste stammen jetzt aus einer gemeinsamen Zeichenroutine, die Farbwähler und Design-Designer beide aufrufen – die beiden Wähler können nicht mehr auseinanderlaufen. Ein Bild des Designers vor und nach dem Verschieben ist pixelgleich.",
                    "Grautöne behalten den gewählten Farbton, statt die Leiste auf Rot zurückzusetzen; das Hex-Feld liest bei Enter oder Fokusverlust, und das Fenster blendet sich wie Einstellungen und Änderungen ein und aus.",
                    "Sechs neue Selbsttest-Prüfungen decken die Farbmathematik (HSV-Hin und Rückweg, Eckfarbtöne, Graustufen) und beide Zeichenroutinen ab (Weiß bei Sättigung 0, reiner Farbton bei Sättigung 1, Schwarz am unteren Rand, warme bis kühle Leiste)."
                }),
            new ReleaseNote("1.4.5-beta.20", "27 Oct 2026",
                "Settings and Changelog fade in and out",
                "Einstellungen und Änderungen blenden weich ein und aus",
                new string[]
                {
                    "Settings and Changelog now fade in over the editor when they open and fade back out when they close: 150 ms in, 120 ms out, eased at both ends instead of a linear ramp.",
                    "The fade is one window-wide blend, so the rounded corners, the custom chrome and every control arrive together. Both sheets are fully painted before they appear, so the animation never hides a blank body.",
                    "Every way of closing uses the same fade-out — X, Esc, Space, Enter, Cancel and Save — and a second click during it no longer cuts the animation short. Closing while the sheet is still fading in continues from the current opacity instead of flashing to full first.",
                    "Windows' animation setting is respected: with visual effects turned off the sheets open and close instantly. The fade is a window attribute only, so a rendered sheet is byte-identical with and without it — the animation changes nothing about how the sheet is drawn."
                },
                new string[]
                {
                    "Einstellungen und Änderungen blenden sich beim Öffnen über dem Editor ein und beim Schließen wieder aus: 150 ms hinein, 120 ms hinaus, an beiden Enden weich statt linear.",
                    "Das Ein- und Ausblenden erfasst das ganze Fenster, abgerundete Ecken, eigener Rahmen und alle Bedienelemente kommen gemeinsam. Beide Fenster sind vor dem Erscheinen fertig gezeichnet, die Animation verdeckt also nie einen leeren Rumpf.",
                    "Jeder Weg zum Schließen nutzt dasselbe Ausblenden – X, Esc, Leertaste, Enter, Abbrechen und Speichern –, und ein zweiter Klick darauf schneidet die Animation nicht mehr ab. Ein Schließen während des Einblendens läuft von der aktuellen Deckkraft weiter, statt kurz auf voll zu springen.",
                    "Die Windows-Einstellung für Animationen wird beachtet: Sind visuelle Effekte aus, öffnen und schließen die Fenster sofort. Das Ein- und Ausblenden ist reine Fenstereigenschaft – ein gerendertes Fenster ist mit und ohne Animation bytegleich, an der Darstellung ändert sich nichts."
                }),
            new ReleaseNote("1.4.5-beta.19", "27 Oct 2026",
                "Release build from the pipeline",
                "Release-Build aus der Pipeline",
                new string[]
                {
                    "This build carries no feature changes: it is the 1.4.5-beta.18 app, rebuilt and published by the release workflow that runs on a version tag.",
                    "The tag names the version, the run compiles and self-tests the app on a clean Windows runner, and the packages are uploaded from that same run — so the download matches the tag.",
                    "The Changelog fix from beta.18 is included unchanged, and the app now reports 1.4.5-beta.19."
                },
                new string[]
                {
                    "Dieser Build enthält keine funktionalen Änderungen: Es ist die App aus 1.4.5-beta.18, neu gebaut und veröffentlicht durch den Release-Workflow, der bei einem Versions-Tag läuft.",
                    "Der Tag benennt die Version, der Lauf kompiliert die App und führt den Selbsttest auf einem frischen Windows-Runner aus; die Pakete stammen aus genau diesem Lauf – der Download entspricht damit dem Tag.",
                    "Die Changelog-Korrektur aus beta.18 ist unverändert enthalten; die App meldet jetzt 1.4.5-beta.19."
                }),
            new ReleaseNote("1.4.5-beta.18", "27 Oct 2026",
                "Changelog opens complete",
                "Changelog öffnet vollständig",
                new string[]
                {
                    "Clicking Changelog used to lay out the entire release history inside the first paint, so the sheet sat there with a blank body for seconds until the render finished.",
                    "It now renders one entry at a time, and the entries the sheet opens on are ready before the window appears: the first frame is complete and the first paint takes about 16 ms instead of about a second.",
                    "Scrolling still copies a cache — the next entry above and below the viewport is rendered ahead, so wheel and track scrolling stay smooth.",
                    "The sheet itself is unchanged: a screenshot before and after the fix is pixel-identical."
                },
                new string[]
                {
                    "Ein Klick auf Änderungen hat bisher die gesamte Versionshistorie im ersten Zeichnen aufgebaut; der Rumpf des Fensters blieb dabei sekundenlang leer.",
                    "Jetzt wird jeder Eintrag einzeln gezeichnet, und die Einträge, mit denen das Fenster öffnet, entstehen schon vor dem Erscheinen: Das erste Bild ist vollständig, und das erste Zeichnen dauert etwa 16 ms statt etwa einer Sekunde.",
                    "Beim Scrollen wird weiterhin ein Cache kopiert – der nächste Eintrag über und unter dem sichtbaren Bereich wird vorab gezeichnet, damit Mausrad und Balken flüssig bleiben.",
                    "Die Darstellung selbst bleibt gleich: ein Bildschirmfoto vor und nach der Korrektur ist pixelgleich."
                }),
            new ReleaseNote("1.4.5-beta.17", "26 Oct 2026",
                "Lighter Performance mode, Rich Presence out of the box",
                "Leichterer Leistungsmodus, Rich Presence ohne Einrichtung",
                new string[]
                {
                    "Performance mode now keeps the finished canvas in a cache. Repainting the same content is a copy of the visible area instead of redrawing the board, checkerboard, artwork, grid and rounded frame: about 1 ms instead of about 7 ms in measurement.",
                    "A stroke patches only the cells it crosses into that cache and asks Windows for exactly those cells, not their bounding box — a fast flick across the canvas now costs the same as a slow drag.",
                    "Measured on a 740 × 660 canvas at 14× zoom: one mouse move while painting takes about 0.5–1 ms with Performance mode and 7–10 ms without, roughly ten times less work. The zoomed canvas is no longer rebuilt on every move.",
                    "The fast path is compared against the normal render pixel by pixel in the self-test suite, and a test asserts that a stroke does not rebuild the cached canvas.",
                    "Rich Presence can now ship with the app's own Discord application: one Application ID built into the release is used by everyone, so users no longer create a Discord application themselves. A personal ID entered under Settings still wins over the built-in one.",
                    "A build that carries the application switches Rich Presence on for a new install; a build without one says so in Settings instead of asking every user to set one up."
                },
                new string[]
                {
                    "Der Leistungsmodus hält die fertige Zeichenfläche jetzt im Cache. Dasselbe Bild neu zu zeichnen kopiert nur den sichtbaren Bereich, statt Brett, Schachbrett, Grafik, Raster und runden Rahmen neu zu zeichnen: rund 1 ms statt rund 7 ms im Test.",
                    "Ein Strich schreibt nur die Zellen in diesen Cache, die er kreuzt, und fordert von Windows genau diese Zellen an – nicht ihr umschließendes Rechteck. Ein schneller Zug über die Fläche kostet damit so viel wie ein langsamer.",
                    "Auf einer 740 × 660-Fläche bei 14× Zoom gemessen: eine Mausbewegung beim Malen dauert mit Leistungsmodus etwa 0,5–1 ms und ohne 7–10 ms, also rund zehnmal weniger Arbeit. Der zoomierte Canvas wird nicht mehr bei jeder Bewegung neu gebaut.",
                    "Der schnelle Pfad wird in den Selbsttests Pixel für Pixel mit dem normalen Zeichnen verglichen; ein Test prüft zusätzlich, dass ein Strich den Cache nicht neu aufbaut.",
                    "Rich Presence kann jetzt mit der eigenen Discord-Anwendung der App ausgeliefert werden: eine im Release eingebaute Application-ID gilt für alle, sodass Nutzer keine eigene Discord-Anwendung mehr erstellen müssen. Eine eigene ID in den Einstellungen hat weiterhin Vorrang.",
                    "Ein Build mit eigener Anwendung schaltet Rich Presence bei einer Neuinstallation ein; ein Build ohne sie sagt das in den Einstellungen, statt von jedem Nutzer eine Einrichtung zu verlangen."
                }),
            new ReleaseNote("1.4.5-beta.16", "26 Oct 2026",
                "Smooth changelog, faster Performance mode",
                "Ruhiger Changelog, schnellerer Leistungsmodus",
                new string[]
                {
                    "The changelog no longer re-wraps every entry on each scroll step. The whole sheet is measured and drawn once into a cache, and each frame is a crop of it: a full scroll frame dropped from about 110 ms to about 3 ms in measurement.",
                    "Wheel and track scrolling now ease into place over a dozen frames instead of jumping, and dragging the thumb still follows the pointer 1:1.",
                    "Performance mode caches the grid and guides in their own overlay and repaints only the cells a stroke actually crosses, instead of the whole bounding box of a fast mouse move.",
                    "While a stroke runs, the zoomed canvas cache is no longer rebuilt on every mouse move: the 40 × 40 composite is painted straight to the canvas, and the toolbar, layer panel and large preview wait for the end of the stroke.",
                    "Measured before and after on the same 740 × 660 canvas at 14× zoom: one moved cell went from 1.55 ms to 0.4 ms of paint time, and a changelog scroll frame from 111 ms to 2.8 ms.",
                    "The full self-test suite also compares the cached render against the normal render pixel by pixel, so the fast path cannot drift visually."
                },
                new string[]
                {
                    "Der Changelog baut nicht mehr bei jedem Scroll-Schritt den gesamten Text neu auf. Das Blatt wird einmal vermessen und in einen Cache gezeichnet, jedes Bild ist nur noch ein Ausschnitt daraus: ein volles Scroll-Bild fiel im Test von rund 110 ms auf etwa 3 ms.",
                    "Mausrad und Klick in die Leiste gleiten jetzt über ein Dutzend Bilder an die Zielposition, statt zu springen; der Griff folgt der Maus weiterhin 1:1.",
                    "Der Leistungsmodus legt Raster und Hilfslinien in eine eigene Ebene im Cache und zeichnet nur noch die Pixel neu, die ein Strich wirklich kreuzt – nicht mehr das ganze Rechteck eines schnellen Mauszugs.",
                    "Während eines Strichs wird der zoomierte Canvas-Cache nicht mehr bei jeder Mausbewegung neu gebaut: Das 40 × 40-Composite geht direkt auf die Fläche, und Werkzeugleiste, Ebenen-Panel und große Vorschau warten auf das Ende des Strichs.",
                    "Vorher/Nachher auf derselben 740 × 660-Fläche bei 14× Zoom gemessen: ein bewegtes Pixel sank von 1,55 ms auf 0,4 ms Zeichenzeit, ein Changelog-Scrollbild von 111 ms auf 2,8 ms.",
                    "Die Selbsttests vergleichen den Cache zusätzlich Pixel für Pixel mit dem normalen Zeichnen, damit der schnelle Pfad optisch nicht abweicht."
                }),
            new ReleaseNote("1.4.5-beta.15", "26 Oct 2026",
                "Changelog scrolls with the mouse",
                "Änderungen lassen sich mit der Maus scrollen",
                new string[]
                {
                    "The bar on the right of the Changelog sheet is a real scrollbar now: drag the handle to move through the notes, or click the track above or below it to page up and down.",
                    "The handle grows on hover so it is easy to grab, and it stops cleanly at the first and last entry instead of running past the ends.",
                    "The mouse wheel still scrolls the notes, and it now scrolls exactly once per notch instead of being handled twice.",
                    "Checked in the running app with real mouse input: drag, track clicks, wheel and dragging past the end."
                },
                new string[]
                {
                    "Der Balken rechts im Änderungsfenster ist jetzt eine echte Bildlaufleiste: den Griff ziehen, um durch die Einträge zu fahren, oder über oder unter ihm in die Leiste klicken, um seitenweise zu blättern.",
                    "Der Griff wird beim Überfahren breiter und lässt sich so leichter greifen; am ersten und letzten Eintrag hält er sauber an, statt über das Ende hinauszulaufen.",
                    "Das Mausrad scrollt weiterhin, jetzt aber genau eine Raste pro Schritt statt doppelt.",
                    "In der laufenden App mit echter Maus geprüft: Ziehen, Klicks in die Leiste, Mausrad und Ziehen über das Ende hinaus."
                }),
            new ReleaseNote("1.4.5-beta.14", "26 Oct 2026",
                "Public beta release",
                "Öffentliche Beta",
                new string[]
                {
                    "This build is ready for wider testing. It stays on the beta channel and keeps its own data folder, so it can run next to the stable app without sharing badges or settings.",
                    "Release polish: the executable now carries proper Windows file details (product, version and copyright), the beta label no longer calls this a private copy, and the download ships with documentation, a license and SHA-256 checksums.",
                    "No drawing or theme behaviour changed in this build — it is the beta.13 feature set, checked by the full self-test suite."
                },
                new string[]
                {
                    "Dieser Build ist bereit für breiteres Testen. Er bleibt im Beta-Kanal und nutzt einen eigenen Datenordner, läuft also neben der stabilen App, ohne Badges oder Einstellungen zu teilen.",
                    "Release-Politur: Die EXE hat jetzt vollständige Windows-Dateiinformationen (Produkt, Version und Copyright), die Beta-Markierung nennt sie nicht mehr private Testkopie, und der Download enthält Dokumentation, Lizenz und SHA-256-Prüfsummen.",
                    "Zeichnen und Design sind in diesem Build unverändert – er entspricht beta.13 und ist mit der vollständigen Selbsttest-Suite geprüft."
                }),
            new ReleaseNote("1.4.5-beta.13", "26 Oct 2026",
                "Preset reorder and rename",
                "Vorlagen sortieren und umbenennen",
                new string[]
                {
                    "Saved custom-theme palettes can be reorganised: drag a preset chip onto another slot to move it there. A dashed gap is left behind and a gold outline marks the drop, and the new order is saved together with the active palette.",
                    "Double-click a preset chip to rename it in place. Its three colours stay exactly as they were — only the label changes — and a name another preset already uses gets a numeric suffix. Esc cancels.",
                    "The preset count moved next to the PRESETS heading, and a small hint at the right of the row explains both gestures.",
                    "Checked with real mouse and keyboard input at 100%, 125% and 150% layout scale: Settings opened at each size, Dark → Navy → Dark switched, a preset was dragged to the front and renamed, its palette recalled and saved, then the app restarted to confirm the order and name in settings.json. No clipped text or missed clicks; 150% exercised the compact short-screen layout.",
                    "The debug --scale switch now scales fonts as well as geometry, so its 125% and 150% renders match a real display DPI."
                },
                new string[]
                {
                    "Gespeicherte eigene Paletten lassen sich jetzt umsortieren: eine Vorlagen-Kachel auf einen anderen Platz ziehen. Eine gestrichelte Lücke bleibt zurück und eine goldene Kontur zeigt das Ziel; die neue Reihenfolge wird mit der aktiven Palette gespeichert.",
                    "Doppelklick auf eine Vorlage benennt sie direkt um. Die drei Farben bleiben unverändert – nur der Name ändert sich – und ein bereits vergebener Name erhält eine Nummer. Esc bricht ab.",
                    "Die Anzahl steht jetzt neben der Überschrift VORLAGEN, und ein kleiner Hinweis rechts in der Zeile erklärt beide Gesten.",
                    "Mit echter Maus- und Tastatureingabe bei 100 %, 125 % und 150 % geprüft: Einstellungen bei jeder Größe geöffnet, Dunkel → Marineblau → Dunkel gewechselt, eine Vorlage nach vorne gezogen und umbenannt, ihre Palette geladen und gespeichert, dann die App neu gestartet, um Reihenfolge und Namen in settings.json zu bestätigen. Keine abgeschnittenen Texte oder verfehlten Klicks; bei 150 % lief die kompakte Ansicht für kleine Bildschirme.",
                    "Der Debug-Schalter --scale skaliert jetzt auch die Schriften, sodass seine 125-%- und 150-%-Bilder einer echten Bildschirm-DPI entsprechen."
                }),
            new ReleaseNote("1.4.5-beta.12", "26 Oct 2026",
                "Custom theme presets",
                "Vorlagen für eigene Designs",
                new string[]
                {
                    "The custom theme designer now has a PRESETS row. Save the current Surface, Accent and Text under a name, keep up to five palettes and click a chip to recall one instantly.",
                    "Saving under an existing name overwrites that preset, the small × on a chip deletes it, and the count on the right shows how many of the five slots are used. Presets are part of settings.json, so they survive a restart together with the active palette.",
                    "Fixed: the designer's own buttons, hex box and name box kept the previous palette while you dragged a colour, which left rectangles of the old theme around the sheet. The whole sheet chrome now follows the live preview.",
                    "Verified in the running app: Dark → Navy → Dark switching with Performance mode on, a colour picked by hand, a preset saved, recalled and removed, and the result checked again after a restart."
                },
                new string[]
                {
                    "Der Designer für eigene Designs hat jetzt eine Zeile VORLAGEN. Aktuelle Fläche, Akzent und Textfarbe unter einem Namen sichern, bis zu fünf Paletten behalten und per Klick auf eine Kachel sofort wiederherstellen.",
                    "Ein bestehender Name wird überschrieben, das kleine × auf einer Kachel löscht sie, und rechts steht, wie viele der fünf Plätze belegt sind. Vorlagen liegen in settings.json und überleben zusammen mit der aktiven Palette einen Neustart.",
                    "Behoben: Die eigenen Schaltflächen, das Hex-Feld und das Namensfeld des Designers behielten beim Ziehen einer Farbe die alte Palette und hinterließen Rechtecke des vorherigen Themes. Die gesamte Fläche folgt jetzt der Live-Vorschau.",
                    "In der laufenden App geprüft: Wechsel Dunkel → Marineblau → Dunkel mit aktivem Leistungsmodus, eine von Hand gewählte Farbe, eine gesicherte, wieder geladene und gelöschte Vorlage sowie das Ergebnis nach einem Neustart."
                }),
            new ReleaseNote("1.4.5-beta.11", "26 Oct 2026",
                "Custom theme (beta) and canvas fix",
                "Eigenes Design (Beta) und Zeichenflächen-Fix",
                new string[]
                {
                    "Fixed: switching between Dark and Navy — or between two custom palettes — left the pixel area painted in the previous colours until another theme toggle forced a repaint. The canvas cache was keyed on light/dark only; it now keys on the actual palette, so the board, checkerboard and frame follow every change at once.",
                    "Beta: Settings → Appearance has a fourth tile, Custom. It opens a colour designer with a saturation/value square, a hue rail, a hex box and a 24-colour palette.",
                    "Pick a Surface (cards and sheets), an Accent (buttons, selection, links) and a Text colour. The page, recesses, lines, muted text and checkerboard are all derived from those three, and a bright surface switches the whole palette to the light-family derivations automatically.",
                    "Every change previews live on the app behind the sheet; Use theme applies it, Cancel reverts. The three colours and theme: custom are saved in settings.json and restored on the next start, including the boot splash.",
                    "--theme custom renders the palette for screenshots without touching the saved choice."
                },
                new string[]
                {
                    "Behoben: Beim Wechsel zwischen Dunkel und Marineblau – oder zwischen zwei eigenen Paletten – blieb die Zeichenfläche in den alten Farben, bis ein weiterer Themenwechsel neu zeichnete. Der Cache der Zeichenfläche kannte nur hell/dunkel; jetzt hängt er an der echten Palette, sodass Fläche, Schachbrett und Rahmen sofort folgen.",
                    "Beta: In den Einstellungen unter Darstellung gibt es eine vierte Kachel, Eigene. Sie öffnet einen Farb-Designer mit Sättigungs-/Helligkeitsfeld, Farbton-Regler, Hex-Feld und einer Palette aus 24 Farben.",
                    "Fläche (Karten und Fenster), Akzent (Schaltflächen, Auswahl, Links) und Textfarbe wählen. Seite, Vertiefungen, Linien, gedämpfter Text und Schachbrett werden daraus abgeleitet; eine helle Fläche schaltet automatisch auf die hellen Ableitungen um.",
                    "Jede Änderung erscheint live in der App hinter dem Fenster; Design übernehmen gilt, Abbrechen verwirft. Die drei Farben und theme: custom werden in settings.json gespeichert und beim nächsten Start inklusive Startbildschirm wiederhergestellt.",
                    "Mit --theme custom lässt sich die Palette für Screenshots rendern, ohne die gespeicherte Auswahl zu ändern."
                }),
            new ReleaseNote("1.4.5-beta.10", "26 Oct 2026",
                "Layers overhaul, rotate and copy",
                "Ebenen-Ausbau, Drehen und Kopieren",
                new string[]
                {
                    "The layer panel was rebuilt (beta): blend mode and opacity sit under the header like a real layers panel, every row shows its name, a live opacity percentage and a lock, and the stack actions stay pinned to the bottom.",
                    "Double-click a layer to rename it inline (Enter saves, Esc cancels), drag the opacity slider from 0 to 100 %, lock a layer against painting, and click the blend chip to cycle Normal → Multiply → Screen. Every change is one undo step.",
                    "Locked layers refuse pencil, eraser, fill, clear and nudge, and merging into or out of a locked layer is blocked instead of quietly overwriting it.",
                    "New beta tools: turn the whole badge 90° clockwise (R) or anticlockwise (Shift+R) from the new toolbar button, and copy the finished 40 × 40 badge straight to the clipboard with the button next to Export PNG or Ctrl+Shift+C.",
                    "Settings: the version label in the header is no longer clipped — it was right-aligned into a box narrower than the text, which cut off the leading v."
                },
                new string[]
                {
                    "Der Ebenen-Bereich wurde neu gebaut (Beta): Füllmethode und Deckkraft sitzen jetzt unter dem Kopf wie in einem echten Ebenen-Bereich, jede Zeile zeigt Name, aktuelle Deckkraft in Prozent und ein Schloss, und die Aktionen bleiben unten fixiert.",
                    "Doppelklick auf eine Ebene benennt sie direkt um (Enter speichert, Esc bricht ab), der Deckkraft-Regler läuft von 0 bis 100 %, ein Schloss sperrt die Ebene gegen Malen, und ein Klick auf die Füllmethode schaltet Normal → Multiplizieren → Negativ multiplizieren. Jede Änderung ist ein Undo-Schritt.",
                    "Gesperrte Ebenen lehnen Stift, Radierer, Füllen, Leeren und Verschieben ab; Zusammenfassen mit einer gesperrten Ebene wird blockiert, statt sie still zu überschreiben.",
                    "Neue Beta-Werkzeuge: das ganze Badge um 90° im (R) oder gegen den Uhrzeigersinn (Shift+R) drehen und das fertige 40 × 40 Badge direkt in die Zwischenablage kopieren – per Schaltfläche neben PNG exportieren oder mit Strg+Shift+C.",
                    "Einstellungen: Die Versionsanzeige im Kopf wird nicht mehr abgeschnitten – sie war rechtsbündig in ein schmaleres Feld gezeichnet, wodurch das führende v verschwand."
                }),
            new ReleaseNote("1.4.5-beta.9", "26 Oct 2026",
                "Navy theme replaces Glass",
                "Marineblau ersetzt Glas",
                new string[]
                {
                    "The experimental Glass theme is gone. The third appearance option is now Navy: a proper dark palette in deep blue, built from exactly the same flat material tokens as Dark and Light — no glass lighting, no extra painter, nothing that only works in one theme.",
                    "It covers the whole app: window and app bar, editor and details cards, canvas frame, layers panel, colour picker, tooltips, toasts, menus, dialogs, the boot splash and the Settings and Changelog sheets.",
                    "Settings → Appearance shows it as the third tile (no BETA tag), it is saved as theme: navy in settings.json and survives restarts. A settings file that still says theme: glass loads as Navy, so nobody loses their choice.",
                    "--theme navy renders the palette for screenshots without touching the saved choice."
                },
                new string[]
                {
                    "Das experimentelle Glas-Design ist entfernt. Die dritte Darstellung ist jetzt Marineblau: ein richtiges dunkles Design in tiefem Blau, gebaut aus genau denselben flachen Farbwerten wie Dunkel und Hell – ohne Glas-Beleuchtung, ohne eigenen Zeichner und ohne Sonderfälle.",
                    "Es umfasst die ganze App: Fenster und Leiste, Editor- und Detail-Karten, Zeichenflächenrahmen, Ebenen-Bereich, Farbpalette, Tooltips, Hinweise, Menüs, Dialoge, den Startbildschirm sowie die Einstellungen und den Changelog.",
                    "In den Einstellungen unter Darstellung ist es die dritte Kachel (ohne BETA-Markierung), wird als theme: navy in settings.json gespeichert und bleibt nach einem Neustart erhalten. Eine Einstellungsdatei mit theme: glass wird als Marineblau geladen, niemand verliert seine Auswahl.",
                    "Mit --theme navy startet die App im Marineblau-Design, ohne die gespeicherte Auswahl zu ändern."
                }),
            new ReleaseNote("1.4.5-beta.8", "26 Oct 2026",
                "Glass theme, cleaner glass",
                "Glas-Design, klarer",
                new string[]
                {
                    "The Glass theme now reads as frosted glass on a dark window: the page is a clean cool slate instead of purple-tinted navy, and the panes sit a clear step lighter, so they no longer look like black slabs.",
                    "Each pane has a lit top edge that fades downward, a cool reflection drifting in from its top-right corner, a soft bloom around it and a wider sheen under the header. Toolbar groups use the same frosted step.",
                    "Recesses are drawn as smoked glass — the canvas frame, fields, the performance row and its switch are darker than a pane but lighter than the page, so nothing punches a hole through a card. The switch now has a light knob on a slate track.",
                    "Still a dark theme: text, checkerboard and the gold accent keep their contrast, and Dark and Light are unchanged."
                },
                new string[]
                {
                    "Das Glas-Design wirkt jetzt wie mattes Glas auf einem dunklen Fenster: Die Seite ist ein klares, kühles Schiefergrau statt violettem Marineblau, und die Flächen liegen deutlich heller darüber, sodass sie nicht mehr wie schwarze Blöcke wirken.",
                    "Jede Fläche hat eine beleuchtete Oberkante, die nach unten ausläuft, einen kühlen Reflex aus der oberen rechten Ecke, einen weichen Schein außen und einen breiteren Glanz unter dem Kopf. Werkzeuggruppen nutzen dieselbe matte Stufe.",
                    "Vertiefungen sind als Rauchglas gezeichnet – Zeichenflächenrahmen, Felder, die Leistungszeile und ihr Schalter sind dunkler als eine Fläche, aber heller als die Seite; nichts schneidet mehr ein Loch in eine Karte. Der Schalter hat jetzt einen hellen Knopf auf schieferfarbener Spur.",
                    "Weiterhin ein dunkles Design: Text, Schachbrett und der goldene Akzent behalten ihren Kontrast, Dunkel und Hell bleiben unverändert."
                }),
            new ReleaseNote("1.4.5-beta.7", "26 Oct 2026",
                "Performance switch fix",
                "Schalter-Korrektur",
                new string[]
                {
                    "The Performance mode switch is now drawn with the Settings sheet instead of as a separate control, so it no longer cuts a square block out of the rounded performance row or its border.",
                    "The whole row handles the click and lights up on hover, and the on/off states are drawn cleanly in dark, light and glass."
                },
                new string[]
                {
                    "Der Schalter für den Leistungsmodus wird jetzt mit den Einstellungen gezeichnet statt als eigenes Steuerelement. Dadurch schneidet er kein eckiges Stück mehr aus der abgerundeten Zeile oder ihrem Rahmen.",
                    "Die ganze Zeile nimmt den Klick an und leuchtet beim Überfahren auf; die An/Aus-Zustände sind in Dunkel, Hell und Glas sauber gezeichnet."
                }),
            new ReleaseNote("1.4.5-beta.6", "26 Oct 2026",
                "Glass theme (beta)",
                "Glas-Design (Beta)",
                new string[]
                {
                    "Settings now offers a third appearance tile: Glass, a dark glassmorphism palette with a lit navy backdrop, soft coloured light, frosted cards with bright 1px edges and a sheen across every card header. The tile is marked BETA.",
                    "Toolbar groups, the layers panel, menus and the Settings and Changelog sheets all follow the palette; the theme is stored in settings.json as theme: glass and survives restarts.",
                    "Start the app with --theme glass to render it for screenshots without touching the saved choice."
                },
                new string[]
                {
                    "In den Einstellungen gibt es jetzt eine dritte Darstellung: Glas, ein dunkles Glassmorphism-Design mit beleuchtetem Marineblau-Hintergrund, weichem farbigem Licht, matten Karten mit heller 1-px-Kante und einem Glanz über jedem Kartenkopf. Die Kachel ist als BETA markiert.",
                    "Werkzeuggruppen, der Ebenen-Bereich, Menüs sowie die Einstellungen und der Changelog folgen der Palette; das Design wird als theme: glass in settings.json gespeichert und bleibt nach einem Neustart erhalten.",
                    "Mit --theme glass startet die App im Glas-Design, ohne die gespeicherte Auswahl zu ändern."
                }),
            new ReleaseNote("1.4.5-beta.5", "26 Oct 2026",
                "Layer tools and a performance mode",
                "Ebenen-Werkzeuge und Leistungsmodus",
                new string[]
                {
                    "The layers panel has four new buttons next to add and delete: duplicate, merge down, move up and move down. Hover one to see its name and shortcut.",
                    "Ctrl+J duplicates the active layer, Ctrl+M merges it into the layer below, Alt+↑ and Alt+↓ move it up or down the stack. Each action is a single undo step.",
                    "Beta: Settings now has a Performance mode switch. It repaints only the pixels a stroke touched, keeps the checkerboard and grid in a cached bitmap, reuses the canvas surface and batches the large preview while you paint — less CPU and noticeably less input lag."
                },
                new string[]
                {
                    "Der Ebenen-Bereich hat vier neue Schaltflächen neben Hinzufügen und Löschen: Duplizieren, nach unten zusammenfassen, nach oben und nach unten verschieben. Beim Überfahren erscheinen Name und Kürzel.",
                    "Strg+J dupliziert die aktive Ebene, Strg+M fasst sie mit der darunter zusammen, Alt+↑ und Alt+↓ verschieben sie in der Reihenfolge. Jede Aktion ist ein einzelner Undo-Schritt.",
                    "Beta: In den Einstellungen gibt es jetzt den Leistungsmodus. Er zeichnet nur die Pixel neu, die ein Strich berührt hat, hält Schachbrett und Raster in einem Cache, verwendet die Zeichenfläche wieder und bündelt die große Vorschau beim Malen – weniger CPU und deutlich weniger Eingabeverzögerung."
                }),
            new ReleaseNote("1.4.5-beta.4", "26 Oct 2026",
                "Layers, guides and nudging",
                "Ebenen, Hilfslinien und Verschieben",
                new string[]
                {
                    "Beta: a Layers panel (toolbar button or Ctrl+L) with up to 5 layers, each with its own visibility, plus add, delete and flatten. Marks itself as beta in the panel header.",
                    "The guides button now cycles centre, rule of thirds and quarters instead of only the centre.",
                    "Arrow keys nudge the active layer by one pixel, Shift+arrow by five.",
                    "Hovering the BETA pill, or the taskbar entry, shows the version and build number."
                },
                new string[]
                {
                    "Beta: ein Ebenen-Bereich (Schaltfläche in der Leiste oder Strg+L) mit bis zu 5 Ebenen, jede einzeln ein- und ausblendbar, plus Hinzufügen, Löschen und Zusammenfassen. Der Kopf zeigt, dass es Beta ist.",
                    "Die Hilfslinien-Schaltfläche schaltet jetzt Mitte, Drittel und Viertel durch statt nur die Mitte.",
                    "Pfeiltasten verschieben die aktive Ebene um ein Pixel, Shift+Pfeil um fünf.",
                    "Beim Überfahren des BETA-Chips oder des Taskleisten-Eintrags erscheinen Version und Build-Nummer."
                }),
            new ReleaseNote("1.4.5-beta.3", "26 Oct 2026",
                "Clean card collapse",
                "Sauberes Ein- und Ausklappen",
                new string[]
                {
                    "Saving or deleting a badge no longer leaves card borders, labels or field outlines behind when My badges collapses or expands.",
                    "The whole surface repaints whenever the card layout changes, so the switch looks instant and clean."
                },
                new string[]
                {
                    "Beim Speichern oder Löschen eines Badges bleiben keine Kartenränder, Beschriftungen oder Feldumrisse mehr zurück, wenn Meine Badges ein- oder ausgeklappt wird.",
                    "Bei jeder Layout-Änderung wird die ganze Fläche neu gezeichnet, dadurch wirkt der Wechsel sauber."
                }),
            new ReleaseNote("1.4.5-beta.2", "26 Oct 2026",
                "Flip the canvas",
                "Zeichenfläche spiegeln",
                new string[]
                {
                    "Two new buttons beside the mirror toggle flip the whole badge left/right or top/bottom.",
                    "Shift+H and Shift+V do the same from the keyboard, and a flip is a single undo step."
                },
                new string[]
                {
                    "Zwei neue Schalter neben dem Spiegelzeichnen drehen das ganze Badge waagerecht oder senkrecht.",
                    "Shift+H und Shift+V tun dasselbe über die Tastatur; ein Spiegeln ist ein einzelner Undo-Schritt."
                }),
            new ReleaseNote("1.4.5-beta.1", "26 Oct 2026",
                "Beta branch",
                "Beta-Zweig",
                new string[]
                {
                    "This build is the beta channel: it keeps its own data folder, so experiments never reach the stable app."
                },
                new string[]
                {
                    "Dieser Build ist der Beta-Kanal: er nutzt einen eigenen Datenordner, damit Experimente nie die stabile App erreichen."
                }),
            new ReleaseNote("1.4.5", "26 Oct 2026",
                "Centre guides",
                "Mittellinien",
                new string[]
                {
                    "A new toggle beside the pixel grid draws a horizontal and a vertical guide through the exact centre of the canvas.",
                    "The C key toggles it while you draw, and the guides sit on the pixel boundary so both halves stay symmetrical."
                },
                new string[]
                {
                    "Ein neuer Schalter neben dem Pixelraster zeichnet je eine waagerechte und eine senkrechte Linie durch die exakte Mitte der Zeichenfläche.",
                    "Die Taste C schaltet sie beim Zeichnen um; die Linien liegen auf der Pixelgrenze, damit beide Hälften symmetrisch bleiben."
                }),
            new ReleaseNote("1.4.4", "26 Oct 2026",
                "Direct art assets",
                "Direkt zu den Art Assets",
                new string[]
                {
                    "The Developer Portal button opens your app's Rich Presence → Art Assets page when a valid Application ID is entered.",
                    "Without an ID it still opens the application list."
                },
                new string[]
                {
                    "Der Developer-Portal-Knopf öffnet direkt die Seite Rich Presence → Art Assets deiner Anwendung, sobald eine gültige Application-ID eingetragen ist.",
                    "Ohne ID öffnet er weiterhin die Liste der Anwendungen."
                }),
            new ReleaseNote("1.4.3", "26 Oct 2026",
                "Discord icon",
                "Discord-Icon",
                new string[]
                {
                    "The Developer Portal button in Settings wears the Discord mark instead of a generic picture icon."
                },
                new string[]
                {
                    "Der Developer-Portal-Knopf in den Einstellungen trägt jetzt das Discord-Zeichen statt eines allgemeinen Bild-Symbols."
                }),
            new ReleaseNote("1.4.2", "26 Oct 2026",
                "Crisp changelog text",
                "Scharfer Changelog-Text",
                new string[]
                {
                    "Release-note text renders crisply in the light theme instead of ghosting.",
                    "The notes scroll inside their own viewport, so they clip cleanly under the header."
                },
                new string[]
                {
                    "Der Text im Changelog wird im hellen Design jetzt scharf dargestellt statt zu schimmern.",
                    "Die Einträge scrollen in einem eigenen Bereich und werden sauber unter der Kopfzeile abgeschnitten."
                }),
            new ReleaseNote("1.4.1", "26 Oct 2026",
                "One settings chip",
                "Ein Einstellungen-Chip",
                new string[]
                {
                    "The Discord chip and the settings gear are now a single Settings chip in the app bar.",
                    "The Discord connection state stays visible as a small dot on the Settings chip.",
                    "Hovering that chip spells the state out in words, e.g. Connected or Discord is not running."
                },
                new string[]
                {
                    "Der Discord-Chip und das Zahnrad sind jetzt ein gemeinsamer Einstellungen-Chip in der Kopfleiste.",
                    "Der Discord-Verbindungsstatus bleibt als kleiner Punkt am Einstellungen-Chip sichtbar.",
                    "Beim Überfahren des Chips steht der Status als Text da, z. B. Verbunden oder Discord läuft nicht."
                }),
            new ReleaseNote("1.4.0", "26 Oct 2026",
                "Light theme & settings",
                "Helles Design & Einstellungen",
                new string[]
                {
                    "New light theme with a visual picker in Settings — dark stays the default.",
                    "Settings button in the app bar, right next to the Discord chip.",
                    "The startup splash and every sheet follow the selected theme."
                },
                new string[]
                {
                    "Neues helles Design mit Bildauswahl in den Einstellungen – dunkel bleibt Standard.",
                    "Einstellungen-Button in der Kopfleiste, direkt neben Discord.",
                    "Startbildschirm und alle Fenster folgen dem gewählten Design."
                }),
            new ReleaseNote("1.3.0", "26 Oct 2026",
                "Boot screen & changelog",
                "Startbildschirm & Changelog",
                new string[]
                {
                    "New startup splash with the HABBO logo fading in and out.",
                    "In-app changelog with the current version, one click from the header.",
                    "New footer credit: made by Jumayusi, not affiliated with Habbo or Sulake."
                },
                new string[]
                {
                    "Neuer Startbildschirm mit ein- und ausblendendem HABBO-Logo.",
                    "Integriertes Changelog mit aktueller Version, direkt aus der Kopfleiste.",
                    "Neuer Footer-Hinweis: von Jumayusi, nicht mit Habbo oder Sulake verbunden."
                }),
            new ReleaseNote("1.2.0", "26 Oct 2026",
                "Tooltips & flexible layout",
                "Tooltips & flexibles Layout",
                new string[]
                {
                    "Tooltips with keyboard shortcuts on every drawing tool.",
                    "More breathing room around the HABBO logo in the app bar.",
                    "My badges collapses when empty so the details card can expand."
                },
                new string[]
                {
                    "Tooltips mit Tastenkürzeln für alle Zeichenwerkzeuge.",
                    "Mehr Platz um das HABBO-Logo in der Kopfleiste.",
                    "„Meine Badges“ klappt bei leerer Liste zusammen, die Detailkarte wächst."
                }),
            new ReleaseNote("1.1.0", "26 Oct 2026",
                "Modern material redesign",
                "Modernes Material-Design",
                new string[]
                {
                    "Flatter, more compact interface built from quiet surface cards.",
                    "Rounded borderless window with a matching settings sheet.",
                    "Smaller default window and tighter, clearer spacing."
                },
                new string[]
                {
                    "Flachere, kompaktere Oberfläche aus ruhigen Karten.",
                    "Abgerundetes, rahmenloses Fenster mit passendem Einstellungsfenster.",
                    "Kleineres Standardfenster und klarere Abstände."
                }),
            new ReleaseNote("1.0.0", "26 Oct 2026",
                "First release",
                "Erste Version",
                new string[]
                {
                    "40 × 40 pixel editor with pencil, eraser, fill, picker, mirror and grid.",
                    "Transparent PNG export, image import, badge library and autosave.",
                    "English / German interface and Discord Rich Presence."
                },
                new string[]
                {
                    "40 × 40-Pixel-Editor mit Stift, Radierer, Füllen, Pipette, Spiegel und Raster.",
                    "Transparenter PNG-Export, Bildimport, Badge-Bibliothek und Autospeichern.",
                    "Deutsche und englische Oberfläche sowie Discord Rich Presence."
                })
        };
    }

    internal class ReleaseNote
    {
        public readonly string Version, Date, TitleEn, TitleDe;
        public readonly string[] BulletsEn, BulletsDe;

        public ReleaseNote(string version, string date, string titleEn, string titleDe,
            string[] bulletsEn, string[] bulletsDe)
        {
            Version = version;
            Date = date;
            TitleEn = titleEn;
            TitleDe = titleDe;
            BulletsEn = bulletsEn;
            BulletsDe = bulletsDe;
        }

        public string Title { get { return L.De ? TitleDe : TitleEn; } }
        public string[] Bullets { get { return L.De ? BulletsDe : BulletsEn; } }
    }

    /// <summary>Scrollable release-notes sheet, opened from the header chip.</summary>
    internal class ChangelogForm : Form
    {
        WinBtn closeBtn;
        ReleaseList list;

        public ChangelogForm()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = T.Card;
            ClientSize = new Size(T.P(640), T.P(560));
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            list = new ReleaseList();
            list.BackColor = BackColor;
            Controls.Add(list);

            closeBtn = new WinBtn();
            closeBtn.Icon = "close";
            closeBtn.BackColor = T.Card;
            closeBtn.Click += delegate { Close(); };
            Controls.Add(closeBtn);
            closeBtn.SetBounds(ClientSize.Width - T.P(40), T.P(8), T.P(32), T.P(28));

            LayoutAll();
            list.Prepare();
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
            SheetFade.Attach(this);
        }

        /// <summary>Paints the entries before the window appears, so no frame shows a bare body.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            list.Refresh();
        }

        void LayoutAll()
        {
            if (list == null) return;
            list.SetBounds(0, T.P(76), ClientSize.Width,
                Math.Max(T.P(40), ClientSize.Height - T.P(76) - T.P(14)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            T.PaintSheet(g, ClientRectangle, BackColor);

            int pad = T.P(22), tile = T.P(30);
            T.LogoTile(g, new RectangleF(pad, T.P(15), tile, tile), T.F(7f));
            T.Str(g, L.T("Changelog", "Änderungen"), T.FCardTitle, T.Text,
                new Rectangle(pad + tile + T.P(11), T.P(13), T.P(360), T.P(22)), T.FL);
            T.Str(g, L.T("What's new in the app", "Was sich in der App geändert hat"), T.FSmall, T.Muted,
                new Rectangle(pad + tile + T.P(11), T.P(34), T.P(360), T.P(18)), T.FL);
            DrawVersionPill(g, ClientSize.Width - T.P(52), T.P(18));
            T.HLine(g, 0, ClientSize.Width, T.P(64), T.LineSoft);

            using (GraphicsPath rim = T.Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), T.F(12f)))
            using (Pen pen = new Pen(T.Stroke, 1f))
                g.DrawPath(pen, rim);
        }

        void DrawVersionPill(Graphics g, int right, int y)
        {
            string label = L.T("CURRENT BUILD", "AKTUELLER STAND");
            int w = T.Meas(label, T.FMicro).Width + T.P(20);
            int x = right - w;
            RectangleF r = new RectangleF(x, y, w, T.P(22));
            T.FillRound(g, r, T.F(11f), T.PillBg);
            T.StrokeRound(g, r, T.F(11f), T.PillStroke, 1f);
            T.Str(g, label, T.FMicro, T.Teal, new Rectangle(x, y, w, T.P(22)), T.FC);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (list != null) list.ScrollBy(-e.Delta / 3);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutAll();
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Escape ||
                (keyData & Keys.KeyCode) == Keys.Enter ||
                (keyData & Keys.KeyCode) == Keys.Space)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Y < T.P(58))
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

    /// <summary>
    /// Every entry is laid out once into an opaque bitmap of its own and rendered the first time
    /// the viewport needs it, so opening the sheet costs one screenful instead of the whole
    /// history, and scrolling is a blit instead of re-wrapping every entry on every wheel notch.
    /// The slices have a filled background on purpose: a transparent buffer made ClearType glyphs
    /// ghost in the light theme.
    /// </summary>
    internal class ReleaseList : Control
    {
        int scroll;
        int contentH = -1;
        bool hotBar, dragBar;
        int dragGrab;

        Bitmap[] slices;   // index 0 is the version card, 1..n mirror AppInfo.Releases
        int[] tops;        // content y where each slice starts
        int sliceKey;
        int targetScroll;
        bool gliding;
        readonly Timer glide = new Timer();

        public ReleaseList()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            TabStop = false;
            glide.Interval = 15;
            glide.Tick += delegate { GlideTick(); };
        }

        int ContentWidth { get { return Width - T.P(44) - T.P(10); } }

        /// <summary>Scrolls by a wheel notch or a page, easing into the new position.</summary>
        public void ScrollBy(int delta)
        {
            int from = gliding ? targetScroll : scroll;
            int to = Math.Max(0, Math.Min(MaxScroll, from + delta));
            if (to == scroll && !gliding) return;
            targetScroll = to;
            if (gliding) return;
            gliding = true;
            glide.Start();
        }

        void SetScrollNow(int value)
        {
            if (gliding) { gliding = false; glide.Stop(); }
            int ns = Math.Max(0, Math.Min(MaxScroll, value));
            if (ns == scroll) return;
            scroll = ns;
            Invalidate();
        }

        void GlideTick()
        {
            int diff = targetScroll - scroll;
            if (diff == 0) { gliding = false; glide.Stop(); return; }
            int step = (int)Math.Round(diff * 0.30);
            if (step == 0) step = diff > 0 ? 1 : -1;
            scroll += step;
            Invalidate();
            if (Math.Abs(targetScroll - scroll) <= 1)
            {
                scroll = targetScroll;
                gliding = false;
                glide.Stop();
                Invalidate();
            }
        }

        int MaxScroll { get { return Math.Max(0, contentH - Height); } }

        int ContentKey()
        {
            unchecked
            {
                int k = Width;
                k = k * 397 ^ (int)T.Mode;
                k = k * 397 ^ BackColor.ToArgb();
                k = k * 397 ^ (L.De ? 1 : 0);
                return k;
            }
        }

        int BulletHeight(string bullet, int width)
        {
            Size sz = TextRenderer.MeasureText(bullet, T.FSmall, new Size(width, T.P(400)),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(T.P(16), sz.Height);
        }

        int ReleaseHeight(ReleaseNote r)
        {
            int w = ContentWidth - T.P(16);
            int h = T.P(43) + T.P(16);
            foreach (string bullet in r.Bullets) h += BulletHeight(bullet, w) + T.P(6);
            return h;
        }

        int SliceBottom(int i)
        {
            return i + 1 < tops.Length ? tops[i + 1] : contentH - T.P(8);
        }

        /// <summary>Measures the whole history once; the entries themselves render on demand.</summary>
        void EnsureLayout()
        {
            int key = ContentKey();
            if (tops != null && sliceKey == key) return;
            DisposeSlices();
            int n = AppInfo.Releases.Length + 1;
            int[] places = new int[n];
            int y = T.P(8);
            places[0] = y;
            y += T.P(72) + T.P(18);
            for (int i = 1; i < n; i++)
            {
                places[i] = y;
                y += ReleaseHeight(AppInfo.Releases[i - 1]);
            }
            contentH = Math.Max(1, y + T.P(8));
            tops = places;
            slices = new Bitmap[n];
            sliceKey = key;
            scroll = Math.Min(scroll, MaxScroll);
        }

        /// <summary>Renders one entry into its own bitmap the first time the viewport needs it.</summary>
        Bitmap Slice(int i)
        {
            if (slices[i] != null) return slices[i];
            int w = Math.Max(1, Width);
            int h = Math.Max(1, SliceBottom(i) - tops[i]);
            Bitmap b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                using (SolidBrush bg = new SolidBrush(BackColor))
                    g.FillRectangle(bg, 0, 0, w, h);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (i == 0) DrawVersionCard(g, 0);
                else DrawRelease(g, 0, AppInfo.Releases[i - 1]);
            }
            slices[i] = b;
            return b;
        }

        /// <summary>Blits every entry the viewport covers.</summary>
        void PaintEntries(Graphics g)
        {
            if (tops == null) return;
            int top = scroll, bottom = scroll + Height;
            for (int i = 0; i < tops.Length; i++)
            {
                if (SliceBottom(i) <= top) continue;
                if (tops[i] >= bottom) break;
                Bitmap b = Slice(i);
                int y = tops[i] - scroll;
                g.DrawImage(b, new Rectangle(0, y, b.Width, b.Height),
                    new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
            }
        }

        /// <summary>Renders one entry beyond each edge, so a scroll step is always a blit.</summary>
        void Prefetch()
        {
            if (tops == null) return;
            int top = scroll, bottom = scroll + Height;
            for (int i = 0; i < tops.Length; i++)
            {
                if (tops[i] < bottom || slices[i] != null) continue;
                Slice(i);
                break;
            }
            for (int i = tops.Length - 1; i >= 0; i--)
            {
                if (SliceBottom(i) > top || slices[i] != null) continue;
                Slice(i);
                break;
            }
        }

        /// <summary>Warms the entry cache before the sheet is shown, so the first paint is a blit.</summary>
        public void Prepare()
        {
            if (Width <= 0 || Height <= 0) return;
            EnsureLayout();
            for (int i = 0; i < tops.Length; i++)
            {
                if (SliceBottom(i) <= scroll) continue;
                if (tops[i] >= scroll + Height * 2) break;
                Slice(i);
            }
        }

        /// <summary>Drops the rendered entries; the next paint re-measures and re-renders.</summary>
        void DisposeSlices()
        {
            if (slices != null)
            {
                foreach (Bitmap b in slices) if (b != null) b.Dispose();
                slices = null;
            }
            tops = null;
            contentH = -1;
            sliceKey = 0;
        }

        int BarHeight
        {
            get { return Math.Max(T.P(28), (int)((float)Height * Height / Math.Max(Height, contentH))); }
        }

        /// <summary>Generous hit area, since the drawn thumb is only a few pixels wide.</summary>
        Rectangle ThumbHit
        {
            get
            {
                if (contentH <= Height) return Rectangle.Empty;
                int barH = BarHeight;
                int travel = Height - barH;
                int max = Math.Max(1, MaxScroll);
                int barY = travel <= 0 ? 0 : (int)((float)scroll * travel / max);
                return new Rectangle(Width - T.P(12), barY, T.P(12), barH);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(BackColor)) g.FillRectangle(bg, ClientRectangle);
            EnsureLayout();
            PaintEntries(g);
            Prefetch();
            DrawBar(g);
        }

        void DrawBar(Graphics g)
        {
            if (contentH <= Height) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool active = hotBar || dragBar;
            int barH = BarHeight;
            int max = Math.Max(1, MaxScroll);
            int barY = (int)((float)scroll * (Height - barH) / max);
            float bw = active ? T.F(6f) : T.F(4f);
            float bx = active ? Width - T.P(11) : Width - T.P(10);
            Color barColor = active ? T.Mix(T.ScrollBar, T.Text, 0.22f) : T.ScrollBar;
            T.FillRound(g, new RectangleF(bx, barY, bw, barH), bw / 2f, barColor);
        }

        void InvalidateBar()
        {
            Invalidate(new Rectangle(Width - T.P(16), 0, T.P(16), Height));
        }

        int DrawVersionCard(Graphics g, int y)
        {
            int pad = T.P(22);
            RectangleF card = new RectangleF(pad, y, Width - pad * 2, T.P(72));
            T.FillRound(g, card, T.F(10f), T.Segment);
            T.StrokeRound(g, card, T.F(10f), T.Stroke, 1f);
            T.Str(g, "Habbo Badge Creator", T.FSmall, T.Muted,
                new Rectangle(pad + T.P(14), y + T.P(10), T.P(300), T.P(16)), T.FL);
            T.Str(g, L.T("Version ", "Version ") + AppInfo.Version, T.FPageTitle, T.Text,
                new Rectangle(pad + T.P(14), y + T.P(27), T.P(300), T.P(28)), T.FL);
            T.Str(g, AppInfo.Date + "  ·  " + L.T("current build", "aktuelle Version"), T.FMicro, T.Dim,
                new Rectangle(pad + T.P(14), y + T.P(53), T.P(400), T.P(14)), T.FL);
            return y + T.P(72) + T.P(18);
        }

        int DrawRelease(Graphics g, int y, ReleaseNote r)
        {
            int pad = T.P(22);
            T.Str(g, "v" + r.Version, T.FBtn, T.Gold,
                new Rectangle(pad, y, T.P(120), T.P(18)), T.FL);
            T.Str(g, r.Date, T.FMicro, T.Dim,
                new Rectangle(Width - T.P(170), y + T.P(2), T.P(126), T.P(16)), T.FR);
            T.Str(g, r.Title, T.FBody, T.Text,
                new Rectangle(pad, y + T.P(19), ContentWidth, T.P(20)), T.FL);

            int by = y + T.P(43);
            foreach (string bullet in r.Bullets)
            {
                int w = ContentWidth - T.P(16);
                int h = BulletHeight(bullet, w);
                using (SolidBrush dot = new SolidBrush(T.Gold))
                    g.FillEllipse(dot, pad + T.P(2), by + T.P(6), T.P(4), T.P(4));
                T.Wrap(g, bullet, T.FSmall, T.Muted, new Rectangle(pad + T.P(16), by, w, h + T.P(2)));
                by += h + T.P(6);
            }
            return by + T.P(16);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Rectangle thumb = ThumbHit;
            if (thumb.IsEmpty) return;
            if (thumb.Contains(e.Location))
            {
                dragBar = true;
                dragGrab = e.Y - thumb.Y;
                Capture = true;
                Invalidate();
            }
            else if (e.X >= Width - T.P(16))
            {
                ScrollBy(e.Y < thumb.Y ? -(Height - T.P(36)) : Height - T.P(36));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragBar)
            {
                int travel = Height - BarHeight;
                if (travel <= 0) return;
                int y = Math.Max(0, Math.Min(travel, e.Y - dragGrab));
                SetScrollNow((int)Math.Round((double)y * MaxScroll / travel));
                return;
            }
            bool hot = !ThumbHit.IsEmpty && e.X >= Width - T.P(16) && ThumbHit.Contains(e.Location);
            if (hot != hotBar) { hotBar = hot; InvalidateBar(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!dragBar) return;
            dragBar = false;
            Capture = false;
            InvalidateBar();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (dragBar || !hotBar) return;
            hotBar = false;
            InvalidateBar();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollBy(-e.Delta / 3);
            HandledMouseEventArgs handled = e as HandledMouseEventArgs;
            if (handled != null) handled.Handled = true;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (gliding) { gliding = false; glide.Stop(); }
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                glide.Stop();
                glide.Dispose();
                DisposeSlices();
            }
            base.Dispose(disposing);
        }
    }
}
