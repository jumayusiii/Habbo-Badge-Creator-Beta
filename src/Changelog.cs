using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HabboBadgeCreator
{
    /// <summary>Version, credit and the release history shown in the changelog.</summary>
    internal static class AppInfo
    {
        public const string Version = "1.4.5-beta.14";
        public const string Build = "2026-10-26.14";
        public const string Date = "26 October 2026";
        public const string Credit = "Habbo Badge Creator © 2026 Jumayusi — Not affiliated with Habbo or Sulake";

        public static readonly ReleaseNote[] Releases = new ReleaseNote[]
        {
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            using (GraphicsPath p = T.Round(new RectangleF(0, 0, Width, Height), T.F(12f)))
                Region = new Region(p);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>
    /// The release notes scroll inside their own control, so the window clips them the way a
    /// real viewport does. (GDI text ignores Graphics clip paths, and blitting a transparent
    /// offscreen buffer made the light theme's glyphs ghost.)
    /// </summary>
    internal class ReleaseList : Control
    {
        int scroll;
        int contentH = -1;

        public ReleaseList()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = T.Card;
            TabStop = false;
        }

        int ContentWidth { get { return Width - T.P(44) - T.P(10); } }

        public void ScrollBy(int delta)
        {
            int max = Math.Max(0, contentH - Height);
            int ns = Math.Max(0, Math.Min(max, scroll + delta));
            if (ns == scroll) return;
            scroll = ns;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            T.PaintSheet(g, ClientRectangle, BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int y = T.P(8) - scroll;
            y = DrawVersionCard(g, y);
            foreach (ReleaseNote r in AppInfo.Releases)
                y = DrawRelease(g, y, r);
            contentH = y + scroll + T.P(8);

            if (contentH > Height)
            {
                int barH = Math.Max(T.P(28), (int)((float)Height * Height / contentH));
                int max = Math.Max(1, contentH - Height);
                int barY = (int)((float)scroll * (Height - barH) / max);
                T.FillRound(g, new RectangleF(Width - T.P(8), barY, T.P(4), barH), T.F(2f), T.ScrollBar);
            }
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
                Size sz = TextRenderer.MeasureText(bullet, T.FSmall, new Size(w, T.P(400)),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                int h = Math.Max(T.P(16), sz.Height);
                using (SolidBrush dot = new SolidBrush(T.Gold))
                    g.FillEllipse(dot, pad + T.P(2), by + T.P(6), T.P(4), T.P(4));
                T.Wrap(g, bullet, T.FSmall, T.Muted, new Rectangle(pad + T.P(16), by, w, h + T.P(2)));
                by += h + T.P(6);
            }
            return by + T.P(16);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            contentH = -1;
            Invalidate();
        }
    }
}
